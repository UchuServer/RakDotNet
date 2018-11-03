using System;
using System.Net;
using System.Threading.Tasks;
using RakDotNet;

namespace Example
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var server = new RakNetServer(1001, "3.25 ND1");

            Console.WriteLine("starting");

            server.NewConnection += endpoint =>
                Console.WriteLine($"{endpoint.Address}:{endpoint.Port} connected");

            server.PacketReceived += (endpoint, data) =>
                Console.WriteLine($"received packet from {endpoint.Address}:{endpoint.Port}");

            server.Disconnection += endpoint =>
                Console.WriteLine($"{endpoint.Address}:{endpoint.Port} disconnected");

            server.Start();

            await Task.Delay(-1);
        }
    }
}