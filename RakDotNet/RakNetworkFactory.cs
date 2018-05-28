using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

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
