using System;
using System.Runtime.InteropServices;

namespace RakDotNet
{
    public class SocketDescriptor
    {
        [DllImport("RakDotNetNative")]
        private static extern IntPtr InitializeSocketDescriptor1();
        [DllImport("RakDotNetNative")]
        private static extern IntPtr InitializeSocketDescriptor2(ushort port, string hostAddress);
        [DllImport("RakDotNetNative")]
        private static extern ushort SocketDescriptorGetPort(IntPtr socketDescriptor);
        [DllImport("RakDotNetNative")]
        private static extern string SocketDescriptorGetHostAddress(IntPtr socketDescriptor);

        internal IntPtr ptr;

        public ushort Port => SocketDescriptorGetPort(ptr);
        public string HostAddress => SocketDescriptorGetHostAddress(ptr);

        public SocketDescriptor()
        {
            ptr = InitializeSocketDescriptor1();
        }

        public SocketDescriptor(ushort port, string hostAddress)
        {
            ptr = InitializeSocketDescriptor2(port, hostAddress);
        }
    }
}
