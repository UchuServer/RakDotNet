using System;
using System.Runtime.InteropServices;

namespace RakDotNet
{
    public class RakNetworkFactory
    {
        [DllImport("RakDotNetNative")]
        private static extern IntPtr RakNetworkFactoryGetRakPeerInterface();
        [DllImport("RakDotNetNative")]
        internal static extern void RakNetworkFactoryDestroyRakPeerInterface(IntPtr rakPeerInterface);
        [DllImport("RakDotNetNative")]
        private static extern IntPtr RakNetworkFactoryGetReplicaManager();
        [DllImport("RakDotNetNative")]
        internal static extern void RakNetworkFactoryDestroyReplicaManager(IntPtr replicaManager);

        public static RakPeerInterface GetRakPeerInterface() 
            => new RakPeerInterface(RakNetworkFactoryGetRakPeerInterface());
        public static void DestroyRakPeerInterface(RakPeerInterface rakPeerInterface)
            => RakNetworkFactoryDestroyRakPeerInterface(rakPeerInterface.ptr);
        public static ReplicaManager GetReplicaManager()
            => new ReplicaManager(RakNetworkFactoryGetReplicaManager());
        public static void DestroyReplicaManager(ReplicaManager replicaManager)
            => RakNetworkFactoryDestroyReplicaManager(replicaManager.ptr);
    }
}
