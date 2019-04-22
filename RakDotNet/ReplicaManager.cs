using System.Collections.Generic;
using System.Net;

namespace RakDotNet
{
    public class ReplicaManager
    {
        private readonly IRakNetServer _server;
        private readonly List<IPEndPoint> _connected;
        private readonly Dictionary<IReplica, ushort> _replicas;

        private ushort _networkId;

        public ReplicaManager(IRakNetServer server)
        {
            _server = server;
            _connected = new List<IPEndPoint>();
            _replicas = new Dictionary<IReplica, ushort>();

            _server.Disconnection += endpoint =>
            {
                if (_connected.Contains(endpoint))
                    _connected.Remove(endpoint);
            };
        }

        public void AddConnection(IPEndPoint endpoint)
        {
            _connected.Add(endpoint);

            foreach (var replica in _replicas.Keys)
            {
                SendConstruction(replica, false, new[] {endpoint});
            }
        }

        public void SendConstruction(IReplica replica, bool newReplica = true, ICollection<IPEndPoint> endpoints = null)
        {
            var recipients = endpoints ?? _connected.ToArray();

            if (newReplica)
                _replicas[replica] = _networkId++;

            var stream = new BitStream();

            stream.WriteByte((byte) MessageIdentifiers.ReplicaManagerConstruction);
            stream.WriteBit(true);
            stream.WriteUShort(_replicas[replica]);

            replica.Construct(stream);

            _server.Send(stream, recipients);
        }

        public void SendSerialization(IReplica replica, ICollection<IPEndPoint> endpoints = null)
        {
            var recipients = endpoints ?? _connected.ToArray();
            var stream = new BitStream();

            stream.WriteByte((byte) MessageIdentifiers.ReplicaManagerSerialize);
            stream.WriteUShort(_replicas[replica]);
            stream.WriteSerializable(replica);

            _server.Send(stream, recipients);
        }

        public void SendDestruction(IReplica replica, ICollection<IPEndPoint> endpoints = null)
        {
            var recipients = endpoints ?? _connected.ToArray();
            var stream = new BitStream();

            stream.WriteByte((byte) MessageIdentifiers.ReplicaManagerDestruction);
            stream.WriteUShort(_replicas[replica]);
            replica.Destruct();

            _server.Send(stream, recipients);

            _replicas.Remove(replica);
        }
    }
}