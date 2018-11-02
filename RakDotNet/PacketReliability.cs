namespace RakDotNet
{
    public enum PacketReliability : byte
    {
        Unreliable,
        UnreliableSequenced,
        Reliable,
        ReliableOrdered,
        ReliableSequenced
    }
}