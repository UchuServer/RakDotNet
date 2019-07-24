using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using RakDotNet.IO;

namespace RakDotNet.TcpUdp
{
    public class TcpUdpServer : IRakNetServer
    {
        private readonly X509Certificate _cert;
        private readonly List<TcpClient> _clients;
        private readonly IPEndPoint _endpoint;
        private readonly byte[] _password;
        private readonly Dictionary<IPEndPoint, int> _seqNums;

        private readonly TcpListener _tcp;
        private readonly UdpClient _udp;

        private bool _active;
        private long _startTime;

        public TcpUdpServer(int port, string password, X509Certificate cert = null)
            : this(port, password.Select(c => (byte) c).ToArray(), cert)
        {
        }

        public TcpUdpServer(int port, byte[] password, X509Certificate cert = null)
        {
            _tcp = new TcpListener(IPAddress.Any, port);
            _udp = new UdpClient(port);
            _password = password;
            _cert = cert;
            _clients = new List<TcpClient>();
            _seqNums = new Dictionary<IPEndPoint, int>();
            _endpoint = (IPEndPoint) _tcp.LocalEndpoint;
            _active = false;
        }

        public event Action<IPEndPoint, byte[]> PacketReceived;
        public event Action<IPEndPoint> NewConnection;
        public event Action<IPEndPoint> Disconnection;

        public ServerProtocol Protocol => ServerProtocol.TcpUdp;

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

                    if (client == null) continue;
                    _clients.RemoveAll(c => c.Client.RemoteEndPoint.Equals(client.Client.RemoteEndPoint));
                    _clients.Add(client);

                    HandleTcpClient(client);
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

                    HandleUdpDatagram(res.RemoteEndPoint, res.Buffer);
                }
            });
        }

        public void Stop()
        {
            _active = false;
            _clients.Clear();

            _tcp.Stop();
        }

        public void CloseConnection(IPEndPoint endpoint)
        {
            if (!IsConnected(endpoint))
                throw new InvalidOperationException("Client is not connected");

            Send(new[] {(byte) MessageIdentifiers.DisconnectionNotification}, endpoint);

            _clients.RemoveAll(c => c.Client.RemoteEndPoint.Equals(endpoint));

            Disconnection?.Invoke(endpoint);
        }

        public void Send(Stream stream, IPEndPoint endpoint,
            PacketReliability reliability = PacketReliability.ReliableOrdered)
        {
            Send(stream, new[] {endpoint}, false, reliability);
        }

        public void Send(Stream stream, ICollection<IPEndPoint> endpoints = null, bool broadcast = false,
            PacketReliability reliability = PacketReliability.ReliableOrdered)
        {
            if (!(stream is MemoryStream ms)) stream.CopyTo(ms = new MemoryStream());

            var data = ms.ToArray();
            Array.Resize(ref data, (int) stream.Position);

            Send(data, endpoints, broadcast, reliability);
        }

        public void Send(byte[] data, IPEndPoint endpoint,
            PacketReliability reliability = PacketReliability.ReliableOrdered)
        {
            Send(data, new[] {endpoint}, false, reliability);
        }

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

                    Buffer.BlockCopy(data, 0, buf, 1, data.Length);

                    foreach (var recipient in recipients)
                        Task.Run(async () => await _udp.SendAsync(buf, buf.Length, recipient));

                    break;
                }

                // UnreliableSequenced is not/rarely used, so can be left out
                case PacketReliability.UnreliableSequenced:
                    throw new NotSupportedException();

                default:
                {
                    var buf = new byte[4 + data.Length];

                    Buffer.BlockCopy(BitConverter.GetBytes((uint) data.Length), 0, buf, 0, 4);
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
                            {
                                stream = tcpStream;
                            }

                            await stream.WriteAsync(buf, 0, buf.Length);
                        });
                    }

                    break;
                }
            }
        }

        public event Action<IPEndPoint, SendFailReason, byte[]> SendFailed;

        /// <summary>
        ///     Set up a task to handle messages from a TCP client.
        /// </summary>
        /// <param name="client"></param>
        private void HandleTcpClient(TcpClient client)
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
                {
                    stream = tcpStream;
                }

                var packetLength = 0u;

                while (client.Connected)
                {
                    await Task.Delay(30);

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

                        HandleData((IPEndPoint) client.Client.RemoteEndPoint, buf);
                    }
                }
            });
        }

        /// <summary>
        ///     Handle a UDP Datagram
        /// </summary>
        /// <param name="endpoint">Client</param>
        /// <param name="data">Data</param>
        private void HandleUdpDatagram(IPEndPoint endpoint, byte[] data)
        {
            if (!_seqNums.ContainsKey(endpoint)) _seqNums[endpoint] = 0;

            using (var stream = new MemoryStream(data))
            {
                var rel = (PacketReliability) stream.ReadByte();

                switch (rel)
                {
                    case PacketReliability.Unreliable:
                    {
                        var buf = new byte[data.Length - 1];

                        stream.Read(buf, 0, data.Length - 1);

                        HandleData(endpoint, buf);
                        break;
                    }

                    case PacketReliability.UnreliableSequenced:
                    {
                        var seqBuf = new byte[4];

                        stream.Read(seqBuf, 0, 4);

                        var seqNum = BitConverter.ToUInt32(seqBuf, 0);

                        if (seqNum > _seqNums[endpoint])
                        {
                            _seqNums[endpoint] = (int) seqNum;

                            var buf = new byte[data.Length - 5];

                            stream.Read(buf, 0, data.Length - 5);

                            HandleData(endpoint, buf);
                        }

                        break;
                    }
                }
            }
        }

        /// <summary>
        ///     Forward the handling of data.
        /// </summary>
        /// <param name="endpoint">Client</param>
        /// <param name="data">Data</param>
        private void HandleData(IPEndPoint endpoint, byte[] data)
        {
            var stream = new MemoryStream(data);

            using (var reader = new BitReader(stream))
            {
                var id = (MessageIdentifiers) reader.Read<byte>();
                switch (id)
                {
                    case MessageIdentifiers.ConnectionRequest:
                        HandleConnectionRequest(reader, endpoint);
                        break;
                    case MessageIdentifiers.InternalPing:
                        HandleInternalPing(reader, endpoint);
                        break;
                    case MessageIdentifiers.NewIncomingConnection:
                        NewConnection?.Invoke(endpoint);
                        break;
                    case MessageIdentifiers.DisconnectionNotification:
                        HandleDisconnection(endpoint);
                        break;
                    case MessageIdentifiers.UserPacketEnum:
                        PacketReceived?.Invoke(endpoint, data);
                        break;
                }
            }
        }

        /// <summary>
        ///     Reply to a client connection request.
        /// </summary>
        /// <param name="reader">Connection Request</param>
        /// <param name="endpoint">Client</param>
        /// <exception cref="NotImplementedException">The password does not match.</exception>
        private void HandleConnectionRequest(BitReader reader, IPEndPoint endpoint)
        {
            var password = reader.ReadBytes((int) (reader.BaseStream.Length - reader.BaseStream.Position));

            if (password.SequenceEqual(_password))
            {
                var stream = new MemoryStream();
                using (var writer = new BitWriter(stream))
                {
                    writer.Write((byte) MessageIdentifiers.ConnectionRequestAccepted);
                    writer.Write(endpoint.Address.GetAddressBytes());
                    writer.Write((ushort) endpoint.Port);
                    writer.Write(new byte[2]);
                    writer.Write(_endpoint.Address.GetAddressBytes());
                    writer.Write((ushort) _endpoint.Port);
                }

                Send(stream, endpoint);
            }
            else throw new NotImplementedException();
        }

        /// <summary>
        ///     Reply to a client ping request.
        /// </summary>
        /// <param name="reader">Ping request</param>
        /// <param name="endpoint">Client</param>
        private void HandleInternalPing(BitReader reader, IPEndPoint endpoint)
        {
            var stream = new MemoryStream();
            using (var writer = new BitWriter(stream))
            {
                writer.Write((byte) MessageIdentifiers.ConnectedPong);
                writer.Write(reader.Read<uint>());
                writer.Write((uint) (Environment.TickCount - _startTime));
            }

            Send(stream, endpoint);
        }

        /// <summary>
        ///     Reply to a client request to disconnect from the server.
        /// </summary>
        /// <param name="endpoint">Client</param>
        private void HandleDisconnection(IPEndPoint endpoint)
        {
            _clients.RemoveAll(c => c.Client.RemoteEndPoint.Equals(endpoint));

            Disconnection?.Invoke(endpoint);
        }

        /// <summary>
        ///     Check if a client is connected to the server.
        /// </summary>
        /// <param name="endpoint">Client</param>
        /// <returns></returns>
        private bool IsConnected(IPEndPoint endpoint) // TODO: check if the client is actually connected
            => _clients.Exists(c => c.Client.RemoteEndPoint.Equals(endpoint));
    }
}