using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using RakDotNet.IO;

namespace RakDotNet
{
    public class ReplicaManager : IReplicaManager
    {
        private readonly ConcurrentDictionary<IPEndPoint, IRakConnection> _connections;
        private readonly ConcurrentDictionary<IReplica, ushort> _replicas;

        private ushort _networkId;

        public ReplicaManager()
        {
            _connections = new ConcurrentDictionary<IPEndPoint, IRakConnection>();
            _replicas = new ConcurrentDictionary<IReplica, ushort>();

            _networkId = 0;
        }

        public void AddConnection(IRakConnection connection)
        {
            if (!_connections.TryAdd(connection.EndPoint, connection)) return;
            
            foreach (var replica in _replicas.Keys) SendConstruction(replica, false, new[] {connection.EndPoint});

            connection.Disconnected += reason =>
            {
                _connections.TryRemove(connection.EndPoint, out _);

                return Task.CompletedTask;
            };
        }

        public void SendConstruction(IReplica replica, bool newReplica = true, ICollection<IPEndPoint> endPoints = null)
        {
            var recipients = endPoints ?? _connections.Keys;

            if (newReplica)
                _replicas[replica] = _networkId++;

            using var stream = new MemoryStream();
            using var writer = new BitWriter(stream);
            
            writer.Write((byte) MessageIdentifier.ReplicaManagerConstruction);
            writer.WriteBit(true);
            writer.Write(_replicas[replica]);

            replica.Construct(writer);

            foreach (var endPoint in recipients)
                if (_connections.TryGetValue(endPoint, out var conn))
                    conn.Send(stream.ToArray());
        }

        public void SendSerialization(IReplica replica, ICollection<IPEndPoint> endPoints = null)
        {
            var recipients = endPoints ?? _connections.Keys;

            using var stream = new MemoryStream();
            using var writer = new BitWriter(stream);
            
            writer.Write((byte) MessageIdentifier.ReplicaManagerSerialize);
            writer.Write(_replicas[replica]);
            writer.WriteSerializable(replica);

            foreach (var endPoint in recipients)
                if (_connections.TryGetValue(endPoint, out var conn))
                    conn.Send(stream.ToArray());
        }

        public void SendDestruction(IReplica replica, ICollection<IPEndPoint> endPoints = null)
        {
            var recipients = endPoints ?? _connections.Keys;

            using (var stream = new MemoryStream())
            {
                using var writer = new BitWriter(stream);
                
                writer.Write((byte) MessageIdentifier.ReplicaManagerDestruction);
                writer.Write(_replicas[replica]);

                replica.Destruct();

                foreach (var endPoint in recipients)
                    if (_connections.TryGetValue(endPoint, out var conn))
                        conn.Send(stream.ToArray());
            }

            _replicas.TryRemove(replica, out _);
        }
    }
}