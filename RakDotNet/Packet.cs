namespace RakDotNet
{
    public class Packet
    {
        public uint MessageNumber { get; set; }
        
        public PacketReliability Reliability { get; set; }
        
        public byte OrderingChannel { get; set; }
        public uint OrderingIndex { get; set; }
        
        public bool SplitPacket { get; set; }
        public ushort SplitPacketId { get; set; }
        public uint SplitPacketIndex { get; set; }
        public uint SplitPacketCount { get; set; }
        
        public byte[] Data { get; set; }
    }
}