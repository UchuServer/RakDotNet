using System;
using System.Runtime.InteropServices;

namespace RakDotNet
{
    public class SystemAddress
    {
        [DllImport("RakDotNetNative")]
        private static extern IntPtr InitializeSystemAddress();
        [DllImport("RakDotNetNative")]
        private static extern uint SystemAddressGetBinaryAddress(IntPtr systemAddress);
        [DllImport("RakDotNetNative")]
        private static extern ushort SystemAddressGetPort(IntPtr systemAddress);
        [DllImport("RakDotNetNative")]
        private static extern void SystemAddressSetBinaryAddress(IntPtr systemAdress, string address);
        [DllImport("RakDotNetNative")]
        private static extern void SystemAddressSetPort(IntPtr systemAddress, ushort port);
        [DllImport("RakDotNetNative")]
        private static extern string SystemAddressToString(IntPtr systemAddress, bool writePort);

        internal IntPtr ptr;

        public uint BinaryAddress => SystemAddressGetBinaryAddress(ptr);
 
        public ushort Port
        {
            get => SystemAddressGetPort(ptr);
            set => SystemAddressSetPort(ptr, value);
        }

        public SystemAddress()
        {
            ptr = InitializeSystemAddress();
        }

        internal SystemAddress(IntPtr systemAddress)
        {
            ptr = systemAddress;
        }

        public override string ToString() => SystemAddressToString(ptr, true);
        public string ToString(bool writePort) => SystemAddressToString(ptr, writePort);

        public void SetBinaryAddress(string address) => SystemAddressSetBinaryAddress(ptr, address);
    }
}
