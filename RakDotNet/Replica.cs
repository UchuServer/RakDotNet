using System;
using System.Runtime.InteropServices;

namespace RakDotNet
{
    public abstract class Replica
    {
        private delegate void NativeReplicaConstructCallback(uint time, IntPtr address, uint flags, IntPtr stream, bool includeTimestamp);
        private delegate void NativeReplicaScopeChangeCallback(bool inScope, IntPtr stream, uint time, IntPtr address, bool includeTimestamp);
        private delegate void NativeReplicaSerializeCallback(bool sendTimestamp, IntPtr stream, uint lastSendTime, int priority, int reliability, uint currentTime, IntPtr address, uint flags);

        [DllImport("RakDotNetNative")]
        private static extern IntPtr InitializeNativeReplica();
        [DllImport("RakDotNetNative")]
        private static extern void DisposeNativeReplica(IntPtr ptr);
        [DllImport("RakDotNetNative")]
        private static extern void NativeReplicaSetConstructCallback(IntPtr ptr, [MarshalAs(UnmanagedType.FunctionPtr)] NativeReplicaConstructCallback callback);
        [DllImport("RakDotNetNative")]
        private static extern void NativeReplicaSetScopeChangeCallback(IntPtr ptr, [MarshalAs(UnmanagedType.FunctionPtr)] NativeReplicaScopeChangeCallback callback);
        [DllImport("RakDotNetNative")]
        private static extern void NativeReplicaSetSerializeCallback(IntPtr ptr, [MarshalAs(UnmanagedType.FunctionPtr)] NativeReplicaSerializeCallback callback);

        internal IntPtr ptr;

        public Replica()
        {
            ptr = InitializeNativeReplica();

            NativeReplicaSetConstructCallback(ptr, (time, address, flags, stream, includeTimestamp) =>
            {
                SendConstruct(DateTimeOffset.FromUnixTimeMilliseconds(time), new SystemAddress(address), flags, new BitStream(stream), includeTimestamp);
            });

            NativeReplicaSetScopeChangeCallback(ptr, (inScope, stream, time, address, includeTimestamp) =>
            {
                SendScopeChange(inScope, new BitStream(stream), DateTimeOffset.FromUnixTimeMilliseconds(time), new SystemAddress(address), includeTimestamp);
            });

            NativeReplicaSetSerializeCallback(ptr, (sendTimestamp, stream, lastSendTime, priority, reliability, currentTime, address, flags) =>
            {
                Serialize(sendTimestamp, new BitStream(stream), DateTimeOffset.FromUnixTimeMilliseconds(lastSendTime), priority, reliability, DateTimeOffset.FromUnixTimeMilliseconds(currentTime), new SystemAddress(address), flags);
            });
        }

        ~Replica()
        {
            DisposeNativeReplica(ptr);
            ptr = IntPtr.Zero;
        }

        public abstract void SendConstruct(DateTimeOffset time, SystemAddress address, uint flags, BitStream stream, bool includeTimestamp);
        public abstract void SendScopeChange(bool inScope, BitStream stream, DateTimeOffset time, SystemAddress address, bool includeTimestamp);
        public abstract void Serialize(bool sendTimestamp, BitStream stream, DateTimeOffset lastSendTime, int priority, int reliability, DateTimeOffset currentTime, SystemAddress address, uint flags);
    }
}
