using System.Net;
using System.Net.Sockets;

namespace RakDotNet.TcpUdp
{
    public static class TcpExtensions
    {
        public static IPEndPoint GetRemoteEndPoint(this TcpClient @this)
        {
            return (IPEndPoint) @this.Client.RemoteEndPoint;
        }

        public static IPEndPoint GetLocalEndPoint(this TcpListener @this)
        {
            return (IPEndPoint) @this.LocalEndpoint;
        }
    }
}