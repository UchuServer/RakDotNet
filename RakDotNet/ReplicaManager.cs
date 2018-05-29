using System;
using System.Runtime.InteropServices;

namespace RakDotNet
{
    public class ReplicaManager
    {
        [DllImport("RakDotNetNative")]
        private static extern IntPtr InitializeReplicaManager();
        [DllImport("RakDotNetNative")]
        private static extern void DisposeReplicaManager(IntPtr ptr);
        // TODO: add the other methods

        internal IntPtr ptr;

        public ReplicaManager()
        {
            ptr = InitializeReplicaManager();
        }

        ~ReplicaManager()
        {
            DisposeReplicaManager(ptr);
            ptr = IntPtr.Zero;
        }
    }
}
