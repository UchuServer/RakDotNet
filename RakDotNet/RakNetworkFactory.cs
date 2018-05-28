using System;
using System.Runtime.InteropServices;

namespace RakDotNet
{
    public class RakNetworkFactory
    {
        [DllImport("RakDotNetNative")]
        private static extern IntPtr RakNetworkFactoryGetRakPeerInterface();
        [DllImport("RakDotNetNative")]
        internal static extern void RakNetworkFactoryDestoryRakPeerInterface(IntPtr rakPeerInterface);

        public static RakPeerInterface GetRakPeerInterface() 
            => new RakPeerInterface(RakNetworkFactoryGetRakPeerInterface());
        public static void DestroyRakPeerInterface(RakPeerInterface rakPeerInterface)
            => RakNetworkFactoryDestoryRakPeerInterface(rakPeerInterface.ptr);
    }
}
