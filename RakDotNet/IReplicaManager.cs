using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace RakDotNet
{
    public interface IReplicaManager
    {
        void AddConnection(IRakConnection connection);

        void SendConstruction(IReplica replica, bool newReplica = true, ICollection<IPEndPoint> endPoints = null);

        void SendSerialization(IReplica replica, ICollection<IPEndPoint> endPoints = null);

        void SendDestruction(IReplica replica, ICollection<IPEndPoint> endPoints = null);
    }
}
