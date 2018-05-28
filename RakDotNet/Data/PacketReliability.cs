namespace RakDotNet
{
    public enum PacketReliability : int
    {
        UNRELIABLE,
        UNRELIABLE_SEQUENCED,
        RELIABLE,
        RELIABLE_ORDERED,
        RELIABLE_SEQUENCED,
    }
}
