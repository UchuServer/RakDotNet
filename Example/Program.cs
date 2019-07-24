using System;
using System.Threading.Tasks;
using RakDotNet;
using RakDotNet.TcpUdp;

namespace Example
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            IRakNetServer server = new TcpUdpServer(21836, "3.25 ND1");

            Console.WriteLine("starting");

            server.NewConnection += endpoint =>
                Console.WriteLine($"{endpoint.Address}:{endpoint.Port} connected");

            server.PacketReceived += (endpoint, data) =>
                Console.WriteLine($"received packet from {endpoint.Address}:{endpoint.Port} with id {data[0]}");

            server.Disconnection += endpoint =>
                Console.WriteLine($"{endpoint.Address}:{endpoint.Port} disconnected");

            server.Start();

            await Task.Delay(-1);
        }
    }
}