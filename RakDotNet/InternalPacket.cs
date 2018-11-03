namespace RakDotNet
{
    public class InternalPacket : ISerializable
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

        public void Serialize(BitStream stream)
        {
            stream.WriteUInt(MessageNumber);
            stream.WriteBits(new[] {(byte) Reliability}, 3);

            if (Reliability == PacketReliability.UnreliableSequenced ||
                Reliability == PacketReliability.ReliableSequenced ||
                Reliability == PacketReliability.ReliableOrdered)
            {
                stream.WriteBits(new[] {OrderingChannel}, 5);
                stream.WriteUInt(OrderingIndex);
            }

            stream.WriteBit(SplitPacket);

            if (SplitPacket)
            {
                stream.WriteUShort(SplitPacketId);
                stream.WriteUIntCompressed(SplitPacketIndex);
                stream.WriteUIntCompressed(SplitPacketCount);
            }

            stream.WriteUShortCompressed((ushort) BitStream.BytesToBits(Data.Length));

            stream.AlignWrite();

            stream.Write(Data);
        }

        public void Deserialize(BitStream stream)
        {
            MessageNumber = stream.ReadUInt();

            Reliability = (PacketReliability) stream.ReadBits(3)[0];

            if (Reliability == PacketReliability.UnreliableSequenced ||
                Reliability == PacketReliability.ReliableSequenced ||
                Reliability == PacketReliability.ReliableOrdered)
            {
                OrderingChannel = stream.ReadBits(5)[0];
                OrderingIndex = stream.ReadUInt();
            }

            SplitPacket = stream.ReadBit();

            if (SplitPacket)
            {
                SplitPacketId = stream.ReadUShort();
                SplitPacketIndex = stream.ReadCompressedUInt();
                SplitPacketCount = stream.ReadCompressedUInt();
            }

            var length = stream.ReadCompressedUShort();

            stream.AlignRead();

            Data = stream.Read(BitStream.BitsToBytes(length));
        }
    }
}