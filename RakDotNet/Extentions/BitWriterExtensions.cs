using System;
using System.Runtime.InteropServices;
using RakDotNet.IO;

namespace RakDotNet
{
    public static class BitWriterExtensions
    {
        #region Compressed
        
        public static int WriteCompressed(this BitWriter @this, ReadOnlySpan<byte> buf, int bits, bool unsigned)
        {
            for (var i = (bits >> 3) - 1; i > 0; i--)
            {
                var flag = buf[i] == (unsigned ? 0x00 : 0xFF);

                @this.WriteBit(flag);

                if (flag)
                    continue;

                return @this.Write(buf, (i + 1) << 3);
            }

            var flag2 = (buf[0] & 0xF0) == (unsigned ? 0x00 : 0xF0);

            @this.WriteBit(flag2);

            return @this.Write(buf.Slice(0, 1), flag2 ? 4 : 8);
        }

        public static int WriteCompressed(this BitWriter @this, Span<byte> buf, int bits, bool unsigned)
            => @this.WriteCompressed((ReadOnlySpan<byte>)buf, bits, unsigned);

        public static int WriteCompressed(this BitWriter @this, byte[] buf, int index, int length, int bits, bool unsigned)
        {
            if (bits > (length * 8))
                throw new ArgumentOutOfRangeException(nameof(bits), "Bit count exceeds buffer length");

            if (index > length)
                throw new ArgumentOutOfRangeException(nameof(index), "Index exceeds buffer length");

            return @this.WriteCompressed(new ReadOnlySpan<byte>(buf, index, length), bits, unsigned);
        }

        public static int WriteCompressed<T>(this BitWriter @this, T val, int bits, bool unsigned) where T : struct
        {
            var size = Marshal.SizeOf<T>();
            var buf = new byte[size];
            var ptr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(val, ptr, false);
            Marshal.Copy(ptr, buf, 0, size);

            return @this.WriteCompressed(new ReadOnlySpan<byte>(buf), bits, unsigned);
        }

        public static int WriteCompressed<T>(this BitWriter @this, T val, bool unsigned) where T : struct
            => @this.WriteCompressed<T>(val, Marshal.SizeOf<T>() * 8, unsigned);

        #endregion
        
        public static int Write(this BitWriter @this, Span<byte> buf) =>
            @this.Write(buf, buf.Length * 8);

        public static void Write(this BitWriter @this, ISerializable serializable) => serializable.Serialize(@this);
        
        public static void WriteString(this BitWriter @this, string val, int length = 33, bool wide = false)
        {
            foreach (var c in val)
            {
                if (wide) @this.Write((short) c);
                else @this.Write((byte) c);
            }

            Write(@this, new byte[(length - val.Length) * (wide ? sizeof(char) : sizeof(byte))]);
        }
    }
}