using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RakDotNet
{
    public class RakServer
    {
        private readonly UdpClient sock;
        private readonly string password;
        private readonly int maxConnections;
        private readonly Dictionary<IPEndPoint, ReliabilityLayer> connections;

        public bool Active { get; private set; }

        public RakServer(short port, int maxConnections, string password = null)
        {
            Active = false;
            sock = new UdpClient(port);
            connections = new Dictionary<IPEndPoint, ReliabilityLayer>();

            this.maxConnections = maxConnections;
            this.password = password;
        }

        public async Task StartAsync()
        {
            Active = true;

            while (Active)
            {
                await Task.Delay(30); // 30 millisecond delay

                var packet = await sock.ReceiveAsync();

                if (packet == null)
                    continue;

                if (packet.Buffer.Length <= 2)
                {
                    if (packet.Buffer[0] == 9 && packet.Buffer[1] == 0)
                    {
                        if (connections.Count < maxConnections)
                        {
                            if (!connections.ContainsKey(packet.RemoteEndPoint))
                                connections[packet.RemoteEndPoint] = new ReliabilityLayer(sock, packet.RemoteEndPoint);

                            var data = new byte[] { (byte)MessageIdentifier.ID_OPEN_CONNECTION_REPLY, 0 };

                            await sock.SendAsync(data, data.Length, packet.RemoteEndPoint);
                        } // TODO: handle 'connections.Count >= maxConnections'
                    }
                }
                else
                {
                    ReliabilityLayer reliability;

                    if (connections.TryGetValue(packet.RemoteEndPoint, out reliability))
                    {
                        foreach (var pkt in reliability.HandlePacket(packet.Buffer))
                        {
                        }
                    }
                }
            }
        }
    }
}
