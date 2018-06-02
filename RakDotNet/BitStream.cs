using System;
using System.Runtime.InteropServices;

namespace RakDotNet
{
    public class BitStream : IDisposable
    {
        #region Native Destructors/Constructors

        [DllImport("RakDotNetNative")]
        private static extern IntPtr InitializeBitStream1();

        [DllImport("RakDotNetNative")]
        private static extern IntPtr InitializeBitStream2(uint initialBytesToAllocate);

        [DllImport("RakDotNetNative")]
        private static extern IntPtr InitializeBitStream3(IntPtr data, uint length, bool copyData);

        [DllImport("RakDotNetNative")]
        private static extern void DisposeBitStream(IntPtr bitStream);

        #endregion

        #region Native Other

        [DllImport("RakDotNetNative")]
        private static extern uint BitStreamGetNumberOfUnreadBits(IntPtr bitStream);

        [DllImport("RakDotNetNative")]
        private static extern IntPtr BitStreamGetData(IntPtr bitStream);

        [DllImport("RakDotNetNative")]
        private static extern uint BitStreamGetNumberOfBytesUsed(IntPtr bitStream);

        #endregion

        #region Native Reads

        [DllImport("RakDotNetNative")]
        private static extern IntPtr BitStreamReadBits(IntPtr bitStream, uint length, bool alignRight);

        [DllImport("RakDotNetNative")]
        private static extern sbyte BitStreamReadInt8(IntPtr bitStream);

        [DllImport("RakDotNetNative")]
        private static extern byte BitStreamReadUInt8(IntPtr bitStream);

        [DllImport("RakDotNetNative")]
        private static extern short BitStreamReadInt16(IntPtr bitStream);

        [DllImport("RakDotNetNative")]
        private static extern ushort BitStreamReadUInt16(IntPtr bitStream);

        [DllImport("RakDotNetNative")]
        private static extern int BitStreamReadInt32(IntPtr bitStream);

        [DllImport("RakDotNetNative")]
        private static extern uint BitStreamReadUInt32(IntPtr bitStream);

        [DllImport("RakDotNetNative")]
        private static extern long BitStreamReadInt64(IntPtr bitStream);

        [DllImport("RakDotNetNative")]
        private static extern ulong BitStreamReadUInt64(IntPtr bitStream);

        [DllImport("RakDotNetNative")]
        private static extern bool BitStreamReadBit(IntPtr bitStream);

        #endregion

        #region Native Writes

        [DllImport("RakDotNetNative")]
        private static extern void BitStreamWriteInt8(IntPtr bitStream, sbyte val);

        [DllImport("RakDotNetNative")]
        private static extern void BitStreamWriteUInt8(IntPtr bitStream, byte val);

        [DllImport("RakDotNetNative")]
        private static extern void BitStreamWriteInt16(IntPtr bitStream, short val);

        [DllImport("RakDotNetNative")]
        private static extern void BitStreamWriteUInt16(IntPtr bitStream, ushort val);

        [DllImport("RakDotNetNative")]
        private static extern void BitStreamWriteInt32(IntPtr bitStream, int val);

        [DllImport("RakDotNetNative")]
        private static extern void BitStreamWriteUInt32(IntPtr bitStream, uint val);

        [DllImport("RakDotNetNative")]
        private static extern void BitStreamWriteInt64(IntPtr bitStream, long val);

        [DllImport("RakDotNetNative")]
        private static extern void BitStreamWriteUInt64(IntPtr bitStream, ulong val);

        [DllImport("RakDotNetNative")]
        private static extern void BitStreamWriteBit(IntPtr bitStream, bool val);

        #endregion

        #region Native Compressed Reads

        [DllImport("RakDotNetNative")]
        private static extern sbyte BitStreamReadInt8Compressed(IntPtr bitStream);

        [DllImport("RakDotNetNative")]
        private static extern byte BitStreamReadUInt8Compressed(IntPtr bitStream);

        [DllImport("RakDotNetNative")]
        private static extern short BitStreamReadInt16Compressed(IntPtr bitStream);

        [DllImport("RakDotNetNative")]
        private static extern ushort BitStreamReadUInt16Compressed(IntPtr bitStream);

        [DllImport("RakDotNetNative")]
        private static extern int BitStreamReadInt32Compressed(IntPtr bitStream);

        [DllImport("RakDotNetNative")]
        private static extern uint BitStreamReadUInt32Compressed(IntPtr bitStream);

        [DllImport("RakDotNetNative")]
        private static extern long BitStreamReadInt64Compressed(IntPtr bitStream);

        [DllImport("RakDotNetNative")]
        private static extern ulong BitStreamReadUInt64Compressed(IntPtr bitStream);

        [DllImport("RakDotNetNative")]
        private static extern bool BitStreamReadBitCompressed(IntPtr bitStream);

        #endregion

        #region Native Compressed Writes

        [DllImport("RakDotNetNative")]
        private static extern void BitStreamWriteInt8Compressed(IntPtr bitStream, sbyte val);

        [DllImport("RakDotNetNative")]
        private static extern void BitStreamWriteUInt8Compressed(IntPtr bitStream, byte val);

        [DllImport("RakDotNetNative")]
        private static extern void BitStreamWriteInt16Compressed(IntPtr bitStream, short val);

        [DllImport("RakDotNetNative")]
        private static extern void BitStreamWriteUInt16Compressed(IntPtr bitStream, ushort val);

        [DllImport("RakDotNetNative")]
        private static extern void BitStreamWriteInt32Compressed(IntPtr bitStream, int val);

        [DllImport("RakDotNetNative")]
        private static extern void BitStreamWriteUInt32Compressed(IntPtr bitStream, uint val);

        [DllImport("RakDotNetNative")]
        private static extern void BitStreamWriteInt64Compressed(IntPtr bitStream, long val);

        [DllImport("RakDotNetNative")]
        private static extern void BitStreamWriteUInt64Compressed(IntPtr bitStream, ulong val);

        [DllImport("RakDotNetNative")]
        private static extern void BitStreamWriteBitCompressed(IntPtr bitStream, bool val);

        #endregion

        internal IntPtr ptr;

        #region Destructors/Constructors

        public BitStream()
        {
            ptr = InitializeBitStream1();
        }

        internal BitStream(IntPtr ptr)
        {
            this.ptr = ptr;
        }

        public BitStream(uint initialBytesToAllocate)
        {
            ptr = InitializeBitStream2(initialBytesToAllocate);
        }

        public BitStream(byte[] data, uint length, bool copyData = false)
        {
            var p = Marshal.AllocHGlobal(Marshal.SizeOf<byte>() * (int)length);

            Marshal.Copy(data, 0, p, (int)length);

            ptr = InitializeBitStream3(p, length, copyData);

            Marshal.FreeHGlobal(p);
        }

        public BitStream(byte[] data, bool copyData = false)
            : this(data, (uint)data.Length, copyData)
        {
        }

        ~BitStream()
        {
            Dispose();
        }

        public void Dispose()
        {
            DisposeBitStream(ptr);
            ptr = IntPtr.Zero;
        }

        #endregion

        #region Other

        public uint UnreadBitCount => BitStreamGetNumberOfUnreadBits(ptr);

        public uint BytesUsed => BitStreamGetNumberOfBytesUsed(ptr);

        public byte[] Data
        {
            get
            {
                var len = BytesUsed;
                var p = BitStreamGetData(ptr);
                var data = new byte[len];

                Marshal.Copy(p, data, 0, (int)len);

                return data;
            }
        }

        #endregion

        #region Reads

        public byte[] ReadBits(uint length, bool rightAlign = true)
        {
            var bytes = new byte[] { };

            Marshal.Copy(BitStreamReadBits(ptr, length, rightAlign), bytes, 0, (int)length);

            return bytes;
        }

        public sbyte ReadInt8()
        {
            return BitStreamReadInt8(ptr);
        }

        public byte ReadUInt8()
        {
            return BitStreamReadUInt8(ptr);
        }

        public short ReadInt16()
        {
            return BitStreamReadInt16(ptr);
        }

        public ushort ReadUInt16()
        {
            return BitStreamReadUInt16(ptr);
        }

        public int ReadInt32()
        {
            return BitStreamReadInt32(ptr);
        }

        public uint ReadUInt32()
        {
            return BitStreamReadUInt32(ptr);
        }

        public long ReadInt64()
        {
            return BitStreamReadInt64(ptr);
        }

        public ulong ReadUInt64()
        {
            return BitStreamReadUInt64(ptr);
        }

        public bool ReadBit()
        {
            return BitStreamReadBit(ptr);
        }

        #endregion

        #region Writes

        public void WriteInt8(sbyte val)
        {
            BitStreamWriteInt8(ptr, val);
        }

        public void WriteUInt8(byte val)
        {
            BitStreamWriteUInt8(ptr, val);
        }

        public void WriteInt16(short val)
        {
            BitStreamWriteInt16(ptr, val);
        }

        public void WriteUInt16(ushort val)
        {
            BitStreamWriteUInt16(ptr,val);
        }

        public void WriteInt32(int val)
        {
            BitStreamWriteInt32(ptr, val);
        }

        public void WriteUInt32(uint val)
        {
            BitStreamWriteUInt32(ptr, val);
        }

        public void WriteInt64(long val)
        {
            BitStreamWriteInt64(ptr, val);
        }

        public void WriteUInt64(ulong val)
        {
            BitStreamWriteUInt64(ptr, val);
        }

        public void WriteBit(bool val)
        {
            BitStreamWriteBit(ptr, val);
        }

        #endregion

        #region Compressed Reads

        public sbyte ReadInt8Compressed()
        {
            return BitStreamReadInt8Compressed(ptr);
        }

        public byte ReadUInt8Compressed()
        {
            return BitStreamReadUInt8Compressed(ptr);
        }

        public short ReadInt16Compressed()
        {
            return BitStreamReadInt16Compressed(ptr);
        }

        public ushort ReadUInt16Compressed()
        {
            return BitStreamReadUInt16Compressed(ptr);
        }

        public int ReadInt32Compressed()
        {
            return BitStreamReadInt32Compressed(ptr);
        }

        public uint ReadUInt32Compressed()
        {
            return BitStreamReadUInt32Compressed(ptr);
        }

        public long ReadInt64Compressed()
        {
            return BitStreamReadInt64Compressed(ptr);
        }

        public ulong ReadUInt64Compressed()
        {
            return BitStreamReadUInt64Compressed(ptr);
        }

        public bool ReadBitCompressed()
        {
            return BitStreamReadBitCompressed(ptr);
        }

        #endregion

        #region Compressed Writes

        public void WriteInt8Compressed(sbyte val)
        {
            BitStreamWriteInt8Compressed(ptr, val);
        }

        public void WriteUInt8Compressed(byte val)
        {
            BitStreamWriteUInt8Compressed(ptr, val);
        }

        public void WriteInt16Compressed(short val)
        {
            BitStreamWriteInt16Compressed(ptr, val);
        }

        public void WriteUInt16Compressed(ushort val)
        {
            BitStreamWriteUInt16Compressed(ptr,val);
        }

        public void WriteInt32Compressed(int val)
        {
            BitStreamWriteInt32Compressed(ptr, val);
        }

        public void WriteUInt32Compressed(uint val)
        {
            BitStreamWriteUInt32Compressed(ptr, val);
        }

        public void WriteInt64Compressed(long val)
        {
            BitStreamWriteInt64Compressed(ptr, val);
        }

        public void WriteUInt64Compressed(ulong val)
        {
            BitStreamWriteUInt64Compressed(ptr, val);
        }

        public void WriteBitCompressed(bool val)
        {
            BitStreamWriteBitCompressed(ptr, val);
        }

        #endregion

        #region Serializables

        public void WriteSerializable(Serializable serializable)
            => serializable.Serialize(this);

        public T ReadSerializable<T>() where T : Serializable 
            => (T)typeof(T).GetMethod("Deserialize").Invoke(null, new object[] { this });

        #endregion

        #region Strings

        public void WriteString(string str, uint length = 33)
        {
            for (var i = 0; i < length; i++)
                WriteInt8(i < str.Length ? (sbyte)str[i] : (sbyte)0);
        }

        public string ReadString(uint length = 33)
        {
            var str = "";
            var end = false;

            for (var i = 0; i < length; i++)
            {
                var c = ReadInt8();

                if (c == 0)
                    end = true;

                if (!end)
                    str += (char)c;
            }

            return str;
        }

        public void WriteWString(string str, uint length = 33)
        {
            for (var i = 0; i < length; i++)
            {
                WriteUInt8(i < str.Length ? (byte)str[i] : (byte)0);
                WriteUInt8(0);
            }
        }

        public string ReadWString(uint length = 33)
        {
            var str = "";
            var end = false;

            for (var i = 0; i < length; i++)
            {
                var c = ReadUInt8();
                ReadUInt8();

                if (c == 0)
                    end = true;

                if (!end)
                    str += (char)c;
            }

            return str;
        }

        #endregion
    }
}
