using System;

namespace RakDotNet
{
    public class InternalPacket : Serializable
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

        public override void Serialize(BitStream stream)
        {
            throw new NotImplementedException();
        }
        
        public override void Deserialize(BitStream stream)
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

            if (SplitPacket = stream.ReadBit())
            {
                SplitPacketId = stream.ReadUShort();
                SplitPacketIndex = stream.ReadUInt();
                SplitPacketCount = stream.ReadCompressedUInt();
            }

            var length = stream.ReadCompressedUShort();
            
            stream.AlignRead();

            Data = stream.Read(BitStream.BitsToBytes(length));
        }
    }
}