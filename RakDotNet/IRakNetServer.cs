using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace RakDotNet
{
    public interface IRakNetServer
    {
        /// <summary>
        /// A game message is received from a client.
        /// </summary>
        event Action<IPEndPoint, byte[]> PacketReceived;
        
        /// <summary>
        /// A new client has connected to the server.
        /// </summary>
        event Action<IPEndPoint> NewConnection;
        
        /// <summary>
        /// A client has disconnected from the server.
        /// </summary>
        event Action<IPEndPoint> Disconnection;

        /// <summary>
        /// The protocol the server fallows.
        /// </summary>
        ServerProtocol Protocol { get; }

        /// <summary>
        /// Start the server.
        /// </summary>
        void Start();

        /// <summary>
        /// Close the server.
        /// </summary>
        void Stop();

        /// <summary>
        /// Close a client connection.
        /// </summary>
        /// <param name="endpoint"></param>
        void CloseConnection(IPEndPoint endpoint);

        void Send(Stream stream, IPEndPoint endpoint,
            PacketReliability reliability = PacketReliability.ReliableOrdered);

        void Send(Stream stream, ICollection<IPEndPoint> endpoints = null, bool broadcast = false,
            PacketReliability reliability = PacketReliability.ReliableOrdered);

        void Send(byte[] data, IPEndPoint endpoint, PacketReliability reliability = PacketReliability.ReliableOrdered);

        void Send(byte[] data, ICollection<IPEndPoint> endpoints = null, bool broadcast = false,
            PacketReliability reliability = PacketReliability.ReliableOrdered);
    }
}