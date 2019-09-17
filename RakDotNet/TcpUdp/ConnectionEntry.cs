using System.Threading.Tasks;

namespace RakDotNet.TcpUdp
{
    internal class ConnectionEntry
    {
        public TcpUdpConnection Connection { get; set; }
        public Task RunTask { get; set; }
    }
}