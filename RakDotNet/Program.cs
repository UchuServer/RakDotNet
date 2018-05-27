using System;

namespace RakDotNet
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var bs = new BitStream();

            bs.WriteInt8((sbyte)'h');
            bs.WriteUInt64(1L);

            var h = bs.ReadInt8();
            var l = bs.ReadUInt64();

            Console.WriteLine((char)h);
            Console.WriteLine(l);
        }
    }
}
