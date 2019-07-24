using RakDotNet.IO;

namespace RakDotNet
{
    public interface ISerializable
    {
        void Serialize(BitWriter stream);

        void Deserialize(BitReader stream);
    }
}