using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace RakDotNet.TcpUdp
{
    public class TcpUdpServer : IRakNetServer
    {
        public event Action<IPEndPoint, byte[]> PacketReceived;
        public event Action<IPEndPoint> NewConnection;
        public event Action<IPEndPoint> Disconnection;
        public event Action<IPEndPoint, SendFailReason, byte[]> SendFailed;

        public ServerProtocol Protocol => ServerProtocol.TcpUdp;

        private readonly TcpListener _tcp;
        private readonly UdpClient _udp;
        private readonly byte[] _password;
        private readonly X509Certificate _cert;
        private readonly List<TcpClient> _clients;
        private readonly IPEndPoint _endpoint;

        private int _seqNum;
        private bool _active;
        private long _startTime;

        public TcpUdpServer(int port, string password, X509Certificate cert = null)
            : this(port, BitStream.ToBytes(password), cert)
        {
        }

        public TcpUdpServer(int port, byte[] password, X509Certificate cert = null)
        {
            _tcp = new TcpListener(IPAddress.Any, port);
            _udp = new UdpClient(port);
            _password = password;
            _cert = cert;
            _clients = new List<TcpClient>();
            _endpoint = (IPEndPoint) _tcp.LocalEndpoint;
            _seqNum = 0;
            _active = false;
            _startTime = 0;
        }

        public void Start()
        {
            if (_active)
                throw new InvalidOperationException("Already active");

            _active = true;
            _startTime = Environment.TickCount;

            // TCP
            Task.Run(async () =>
            {
                _tcp.Start();

                while (_active)
                {
                    var client = await _tcp.AcceptTcpClientAsync();

                    if (client != null)
                    {
                        _clients.RemoveAll(c => c.Client.RemoteEndPoint.Equals(client.Client.RemoteEndPoint));
                        _clients.Add(client);

                        _handleTcpClient(client);
                    }
                }
            });

            // Check for dead TCP clients every 30 seconds
            Task.Run(async () =>
            {
                while (_active)
                {
                    await Task.Delay(30000);

                    _clients.RemoveAll(c => !c.Connected);
                }
            });

            // UDP
            Task.Run(async () =>
            {
                while (_active)
                {
                    var res = await _udp.ReceiveAsync();

                    _handleUdpDatagram(res.RemoteEndPoint, res.Buffer);
                }
            });
        }

        private void _handleTcpClient(TcpClient client)
        {
            Task.Run(async () =>
            {
                var tcpStream = client.GetStream();
                Stream stream;

                if (_cert != null)
                {
                    var ssl = new SslStream(tcpStream);

                    try
                    {
                        ssl.AuthenticateAsServer(_cert);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    stream = ssl;
                }
                else
                    stream = tcpStream;

                var packetLength = 0u;

                while (client.Connected)
                {
                    if (packetLength == 0 && client.Available >= 4)
                    {
                        var lenBuf = new byte[4];

                        await stream.ReadAsync(lenBuf, 0, 4);

                        packetLength = BitConverter.ToUInt32(lenBuf, 0);
                    }
                    else if (packetLength > 0 && client.Available >= packetLength)
                    {
                        var buf = new byte[packetLength];

                        await stream.ReadAsync(buf, 0, (int) packetLength);

                        packetLength = 0;

                        _handleData((IPEndPoint) client.Client.RemoteEndPoint, buf);
                    }
                }
            });
        }

        private void _handleUdpDatagram(IPEndPoint endpoint, byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                var rel = (PacketReliability) stream.ReadByte();

                if (rel == PacketReliability.Unreliable)
                {
                    var buf = new byte[data.Length - 1];

                    stream.Read(buf, 0, data.Length - 1);

                    _handleData(endpoint, buf);
                }
                else if (rel == PacketReliability.UnreliableSequenced)
                {
                    var seqBuf = new byte[4];

                    stream.Read(seqBuf, 0, 4);

                    var seqNum = BitConverter.ToUInt32(seqBuf, 0);

                    if (seqNum > _seqNum)
                    {
                        _seqNum = (int) seqNum;

                        var buf = new byte[data.Length - 5];

                        stream.Read(buf, 0, data.Length - 5);

                        _handleData(endpoint, buf);
                    }
                }
            }
        }

        private void _handleData(IPEndPoint endpoint, byte[] data)
        {
            var stream = new BitStream(data);

            var id = (MessageIdentifiers) stream.ReadByte();

            switch (id)
            {
                case MessageIdentifiers.ConnectionRequest:
                    _handleConnectionRequest(stream, endpoint);
                    break;
                case MessageIdentifiers.InternalPing:
                    _handleInternalPing(stream, endpoint);
                    break;
                case MessageIdentifiers.NewIncomingConnection:
                    NewConnection?.Invoke(endpoint);
                    break;
                case MessageIdentifiers.DisconnectionNotification:
                    _handleDisconnection(endpoint);
                    break;
                case MessageIdentifiers.UserPacketEnum:
                    PacketReceived?.Invoke(endpoint, data);
                    break;
            }
        }

        public void Stop()
        {
            _active = false;
            _clients.Clear();

            _tcp.Stop();
        }

        public void CloseConnection(IPEndPoint endpoint)
        {
            if (!_isConnected(endpoint))
                throw new InvalidOperationException("Client is not connected");

            Send(new[] {(byte) MessageIdentifiers.DisconnectionNotification}, endpoint);

            _clients.RemoveAll(c => c.Client.RemoteEndPoint.Equals(endpoint));

            Disconnection?.Invoke(endpoint);
        }

        public void Send(BitStream stream, IPEndPoint endpoint,
            PacketReliability reliability = PacketReliability.ReliableOrdered)
            => Send(stream, new[] {endpoint}, false, reliability);

        public void Send(BitStream stream, ICollection<IPEndPoint> endpoints = null, bool broadcast = false,
            PacketReliability reliability = PacketReliability.ReliableOrdered)
            => Send(stream.BaseBuffer, endpoints, broadcast, reliability);

        public void Send(byte[] data, IPEndPoint endpoint,
            PacketReliability reliability = PacketReliability.ReliableOrdered)
            => Send(data, new[] {endpoint}, false, reliability);

        public void Send(byte[] data, ICollection<IPEndPoint> endpoints = null, bool broadcast = false,
            PacketReliability reliability = PacketReliability.ReliableOrdered)
        {
            var recipients = broadcast || endpoints == null
                ? _clients.Select(c => (IPEndPoint) c.Client.RemoteEndPoint).ToArray()
                : endpoints;

            switch (reliability)
            {
                case PacketReliability.Unreliable:
                {
                    var buf = new byte[1 + data.Length];

                    buf[0] = 0;

                    Buffer.BlockCopy(data, 0, buf, 1, data.Length);

                    foreach (var recipient in recipients)
                    {
                        Task.Run(async () => await _udp.SendAsync(buf, buf.Length, recipient));
                    }
                    break;
                }

                // UnreliableSequenced is not/rarely used, so can be left out
                case PacketReliability.UnreliableSequenced:
                    throw new NotSupportedException();

                default:
                {
                    var buf = new byte[4 + data.Length];
                    var lenBuf = BitConverter.GetBytes((uint) data.Length);

                    Buffer.BlockCopy(lenBuf, 0, buf, 0, 4);
                    Buffer.BlockCopy(data, 0, buf, 4, data.Length);

                    foreach (var recipient in recipients)
                    {
                        var client = _clients.FirstOrDefault(c => c.Client.RemoteEndPoint.Equals(recipient));

                        if (client == null)
                        {
                            SendFailed?.Invoke(recipient, SendFailReason.NotConnected, data);

                            continue;
                        }

                        Task.Run(async () =>
                        {
                            var tcpStream = client.GetStream();
                            Stream stream;

                            if (_cert != null)
                            {
                                var ssl = new SslStream(tcpStream);

                                try
                                {
                                    ssl.AuthenticateAsServer(_cert);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                }

                                stream = ssl;
                            }
                            else
                                stream = tcpStream;

                            await stream.WriteAsync(buf, 0, buf.Length);
                        });
                    }

                    break;
                }
            }
        }

        private void _handleConnectionRequest(BitStream stream, IPEndPoint endpoint)
        {
            var password = stream.ReadBits(stream.BitCount - stream.ReadPosition);

            if (password.SequenceEqual(_password))
            {
                var res = new BitStream();

                res.WriteByte((byte) MessageIdentifiers.ConnectionRequestAccepted);
                res.Write(endpoint.Address.GetAddressBytes());
                res.WriteUShort((ushort) endpoint.Port);
                res.Write(new byte[2]);
                res.Write(_endpoint.Address.GetAddressBytes());
                res.WriteUShort((ushort) _endpoint.Port);

                Send(res, endpoint);
            }
            else
                throw new NotImplementedException();
        }

        private void _handleInternalPing(BitStream stream, IPEndPoint endpoint)
        {
            var time = stream.ReadUInt();

            var pong = new BitStream();

            pong.WriteByte((byte) MessageIdentifiers.ConnectedPong);
            pong.WriteUInt(time);
            pong.WriteUInt((uint) (Environment.TickCount - _startTime));

            Send(pong, endpoint);
        }

        private void _handleDisconnection(IPEndPoint endpoint)
        {
            _clients.RemoveAll(c => c.Client.RemoteEndPoint.Equals(endpoint));

            Disconnection?.Invoke(endpoint);
        }

        private bool _isConnected(IPEndPoint endpoint) // TODO: check if the client is actually connected
            => _clients.Exists(c => c.Client.RemoteEndPoint.Equals(endpoint));
    }
}