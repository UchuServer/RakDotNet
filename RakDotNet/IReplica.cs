using RakDotNet.IO;

namespace RakDotNet
{
    public interface IReplica : ISerializable
    {
        void Construct(BitWriter writer);

        void Destruct();
    }
}