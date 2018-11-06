using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace RakDotNet
{
    public class RakNetServer
    {
        public event Action<IPEndPoint, byte[]> PacketReceived;
        public event Action<IPEndPoint> NewConnection;
        public event Action<IPEndPoint> Disconnection;

        private readonly UdpClient _udp;
        private readonly Dictionary<IPEndPoint, ReliabilityLayer> _connections;
        private readonly byte[] _password;
        private readonly IPEndPoint _endpoint;
        private readonly long _startTime;

        private bool _active;

        public RakNetServer(int port, string password)
            : this(port, BitStream.ToBytes(password))
        {
        }

        public RakNetServer(int port, byte[] password)
        {
            _udp = new UdpClient(new IPEndPoint(IPAddress.Any, port));
            _connections = new Dictionary<IPEndPoint, ReliabilityLayer>();
            _password = password;
            _endpoint = (IPEndPoint) _udp.Client.LocalEndPoint;
            _startTime = Environment.TickCount;
            _active = false;
        }

        public void Start()
        {
            if (_active)
                throw new InvalidOperationException("Already active");

            _active = true;

            Task.Run(async () =>
            {
                while (_active)
                {
                    var datagram = await _udp.ReceiveAsync().ConfigureAwait(false);
                    var data = datagram.Buffer;
                    var endpoint = datagram.RemoteEndPoint;

                    if (data.Length <= 2)
                    {
                        if (data[0] != (byte) MessageIdentifiers.OpenConnectionRequest) continue;

                        if (!_connections.ContainsKey(endpoint))
                            _connections[endpoint] = new ReliabilityLayer(_udp, endpoint);

                        var conn = _connections[endpoint];

                        if (!conn.Active)
                            conn.StartSendLoop();

                        var pkt = new byte[] {(byte) MessageIdentifiers.OpenConnectionReply, 0};

                        await _udp.SendAsync(pkt, pkt.Length, endpoint).ConfigureAwait(false);
                    }
                    else
                    {
                        if (!_connections.TryGetValue(endpoint, out var layer)) continue;

                        foreach (var packet in layer.HandleDatagram(data))
                        {
                            var stream = new BitStream(packet);

                            switch ((MessageIdentifiers) stream.ReadByte())
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
                                    PacketReceived?.Invoke(endpoint, packet);
                                    break;
                            }
                        }
                    }
                }
            });

            Task.Run(async () =>
            {
                while (_active)
                {
                    await Task.Delay(30000);

                    var dead = _connections.Keys.Where(k =>
                    {
                        var conn = _connections[k];

                        return conn.Resends && conn.LastAckTime < Environment.TickCount / 1000f - 10f;
                    }).ToArray();

                    for (var i = 0; i < dead.Length; i++) CloseConnection(dead[i]);
                }
            });
        }

        public void Stop()
        {
            _active = false;

            var keys = _connections.Keys.ToArray();

            for (var i = 0; i < keys.Length; i++) CloseConnection(keys[i]);
        }

        public void Send(BitStream stream, IPEndPoint endpoint,
            PacketReliability reliability = PacketReliability.ReliableOrdered)
            => Send(stream, new[] {endpoint}, false, reliability);

        public void Send(BitStream stream, IPEndPoint[] endpoints = null, bool broadcast = false,
            PacketReliability reliability = PacketReliability.ReliableOrdered)
            => Send(stream.BaseBuffer, endpoints, broadcast, reliability);

        public void Send(byte[] data, IPEndPoint endpoint,
            PacketReliability reliability = PacketReliability.ReliableOrdered)
            => Send(data, new[] {endpoint}, false, reliability);

        public void Send(byte[] data, IPEndPoint[] endpoints = null, bool broadcast = false,
            PacketReliability reliability = PacketReliability.ReliableOrdered)
        {
            var recipients = broadcast || endpoints == null ? _connections.Keys.ToArray() : endpoints;

            foreach (var endpoint in recipients)
            {
                if (!_connections.ContainsKey(endpoint)) continue;

                _connections[endpoint].Send(data, reliability);
            }
        }

        public void CloseConnection(IPEndPoint endpoint)
        {
            if (!_connections.ContainsKey(endpoint))
                throw new InvalidOperationException("Client is not connected");

            Send(new[] {(byte) MessageIdentifiers.DisconnectionNotification}, endpoint);

            _connections[endpoint].StopSendLoop();
            _connections.Remove(endpoint);

            Disconnection?.Invoke(endpoint);
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

                Send(res, endpoint, PacketReliability.Reliable);
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
            _connections[endpoint].StopSendLoop();
            _connections.Remove(endpoint);

            Disconnection?.Invoke(endpoint);
        }
    }
}