using System;
using System.Runtime.InteropServices;

namespace RakDotNet
{
    public class Replica
    {
        [DllImport("RakDotNetNative")]
        private static extern IntPtr InitializeNativeReplica();
        [DllImport("RakDotNetNative")]
        private static extern void DisposeNativeReplica(IntPtr ptr);
        // TODO: add the other methods

        internal IntPtr ptr;

        public Replica()
        {
            ptr = InitializeNativeReplica();
        }

        ~Replica()
        {
            DisposeNativeReplica(ptr);
            ptr = IntPtr.Zero;
        }
    }
}
