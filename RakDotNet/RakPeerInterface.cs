using System;
using System.Runtime.InteropServices;

namespace RakDotNet
{
    public class RakPeerInterface
    {
        [DllImport("RakDotNetNative")]
        private static extern bool RakPeerInterfaceStartup(IntPtr ptr, ushort maxConnections, int threadSleepTimer, IntPtr socketDescriptor);
        [DllImport("RakDotNetNative")]
        private static extern void RakPeerInterfaceInitializeSecurity(IntPtr ptr, string pubKeyE, string pubKeyN, string privKeyP, string privKeyQ);
        [DllImport("RakDotNetNative")]
        private static extern void RakPeerInterfaceDisableSecurity(IntPtr ptr);
        [DllImport("RakDotNetNative")]
        private static extern void RakPeerInterfaceSetMaximumIncomingConnections(IntPtr ptr, ushort connections);
        [DllImport("RakDotNetNative")]
        private static extern void RakPeerInterfaceSetIncomingPassword(IntPtr ptr, string password, int length);
        [DllImport("RakDotNetNative")]
        private static extern void RakPeerInterfaceShutdown(IntPtr ptr, uint blockDuration, sbyte orderingChannel);
        [DllImport("RakDotNetNative")]
        private static extern bool RakPeerInterfaceIsActive(IntPtr ptr);
        [DllImport("RakDotNetNative")]
        private static extern bool RakPeerInterfaceSend1(IntPtr ptr, IntPtr data, int length, int priority, int reliability, sbyte orderingChannel, IntPtr systemAddress, bool broadcast);
        [DllImport("RakDotNetNative")]
        private static extern bool RakPeerInterfaceSend2(IntPtr ptr, IntPtr bitStream, int priority, int reliability, sbyte orderingChannel, IntPtr systemAddress, bool broadcast);
        [DllImport("RakDotNetNative")]
        private static extern IntPtr RakPeerInterfaceReceive(IntPtr ptr);
        [DllImport("RakDotNetNative")]
        internal static extern void RakPeerInterfaceDeallocatePacket(IntPtr ptr, IntPtr packet);

        internal IntPtr ptr;

        public bool Active => RakPeerInterfaceIsActive(ptr);
        public string Password
        {
            set => RakPeerInterfaceSetIncomingPassword(ptr, value, value.Length);
        }
        public ushort MaxIncomingConnections
        {
            set => RakPeerInterfaceSetMaximumIncomingConnections(ptr, value);
        }

        internal RakPeerInterface(IntPtr rakPeer)
        {
            ptr = rakPeer;
        }

        ~RakPeerInterface()
        {
            RakNetworkFactory.RakNetworkFactoryDestoryRakPeerInterface(ptr);
        }

        public bool Startup(ushort maxConnections, int threadSleepTimer, SocketDescriptor socketDescriptor)
            => RakPeerInterfaceStartup(ptr, maxConnections, threadSleepTimer, socketDescriptor.ptr);

        public void InitializeSecurity(string pubKeyE, string pubKeyN, string privKeyP, string privKeyQ)
            => RakPeerInterfaceInitializeSecurity(ptr, pubKeyE, pubKeyN, privKeyP, privKeyQ);

        public void DisableSecurity() => RakPeerInterfaceDisableSecurity(ptr);

        public void Shutdown(uint blockDuration, sbyte orderingChannel) 
            => RakPeerInterfaceShutdown(ptr, blockDuration, orderingChannel);

        public bool Send(byte[] data, int length, /* PacketPriority */ int priority, /* PacketReliability */ int reliability, sbyte orderingChannel, SystemAddress systemAddress, bool broadcast = false)
        {
            var p = Marshal.AllocHGlobal(Marshal.SizeOf<byte>() * data.Length);

            Marshal.Copy(data, 0, p, data.Length);

            try
            {
                return RakPeerInterfaceSend1(ptr, p, length, priority, reliability, orderingChannel, systemAddress.ptr, broadcast);
            }
            finally
            {
                Marshal.FreeHGlobal(p);
            }
        }

        public bool Send(BitStream stream, /* PacketPriority */ int priority, /* PacketReliability */ int reliability, sbyte orderingChannel, SystemAddress systemAddress, bool broadcast = false) 
            => RakPeerInterfaceSend2(ptr, stream.ptr, priority, reliability, orderingChannel, systemAddress.ptr, broadcast);

        public Packet Receive() => new Packet(RakPeerInterfaceReceive(ptr), ptr);
        public void DeallocatePacket(Packet packet) => RakPeerInterfaceDeallocatePacket(ptr, packet.ptr);
    }
}
