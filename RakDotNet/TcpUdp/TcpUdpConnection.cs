using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;

namespace RakDotNet.TcpUdp
{
    public class TcpUdpConnection : IRakConnection
    {
        public const int PING_INTERVAL = 5000;

        public event Func<byte[], Task> MessageReceived;
        public event Func<CloseReason, Task> Disconnected;

        private readonly TcpClient _tcp;
        private readonly X509Certificate _cert;
        private readonly SemaphoreSlim _tcpReceiveLock;
        private readonly CancellationTokenSource _tcpCts;

        private Stream _tcpStream;

        private uint _curPacketLength;
        private bool _sslAuthenticated;

        private int _pingTimer;
        private int _lastPing;
        private int _pingCount;
        private int _cumulativePing;

        private Timer _timer;

        public int AveragePing => _pingCount > 0 && _cumulativePing > 0 ? _cumulativePing / _pingCount : -1;
        public IPEndPoint EndPoint => _tcp.GetRemoteEndPoint();

        internal TcpUdpConnection(TcpClient tcp, X509Certificate certificate = null)
        {
            _tcp = tcp;
            _cert = certificate;
            _tcpReceiveLock = new SemaphoreSlim(1, 1);
            _tcpCts = new CancellationTokenSource();

            _tcpStream = _tcp.GetStream();

            _curPacketLength = 0;
            _sslAuthenticated = false;

            _pingTimer = 0;
            _lastPing = 0;
            _pingCount = 0;
            _cumulativePing = 0;

            _timer = new Timer(PING_INTERVAL)
            {
                AutoReset = true
            };

            _timer.Elapsed += PingTimerElapsed;
        }

        internal async Task RunAsync()
        {
            if (_cert != null && !_sslAuthenticated)
                await AuthenticateSslAsync();

            var tasks = new[]
            {
                RunReceiveTcpAsync(_tcpCts.Token)
            };

            await Task.WhenAny(tasks).ConfigureAwait(false);
        }

        private async Task AuthenticateSslAsync()
        {
            var sslStream = new SslStream(_tcpStream);

            await sslStream.AuthenticateAsServerAsync(_cert).ConfigureAwait(false);

            _sslAuthenticated = true;

            _tcpStream = sslStream;
        }

        private Task RunReceiveTcpAsync(CancellationToken cancelToken)
        {
            return Task.Run(async () =>
            {
                _timer.Start();

                while (true)
                {
                    cancelToken.ThrowIfCancellationRequested();

                    _pingTimer += 20;

                    await ReceiveTcpAsync(_tcpStream, cancelToken).ConfigureAwait(false);
                }
            });
        }

        private async Task ReceiveTcpAsync(Stream stream, CancellationToken cancelToken)
        {
            await _tcpReceiveLock.WaitAsync(cancelToken).ConfigureAwait(false);

            try
            {
                do
                {
                    if (_curPacketLength == 0 && _tcp.Available >= 4)
                    {
                        var packetLenBuffer = new byte[4];

                        await stream.ReadAsync(packetLenBuffer, cancelToken).ConfigureAwait(false);

                        _curPacketLength = BitConverter.ToUInt32(packetLenBuffer);
                    }
                }
                while (_curPacketLength == 0 || _curPacketLength > _tcp.Available);

                var packetBuffer = new byte[_curPacketLength];

                await stream.ReadAsync(packetBuffer, cancelToken).ConfigureAwait(false);

                _curPacketLength = 0;

                OnPacket(packetBuffer);
            }
            finally
            {
                _tcpReceiveLock.Release();
            }
        }

        private void OnPacket(byte[] buf)
        {
            switch ((MessageIdentifier)buf[0])
            {
                case MessageIdentifier.InternalPing:
                    DoPong(buf);
                    break;

                case MessageIdentifier.ConnectedPong:
                    OnPong(buf);
                    break;

                default:
                    var _ = MessageReceived?.Invoke(buf);
                    break;
            }
        }

        private void OnPong(byte[] data)
        {
            var curr = _pingTimer;

            using (var stream = new MemoryStream(data))
            using (var reader = new BinaryReader(stream))
            {
                reader.ReadByte();

                var old = reader.ReadUInt32();

                _lastPing = (int)(curr - old);
                _cumulativePing += _lastPing;
                _pingCount++;
            }
        }

        private void DoPong(byte[] data)
        {
            using (var stream = new MemoryStream(1 + 4 + 4))
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write((byte)MessageIdentifier.ConnectedPong);
                writer.Write(new ArraySegment<byte>(data, 1, 4));
                writer.Write(0u);

                Send(stream.ToArray());
            }
        }

        private void DoPing()
        {
            using (var stream = new MemoryStream(1 + 4))
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write((byte)MessageIdentifier.InternalPing);
                writer.Write((uint)_pingTimer);

                Send(stream.ToArray());
            }
        }

        private void PingTimerElapsed(object sender, System.Timers.ElapsedEventArgs args)
        {
            try
            {
                DoPing();
            }
            catch (IOException)
            {
                _timer.Close();
                _tcpCts.Cancel();

                var _ = DisconnectInternalAsync(CloseReason.ClientDisconnect);
            }
        }

        private async Task DisconnectInternalAsync(CloseReason reason)
        {
            if (Disconnected != null)
                await Disconnected(reason).ConfigureAwait(false);
        }

        public async Task CloseAsync()
        {
            if (!_tcp.Connected)
                throw new InvalidOperationException("Connection is closed!");

            _tcp.Close();

            await DisconnectInternalAsync(CloseReason.ForceDisconnect);
        }

        public void Send(ReadOnlySpan<byte> buf)
        {
            using (var writer = new BinaryWriter(_tcpStream, Encoding.UTF8, true))
            {
                writer.Write(buf.Length);
                writer.Write(buf);
            }
        }

        public void Send(Span<byte> buf) => Send((ReadOnlySpan<byte>)buf);

        public void Send(byte[] buf, int index, int length) => Send(new ReadOnlySpan<byte>(buf, index, length));
    }
}
