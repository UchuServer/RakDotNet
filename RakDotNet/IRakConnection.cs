using System;
using System.Net;
using System.Threading.Tasks;

namespace RakDotNet
{
    public interface IRakConnection
    {
        event Func<byte[], Task> MessageReceived;
        event Func<CloseReason, Task> Disconnected;

        int AveragePing { get; }
        IPEndPoint EndPoint { get; }

        Task CloseAsync();

        void Send(ReadOnlySpan<byte> buf);

        void Send(Span<byte> buf);

        void Send(byte[] buf, int index, int length);
    }
}
