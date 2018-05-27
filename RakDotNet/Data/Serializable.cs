using System;
using System.Collections.Generic;
using System.Text;

namespace RakDotNet
{
    public abstract class Serializable
    {
        public abstract void Serialize(BitStream stream);
        public static Serializable Deserialize(BitStream stream) => null;
    }
}
