namespace RakDotNet
{
    public interface IReplica : ISerializable
    {
        void Construct(BitStream stream);
        void Destruct();
    }
}