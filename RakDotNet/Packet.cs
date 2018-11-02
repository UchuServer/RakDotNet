namespace RakDotNet
{
    public struct Packet
    {
        public byte[] Data { get; set; }
        public PacketReliability Reliability { get; set; }
        public uint OrderingIndex { get; set; }
        
        public bool SplitPacket { get; set; }
        public ushort SplitPacketId { get; set; }
        public int SplitPacketIndex { get; set; }
        public int SplitPacketCount { get; set; }
    }
}