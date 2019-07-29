using RakDotNet.IO;

namespace RakDotNet
{
    public interface ISerializable
    {
        void Serialize(BitWriter writer);

        void Deserialize(BitReader reader);
    }
}