using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace RakDotNet.TcpUdp
{
    public class TcpUdpConnection : IRakConnection
    {
        private readonly X509Certificate _cert;

        private readonly TcpClient _tcp;
        private readonly CancellationTokenSource _tcpCts;
        private readonly SemaphoreSlim _tcpReceiveLock;
        private int _cumulativePing;

        private int _lastPing;
        private int _pingCount;

        private int _pingTimer;
        private bool _sslAuthenticated;

        private Stream _tcpStream;

        private readonly Timer _timer;

        private readonly object _sendLock;

        internal TcpUdpConnection(TcpClient tcp, X509Certificate certificate = null, int pingInterval = 5000)
        {
            _sendLock = new object();
            
            _tcp = tcp;

            _cert = certificate;
            _tcpReceiveLock = new SemaphoreSlim(1, 1);
            _tcpCts = new CancellationTokenSource();

            _tcpStream = _tcp.GetStream();
            
            _sslAuthenticated = false;

            _pingTimer = 0;
            _lastPing = 0;
            _pingCount = 0;
            _cumulativePing = 0;

            _timer = new Timer(pingInterval)
            {
                AutoReset = true
            };

            _timer.Elapsed += PingTimerElapsed;
        }

        public event Func<byte[], Task> MessageReceived;
        public event Func<CloseReason, Task> Disconnected;

        public int AveragePing => _pingCount > 0 && _cumulativePing > 0 ? _cumulativePing / _pingCount : -1;
        public IPEndPoint EndPoint => _tcp.GetRemoteEndPoint();

        public async Task CloseAsync()
        {
            if (!_tcp.Connected)
                throw new InvalidOperationException("Connection is closed!");

            await _tcpStream.DisposeAsync();
            _timer.Stop();
            _tcpCts.Cancel();
            _tcp.Close();

            await DisconnectInternalAsync(CloseReason.ForceDisconnect);
        }

        public void Send(ReadOnlySpan<byte> buf)
        {
            lock (_sendLock)
            {
                _tcpStream.Write(BitConverter.GetBytes((uint) buf.Length));
                _tcpStream.Write(buf);
            }
        }

        public void Send(Span<byte> buf)
            => Send((ReadOnlySpan<byte>) buf);

        public void Send(byte[] buf, int index, int length)
            => Send(new ReadOnlySpan<byte>(buf, index, length));

        public override string ToString()
            => EndPoint.ToString();

        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case IPEndPoint endPoint:
                    return endPoint.Equals(EndPoint);
                case IRakConnection rakConnection:
                    return rakConnection.EndPoint.Equals(EndPoint);
                default:
                    return false;
            }
        }

        protected bool Equals(TcpUdpConnection other)
            => other.EndPoint.Equals(EndPoint);

        public override int GetHashCode()
            => EndPoint.GetHashCode();

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

                    _pingTimer += TcpUdpServer.LoopDelay;
                    
                    await _tcpReceiveLock.WaitAsync(cancelToken).ConfigureAwait(false);
                    
                    try
                    {
                        var packetLenBuffer = new byte[4];

                        await _tcpStream.ReadAsync(packetLenBuffer, cancelToken).ConfigureAwait(false);

                        var packetBuffer = new byte[BitConverter.ToInt32(packetLenBuffer)];

                        await _tcpStream.ReadAsync(packetBuffer, cancelToken).ConfigureAwait(false);

                        var _ = Task.Run(() => OnPacket(packetBuffer), cancelToken);
                    }
                    finally
                    {
                        _tcpReceiveLock.Release();
                    }
                }
            }, cancelToken);
        }

        private void OnPacket(byte[] buf)
        {
            switch ((MessageIdentifier) buf[0])
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

            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream);
            
            reader.ReadByte();

            var old = reader.ReadUInt32();

            _lastPing = (int) (curr - old);
            _cumulativePing += _lastPing;
            _pingCount++;
        }

        private void DoPong(byte[] data)
        {
            using var stream = new MemoryStream(1 + 4 + 4);
            using var writer = new BinaryWriter(stream);
            
            writer.Write((byte) MessageIdentifier.ConnectedPong);
            writer.Write(new ArraySegment<byte>(data, 1, 4));
            writer.Write(0u);

            Send(stream.ToArray());
        }

        private void DoPing()
        {
            using var stream = new MemoryStream(1 + 4);
            using var writer = new BinaryWriter(stream);
            
            writer.Write((byte) MessageIdentifier.InternalPing);
            writer.Write((uint) _pingTimer);

            Send(stream.ToArray());
        }

        private void PingTimerElapsed(object sender, ElapsedEventArgs args)
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
    }
}