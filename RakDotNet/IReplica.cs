using System.IO;

namespace RakDotNet
{
    public interface IReplica : ISerializable
    {
        void Construct(Stream stream);
        void Destruct();
    }
}