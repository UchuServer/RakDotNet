using System;

namespace RakDotNet
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var arr = new byte[8];

            var bs = new BitStream(arr, (uint)arr.Length, true);

            bs.WriteInt8((sbyte)'h');
            bs.WriteUInt64(1L);
            bs.WriteBit(true);
            bs.WriteBitCompressed(true);

            for (var i = 0; i < 8; i++)
            {
                bs.ReadUInt8();
            }

            var h = bs.ReadInt8();
            var l = bs.ReadUInt64();
            var b = bs.ReadBit();
            var b2 = bs.ReadBitCompressed();

            Console.WriteLine((char)h);
            Console.WriteLine(l);
            Console.WriteLine(b);
            Console.WriteLine(b2);
            Console.ReadKey();
        }
    }
}
