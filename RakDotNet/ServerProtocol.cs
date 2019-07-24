using System;

namespace RakDotNet
{
    public enum ServerProtocol
    {
        Custom,
        
        /// <summary>
        /// UDP only Protocol based on RakNet 3.25
        /// </summary>
        [Obsolete("The RakNet Protocol is no longer supported. Use ServerProtocol.TcpUdp instead.")]
        RakNet,
        
        /// <summary>
        /// Protocol with UDP and TCP capabilities.
        /// </summary>
        TcpUdp
    }
}
