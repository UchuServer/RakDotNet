using System;
using System.Runtime.InteropServices;

namespace RakDotNet
{
    public class BitStream : IDisposable
    {
        [DllImport("RakDotNetNative")]
        private static extern IntPtr InitializeBitStream1();

        [DllImport("RakDotNetNative")]
        private static extern IntPtr InitializeBitStream2(uint initialBytesToAllocate);

        [DllImport("RakDotNetNative")]
        private static extern IntPtr InitializeBitStream3(IntPtr data, uint length, bool copyData);

        [DllImport("RakDotNetNative")]
        private static extern void DisposeBitStream(IntPtr bitStream);

        [DllImport("RakDotNetNative")]
        private static extern sbyte ReadInt8(IntPtr bitStream);

        [DllImport("RakDotNetNative")]
        private static extern byte ReadUInt8(IntPtr bitStream);

        [DllImport("RakDotNetNative")]
        private static extern short ReadInt16(IntPtr bitStream);

        [DllImport("RakDotNetNative")]
        private static extern ushort ReadUInt16(IntPtr bitStream);

        [DllImport("RakDotNetNative")]
        private static extern int ReadInt32(IntPtr bitStream);

        [DllImport("RakDotNetNative")]
        private static extern uint ReadUInt32(IntPtr bitStream);

        [DllImport("RakDotNetNative")]
        private static extern long ReadInt64(IntPtr bitStream);

        [DllImport("RakDotNetNative")]
        private static extern ulong ReadUInt64(IntPtr bitStream);

        [DllImport("RakDotNetNative")]
        private static extern bool ReadBit(IntPtr bitStream);

        [DllImport("RakDotNetNative")]
        private static extern void WriteInt8(IntPtr bitStream, sbyte val);

        [DllImport("RakDotNetNative")]
        private static extern void WriteUInt8(IntPtr bitStream, byte val);

        [DllImport("RakDotNetNative")]
        private static extern void WriteInt16(IntPtr bitStream, short val);

        [DllImport("RakDotNetNative")]
        private static extern void WriteUInt16(IntPtr bitStream, ushort val);

        [DllImport("RakDotNetNative")]
        private static extern void WriteInt32(IntPtr bitStream, int val);

        [DllImport("RakDotNetNative")]
        private static extern void WriteUInt32(IntPtr bitStream, uint val);

        [DllImport("RakDotNetNative")]
        private static extern void WriteInt64(IntPtr bitStream, long val);

        [DllImport("RakDotNetNative")]
        private static extern void WriteUInt64(IntPtr bitStream, ulong val);

        [DllImport("RakDotNetNative")]
        private static extern void WriteBit(IntPtr bitStream, bool val);
        
        private IntPtr ptr;

        public BitStream()
        {
            ptr = InitializeBitStream1();
        }

        public BitStream(uint initialBytesToAllocate)
        {
            ptr = InitializeBitStream2(initialBytesToAllocate);
        }

        public BitStream(byte[] data, uint length, bool copyData)
        {
            var p = Marshal.AllocHGlobal(Marshal.SizeOf<byte>() * data.Length);

            Marshal.Copy(data, 0, p, data.Length);

            ptr = InitializeBitStream3(p, length, copyData);

            Marshal.FreeHGlobal(p);
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

        public sbyte ReadInt8()
        {
            return ReadInt8(ptr);
        }

        public byte ReadUInt8()
        {
            return ReadUInt8(ptr);
        }

        public short ReadInt16()
        {
            return ReadInt16(ptr);
        }

        public ushort ReadUInt16()
        {
            return ReadUInt16(ptr);
        }

        public int ReadInt32()
        {
            return ReadInt32(ptr);
        }

        public uint ReadUInt32()
        {
            return ReadUInt32(ptr);
        }

        public long ReadInt64()
        {
            return ReadInt64(ptr);
        }

        public ulong ReadUInt64()
        {
            return ReadUInt64(ptr);
        }

        public bool ReadBit()
        {
            return ReadBit(ptr);
        }

        public void WriteInt8(sbyte val)
        {
            WriteInt8(ptr, val);
        }

        public  void WriteUInt8(byte val)
        {
            WriteUInt8(ptr, val);
        }

        public void WriteInt16(short val)
        {
            WriteInt16(ptr, val);
        }

        public void WriteUInt16(ushort val)
        {
            WriteUInt16(ptr,val);
        }

        public void WriteInt32(int val)
        {
            WriteInt32(ptr, val);
        }

        public void WriteUInt32(uint val)
        {
            WriteUInt32(ptr, val);
        }

        public void WriteInt64(long val)
        {
            WriteInt64(ptr, val);
        }

        public void WriteUInt64(ulong val)
        {
            WriteUInt64(ptr, val);
        }

        public void WriteBit(bool val)
        {
            WriteBit(ptr, val);
        }
    }
}
