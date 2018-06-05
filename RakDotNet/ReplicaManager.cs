using System;
using System.Runtime.InteropServices;

namespace RakDotNet
{
    public class ReplicaManager
    {
        [DllImport("RakDotNetNative")]
        private static extern void ReplicaManagerSetAutoParticipateNewConnections(IntPtr ptr, bool autoAdd);
        [DllImport("RakDotNetNative")]
        private static extern bool ReplicaManagerAddParticipant(IntPtr ptr, IntPtr participant);
        [DllImport("RakDotNetNative")]
        private static extern bool ReplicaManagerRemoveParticipant(IntPtr ptr, IntPtr participant);
        [DllImport("RakDotNetNative")]
        private static extern void ReplicaManagerConstruct(IntPtr ptr, IntPtr replica, bool copy, IntPtr address, bool broadcast);
        [DllImport("RakDotNetNative")]
        private static extern void ReplicaManagerDestruct(IntPtr ptr, IntPtr replica, IntPtr address, bool broadcast);
        [DllImport("RakDotNetNative")]
        private static extern void ReplicaManagerReferencePointer(IntPtr ptr, IntPtr replica);
        [DllImport("RakDotNetNative")]
        private static extern void ReplicaManagerDereferencePointer(IntPtr ptr, IntPtr replica);
        [DllImport("RakDotNetNative")]
        private static extern void ReplicaManagerSetScope(IntPtr ptr, IntPtr replica, bool inScope, IntPtr address, bool broadcast);
        [DllImport("RakDotNetNative")]
        private static extern void ReplicaManagerSignalSerializeIfNeeded(IntPtr ptr, IntPtr replica, IntPtr address, bool broadcast);
        [DllImport("RakDotNetNative")]
        private static extern void ReplicaManagerSetSendChannel(IntPtr ptr, byte channel);
        [DllImport("RakDotNetNative")]
        private static extern void ReplicaManagerSetAutoConstructToNewParticipants(IntPtr ptr, bool autoConstruct);
        [DllImport("RakDotNetNative")]
        private static extern void ReplicaManagerSetDefaultScope(IntPtr ptr, bool scope);
        [DllImport("RakDotNetNative")]
        private static extern void ReplicaManagerSetAutoSerializeInScope(IntPtr ptr, bool autoSerialize);
        [DllImport("RakDotNetNative")]
        private static extern void ReplicaManagerUpdate(IntPtr ptr, IntPtr rakPeer);
        [DllImport("RakDotNetNative")]
        private static extern void ReplicaManagerEnableReplicaInterfaces(IntPtr ptr, IntPtr replica, byte flags);
        [DllImport("RakDotNetNative")]
        private static extern void ReplicaManagerDisableReplicaInterfaces(IntPtr ptr, IntPtr replica, byte flags);
        [DllImport("RakDotNetNative")]
        private static extern bool ReplicaManagerIsConstructed(IntPtr ptr, IntPtr replica, IntPtr address);
        [DllImport("RakDotNetNative")]
        private static extern bool ReplicaManagerIsInScope(IntPtr ptr, IntPtr replica, IntPtr address);
        [DllImport("RakDotNetNative")]
        private static extern uint ReplicaManagerGetReplicaCount(IntPtr ptr);
        [DllImport("RakDotNetNative")]
        private static extern uint ReplicaManagerGetParticipantCount(IntPtr ptr);
        [DllImport("RakDotNetNative")]
        private static extern bool ReplicaManagerHasParticipant(IntPtr ptr, IntPtr address);

        internal IntPtr ptr;

        public bool AutoParticipateNewConnections
        {
            set => ReplicaManagerSetAutoParticipateNewConnections(ptr, value);
        }
        public byte SendChannel
        {
            set => ReplicaManagerSetSendChannel(ptr, value);
        }
        public bool AutoConstructToNewParticipants
        {
            set => ReplicaManagerSetAutoConstructToNewParticipants(ptr, value);
        }
        public bool DefaultScope
        {
            set => ReplicaManagerSetDefaultScope(ptr, value);
        }
        public bool AutoSerializeInScope
        {
            set => ReplicaManagerSetAutoSerializeInScope(ptr, value);
        }
        public uint ReplicaCount => ReplicaManagerGetReplicaCount(ptr);
        public uint ParticipantCount => ReplicaManagerGetParticipantCount(ptr);

        internal ReplicaManager(IntPtr ptr)
        {
            this.ptr = ptr;
        }

        ~ReplicaManager()
        {
            RakNetworkFactory.RakNetworkFactoryDestroyReplicaManager(ptr);
            ptr = IntPtr.Zero;
        }

        public bool AddParticipant(SystemAddress address)
            => ReplicaManagerAddParticipant(ptr, address.ptr);
        public bool RemoveParticipant(SystemAddress address)
            => ReplicaManagerRemoveParticipant(ptr, address.ptr);
        public void Construct(Replica replica, bool copy, SystemAddress address, bool broadcast = false)
            => ReplicaManagerConstruct(ptr, replica.ptr, copy, address.ptr, broadcast);
        public void Destruct(Replica replica, SystemAddress address, bool broadcast = false)
            => ReplicaManagerDestruct(ptr, replica.ptr, address.ptr, broadcast);
        public void ReferencePointer(Replica replica)
            => ReplicaManagerReferencePointer(ptr, replica.ptr);
        public void DereferencePointer(Replica replica)
            => ReplicaManagerDereferencePointer(ptr, replica.ptr);
        public void SetScope(Replica replica, bool inScope, SystemAddress address, bool broadcast = false)
            => ReplicaManagerSetScope(ptr, replica.ptr, inScope, address.ptr, broadcast);
        public void SignalSerializeIfNeeded(Replica replica, SystemAddress address, bool broadcast = false)
            => ReplicaManagerSignalSerializeIfNeeded(ptr, replica.ptr, address.ptr, broadcast);
        public void Update(RakPeerInterface rakPeer) 
            => ReplicaManagerUpdate(ptr, rakPeer.ptr);
        public void EnableReplicaInterfaces(Replica replica, byte flags) 
            => ReplicaManagerEnableReplicaInterfaces(ptr, replica.ptr, flags);
        public void DisableReplicaInterfaces(Replica replica, byte flags) 
            => ReplicaManagerDisableReplicaInterfaces(ptr, replica.ptr, flags);
        public bool IsConstructed(Replica replica, SystemAddress address)
            => ReplicaManagerIsConstructed(ptr, replica.ptr, address.ptr);
        public bool IsInScope(Replica replica, SystemAddress address)
            => ReplicaManagerIsInScope(ptr, replica.ptr, address.ptr);
        public bool HasParticipant(SystemAddress address)
            => ReplicaManagerHasParticipant(ptr, address.ptr);
    }
}
