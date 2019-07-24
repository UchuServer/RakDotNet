using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using RakDotNet.IO;

namespace RakDotNet
{
    public static class BitReaderExtensions
    {
        #region Compressed
        
        public static int ReadCompressed(this BitReader @this, Span<byte> buf, int bits, bool unsigned)
        {
            for (var i = (bits >> 3) - 1; i > 0; i--)
            {
                if (@this.ReadBit())
                    buf[i] = (byte)(unsigned ? 0x00 : 0xFF);
                else
                    return @this.Read(buf, (i + 1) << 3);
            }

            var flag = @this.ReadBit();
            var bufSize = @this.Read(buf, flag ? 4 : 8);

            if (flag)
                buf[0] |= (byte)(unsigned ? 0x00 : 0xF0);

            return bufSize;
        }

        public static int ReadCompressed(this BitReader @this, byte[] buf, int index, int length, int bits, bool unsigned)
        {
            if (bits > (length * 8))
                throw new ArgumentOutOfRangeException(nameof(bits), "Bit count exceeds buffer length");

            if (index > length)
                throw new ArgumentOutOfRangeException(nameof(index), "Index exceeds buffer length");

            return @this.ReadCompressed(new Span<byte>(buf, index, length), bits, unsigned);
        }

        public static T ReadCompressed<T>(this BitReader @this, int bits, bool unsigned) where T : struct
        {
            var bufSize = (int)Math.Ceiling(bits / 8d);
            Span<byte> buf = stackalloc byte[bufSize];

            @this.ReadCompressed(buf, bits, unsigned);

            return MemoryMarshal.Read<T>(buf);
        }

        public static T ReadCompressed<T>(this BitReader @this, bool unsigned) where T : struct
            => @this.ReadCompressed<T>(Marshal.SizeOf<T>() * 8, unsigned);

        #endregion
        
        public static IEnumerable<byte> ReadBytes(this BitReader @this, int bytes)
        {
            Span<byte> buf = stackalloc byte[bytes];
            @this.Read(buf, buf.Length * 8);
            return buf.ToArray();
        }
        
        public static void Read(this BitReader @this, ISerializable serializable) => serializable.Deserialize(@this);
        
        public static string ReadString(this BitReader @this, int length = 33, bool wide = false)
        {
            Span<byte> bytes = stackalloc byte[length * (wide ? 2 : 1)];
            @this.Read(bytes, length * (wide ? sizeof(char) : sizeof(byte)) * 8);

            return wide ? Encoding.Unicode.GetString(bytes) : Encoding.ASCII.GetString(bytes);
        }
    }
}