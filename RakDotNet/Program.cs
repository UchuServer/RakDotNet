using System;
using System.Threading;
using System.Threading.Tasks;

namespace RakDotNet
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var server = RakNetworkFactory.GetRakPeerInterface();

            server.Startup(8, 30, new SocketDescriptor(1001, "127.0.0.1"));
            
            while (true)
            {
                Thread.Sleep(30);

                var packet = server.Receive();

                if (packet == null)
                    continue;

                Console.WriteLine(packet.Data);
            }
        }
    }
}
