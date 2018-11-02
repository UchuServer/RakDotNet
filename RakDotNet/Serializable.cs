namespace RakDotNet
{
    public abstract class Serializable
    {
        public abstract void Serialize(BitStream stream);

        public abstract void Deserialize(BitStream stream);
    }
}