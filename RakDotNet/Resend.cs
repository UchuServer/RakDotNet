using System;

namespace RakDotNet
{
    public class Resend
    {
        public DateTimeOffset Time { get; set; }
        public InternalPacket Packet { get; set; }
    }
}