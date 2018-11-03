namespace RakDotNet
{
    public interface ISerializable
    {
        void Serialize(BitStream stream);

        void Deserialize(BitStream stream);
    }
}