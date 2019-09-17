using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace RakDotNet.TcpUdp
{
    public class TcpUdpServer : IRakServer
    {
        private readonly X509Certificate _cert;

        private readonly byte[] _password;
        private readonly ConcurrentDictionary<IPEndPoint, ConnectionEntry> _tcpConnections;

        private readonly TcpListener _tcpServer;
        private readonly UdpClient _udpClient;
        
        private readonly SemaphoreSlim _udpReceiveLock;
        
        private uint _recvSeqNum;
        private uint _sendSeqNum;

        private Task _tcpAcceptTask;
        private Task _udpRecvTask;

        private bool _tcpStarted;

        public TcpUdpServer(int port, string password, X509Certificate cert = null)
        {
            _cert = cert;

            _udpClient = new UdpClient(port);
            _udpReceiveLock = new SemaphoreSlim(1, 1);

            _tcpServer = new TcpListener(IPAddress.Any, port);
            _tcpConnections = new ConcurrentDictionary<IPEndPoint, ConnectionEntry>();

            _password = new byte[password.Length];

            for (var i = 0; i < password.Length; i++) _password[i] = (byte) password[i];

            _tcpStarted = false;

            _sendSeqNum = 0;
            _recvSeqNum = 0;
        }

        public event Func<IPEndPoint, byte[], Reliability, Task> MessageReceived;
        public event Func<IPEndPoint, Task> ClientConnected;
        public event Func<IPEndPoint, CloseReason, Task> ClientDisconnected;

        public Task RunAsync(CancellationToken cancelToken = default)
        {
            var tasks = new[]
            {
                _tcpAcceptTask = RunTcpAcceptLoopAsync(cancelToken),
                _udpRecvTask = RunReceiveUdpAsync(cancelToken)
            };

            return Task.WhenAny(tasks);
        }

        public async Task ShutdownAsync()
        {
            foreach (var connection in _tcpConnections)
            {
                await CloseAsync(connection.Key);
            }
            
            // TODO: Fix
            _tcpAcceptTask.Dispose();
            _udpRecvTask.Dispose();
            
            _tcpServer.Stop();
            _udpClient.Close();
        }
        
        public async Task SendAsync(IPEndPoint endPoint, byte[] data,
            Reliability reliability = Reliability.ReliableOrdered)
        {
            switch (reliability)
            {
                case Reliability.Unreliable:
                case Reliability.UnreliableSequenced:
                    var len = 1 + data.Length;

                    if (reliability == Reliability.UnreliableSequenced)
                        len += 4;

                    using (var stream = new MemoryStream(len))
                    using (var writer = new BinaryWriter(stream))
                    {
                        writer.Write((byte) reliability);

                        if (reliability == Reliability.UnreliableSequenced)
                            writer.Write(_sendSeqNum++);

                        writer.Write(data);

                        await _udpClient.SendAsync(stream.ToArray(), len, endPoint).ConfigureAwait(false);
                    }

                    break;

                default:
                    if (!_tcpConnections.TryGetValue(endPoint, out var conn))
                        throw new InvalidOperationException("Client is not connected!!");

                    conn.Connection.Send(data);

                    break;
            }
        }
        
        public async Task CloseAsync(IPEndPoint endPoint)
        {
            if (!_tcpConnections.TryGetValue(endPoint, out var conn))
                throw new InvalidOperationException("Client is not connected!!");

            await conn.Connection.CloseAsync();
        }

        public IRakConnection GetConnection(IPEndPoint endPoint)
        {
            return _tcpConnections.TryGetValue(endPoint, out var entry) ? entry.Connection : null;
        }

        private Task RunTcpAcceptLoopAsync(CancellationToken cancelToken)
        {
            return Task.Run(async () =>
            {
                if (_tcpStarted)
                    _tcpServer.Stop();

                _tcpServer.Start();
                _tcpStarted = true;

                while (true)
                {
                    cancelToken.ThrowIfCancellationRequested();

                    var client = await _tcpServer.AcceptTcpClientAsync().ConfigureAwait(false);

                    var remoteEndpoint = client.GetRemoteEndPoint();
                    
                    if (_tcpConnections.ContainsKey(remoteEndpoint))
                    {
                        // We have to kick the client that is logged in and allow the new one. We don't know if the
                        // server thinks it's connected when it's not. This should catch those edge cases.
                        await _tcpConnections[remoteEndpoint].Connection.CloseAsync();
                    }

                    var conn = new TcpUdpConnection(client, _cert);

                    _tcpConnections[remoteEndpoint] = new ConnectionEntry
                    {
                        Connection = conn,
                        RunTask = conn.RunAsync()
                    };

                    conn.MessageReceived += async data =>
                    {
                        await OnMessageReceivedAsync(remoteEndpoint, data, Reliability.ReliableOrdered)
                            .ConfigureAwait(false);
                    };

                    conn.Disconnected += async reason =>
                    {
                        if (!_tcpConnections.ContainsKey(remoteEndpoint))
                            return;

                        _tcpConnections.TryRemove(remoteEndpoint, out _);

                        if (ClientDisconnected != null)
                            await ClientDisconnected(remoteEndpoint, reason).ConfigureAwait(false);
                    };

                    // temp hack
                    var __ = ClientConnected?.Invoke(remoteEndpoint);
                }
            }, cancelToken);
        }

        private Task RunReceiveUdpAsync(CancellationToken cancelToken)
        {
            return Task.Run(async () =>
            {
                while (true)
                {
                    cancelToken.ThrowIfCancellationRequested();

                    await ReceiveUdpAsync(cancelToken).ConfigureAwait(false);
                }
            }, cancelToken);
        }

        private async Task ReceiveUdpAsync(CancellationToken cancelToken)
        {
            await _udpReceiveLock.WaitAsync(cancelToken).ConfigureAwait(false);

            try
            {
                var recv = await _udpClient.ReceiveAsync().ConfigureAwait(false);

                var reliability = (Reliability) recv.Buffer[0];

                var offset = 1;

                if (reliability == Reliability.UnreliableSequenced)
                {
                    offset += 4;

                    var seqNumBuf = new ArraySegment<byte>(recv.Buffer, 1, 4);
                    var seqNum = BitConverter.ToUInt32(seqNumBuf);

                    if (seqNum < _recvSeqNum)
                        return;

                    _recvSeqNum = seqNum;
                }

                var data = new byte[recv.Buffer.Length - offset];

                Buffer.BlockCopy(recv.Buffer, offset, data, 0, data.Length);

                var _ = OnMessageReceivedAsync(recv.RemoteEndPoint, data, reliability);
            }
            finally
            {
                _udpReceiveLock.Release();
            }
        }

        private async Task OnMessageReceivedAsync(IPEndPoint endPoint, byte[] data, Reliability reliability)
        {
            switch ((MessageIdentifier) data[0])
            {
                case MessageIdentifier.ConnectionRequest:
                    await DoConnectionRequestAsync(endPoint, data).ConfigureAwait(false);
                    break;

                // These two (NewIncomingConnection, DisconnectionNotification) are not being sent by client for whatever reason
                case MessageIdentifier.NewIncomingConnection:
                    if (ClientConnected != null)
                        await ClientConnected(endPoint).ConfigureAwait(false);
                    break;

                case MessageIdentifier.DisconnectionNotification:
                    if (ClientDisconnected != null)
                        await ClientDisconnected(endPoint, CloseReason.ClientDisconnect).ConfigureAwait(false);
                    break;

                case MessageIdentifier.UserPacketEnum:
                    if (MessageReceived != null)
                        await MessageReceived(endPoint, data, reliability).ConfigureAwait(false);
                    break;
            }
        }

        private async Task DoConnectionRequestAsync(IPEndPoint endPoint, byte[] data)
        {
            var password = new ArraySegment<byte>(data, 1, data.Length - 1);

            if (!password.SequenceEqual(_password))
                await CloseAsync(endPoint).ConfigureAwait(false);

            using (var stream = new MemoryStream(1 + 4 + 2 + 2 + 4 + 2))
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write((byte) MessageIdentifier.ConnectionRequestAccepted);

                writer.Write(endPoint.Address.GetAddressBytes());
                writer.Write((ushort) endPoint.Port);

                writer.Write(new byte[2]);

                var local = _tcpServer.GetLocalEndPoint();

                writer.Write(local.Address.GetAddressBytes());
                writer.Write((ushort) local.Port);

                await SendAsync(endPoint, stream.ToArray()).ConfigureAwait(false);
            }
        }
    }
}