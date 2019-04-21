using System;
using System.Collections.Generic;
using System.Net;

namespace RakDotNet
{
    public interface IRakNetServer
    {
        event Action<IPEndPoint, byte[]> PacketReceived;
        event Action<IPEndPoint> NewConnection;
        event Action<IPEndPoint> Disconnection;

        void Start();

        void Stop();

        void CloseConnection(IPEndPoint endpoint);

        void Send(BitStream stream, IPEndPoint endpoint,
            PacketReliability reliability = PacketReliability.ReliableOrdered);

        void Send(BitStream stream, ICollection<IPEndPoint> endpoints = null, bool broadcast = false,
            PacketReliability reliability = PacketReliability.ReliableOrdered);

        void Send(byte[] data, IPEndPoint endpoint, PacketReliability reliability = PacketReliability.ReliableOrdered);

        void Send(byte[] data, ICollection<IPEndPoint> endpoints = null, bool broadcast = false,
            PacketReliability reliability = PacketReliability.ReliableOrdered);
    }
}