using RakDotNet.TcpUdp;
using System;
using System.Threading.Tasks;

namespace RakDotNet.Example
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            IRakServer server = new TcpUdpServer(21836, "3.25 ND1");

            var runTask = server.RunAsync();

            server.MessageReceived += (endPoint, data, _rel) =>
            {
                Console.WriteLine($"{endPoint}: {{ {string.Join(", ", data)} }}");

                return Task.CompletedTask;
            };

            server.ClientConnected += endPoint =>
            {
                Console.WriteLine($"New client: {endPoint}");

                return Task.CompletedTask;
            };

            server.ClientDisconnected += (endPoint, reason) =>
            {
                Console.WriteLine($"Client disconnect: {endPoint}");

                return Task.CompletedTask;
            };

            Console.WriteLine("Server running!");

            await runTask;

            Console.WriteLine("Stopping!");
        }
    }
}
