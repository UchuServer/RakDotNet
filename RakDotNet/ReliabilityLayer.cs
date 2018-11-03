using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace RakDotNet
{
    public class ReliabilityLayer
    {
        private const int MtuSize = 1228;
        private const int UdpHeaderSize = 28;

        private readonly UdpClient _udp;
        private readonly IPEndPoint _endpoint;
        private readonly RangeList<uint> _acks;
        private readonly List<uint> _lastReliabilityReceived;
        private readonly Dictionary<uint, byte[]> _outOfOrderPackets;
        private readonly Dictionary<uint, byte[][]> _splitPacketQueue;
        private readonly List<InternalPacket> _sends;
        private readonly Dictionary<uint, Resend> _resends;
        private readonly long _startTime;

        private bool _active;
        private float _lastAckTime;
        private uint _splitPacketId;
        private long _remoteSystemTime;
        private float _retransmissionTimeout;
        private float _smoothedRoundTripTime;
        private float _roundTripTimeVariation;
        private float _congestionWindow;
        private float _slowStartThreshold;
        private int _sent;
        private uint _sendIndex;
        private int _sequencedWriteIndex;
        private int _sequencedReadIndex;
        private int _orderedWriteIndex;
        private int _orderedReadIndex;

        public float LastAckTime => _lastAckTime;
        public bool Resends => _resends.Count > 0;
        public bool Active => _active;

        public ReliabilityLayer(UdpClient udp, IPEndPoint endpoint)
        {
            _udp = udp;
            _endpoint = endpoint;
            _startTime = Environment.TickCount;
            _acks = new UIntRangeList();
            _lastReliabilityReceived = new List<uint>();
            _outOfOrderPackets = new Dictionary<uint, byte[]>();
            _splitPacketQueue = new Dictionary<uint, byte[][]>();
            _sends = new List<InternalPacket>();
            _resends = new Dictionary<uint, Resend>();
            _active = false;
            _slowStartThreshold = float.PositiveInfinity;
            _smoothedRoundTripTime = -1;
        }

        public IEnumerable<byte[]> HandleDatagram(byte[] buffer)
        {
            if (buffer.Length <= 2)
                yield break;

            var stream = new BitStream(buffer);

            if (stream.ReadBit()) // has acks
            {
                var ourSystemTime = stream.ReadUInt();

                var roundTripTime = Environment.TickCount / 1000f - _startTime / 1000f - ourSystemTime / 1000f;

                if (_smoothedRoundTripTime.Equals(-1))
                {
                    _smoothedRoundTripTime = roundTripTime;
                    _roundTripTimeVariation = roundTripTime / 2f;
                }
                else
                {
                    const float alpha = 0.125f;
                    const float beta = 0.25f;

                    _roundTripTimeVariation = (long) ((1 - beta) * _roundTripTimeVariation +
                                                      beta * Math.Abs(_smoothedRoundTripTime - roundTripTime));
                    _smoothedRoundTripTime = (long) ((1 - alpha) * _smoothedRoundTripTime + alpha * roundTripTime);
                }

                _retransmissionTimeout = Math.Max(1, _smoothedRoundTripTime + 4 * _roundTripTimeVariation);

                var acks = new UIntRangeList();

                stream.ReadSerializable(acks);

                foreach (var ack in acks)
                {
                    if (_resends.ContainsKey(ack))
                        _resends.Remove(ack);
                }

                var actHoles = acks.GetHoles().Count(hole => _resends.ContainsKey(hole));

                if (actHoles > 0)
                {
                    _slowStartThreshold = _congestionWindow / 2;
                    _congestionWindow = _slowStartThreshold;
                }
                else
                {
                    if (_sent >= _congestionWindow)
                    {
                        _congestionWindow += acks.Count > _slowStartThreshold && _congestionWindow > 0
                            ? acks.Count / _congestionWindow
                            : acks.Count;
                    }
                }

                _sent = 0;
                _lastAckTime = Environment.TickCount / 1000f;
            }

            if (stream.AllRead)
                yield break;

            if (stream.ReadBit())
                _remoteSystemTime = stream.ReadUInt();

            while (!stream.AllRead)
            {
                var internalPacket = new InternalPacket();

                stream.ReadSerializable(internalPacket);

                if (internalPacket.Reliability == PacketReliability.ReliableOrdered ||
                    internalPacket.Reliability == PacketReliability.Reliable)
                    _acks.Add(internalPacket.MessageNumber);

                if (internalPacket.SplitPacket)
                {
                    if (!_splitPacketQueue.ContainsKey(internalPacket.SplitPacketId))
                        _splitPacketQueue[internalPacket.SplitPacketId] = new byte[internalPacket.SplitPacketCount][];

                    var splitPacket = _splitPacketQueue[internalPacket.SplitPacketId];

                    splitPacket[internalPacket.SplitPacketIndex] = internalPacket.Data;

                    if (splitPacket.All(a => a != null))
                    {
                        internalPacket.Data = splitPacket.SelectMany(b => b).ToArray();

                        _splitPacketQueue.Remove(internalPacket.SplitPacketId);

                        internalPacket.SplitPacket = false;
                    }
                    else
                        continue;
                }

                if (internalPacket.Reliability == PacketReliability.ReliableOrdered)
                {
                    if (!_lastReliabilityReceived.Contains(internalPacket.MessageNumber))
                    {
                        _lastReliabilityReceived.Add(internalPacket.MessageNumber);
                    }
                    else
                        continue;
                }

                if (internalPacket.Reliability == PacketReliability.UnreliableSequenced)
                {
                    if (internalPacket.OrderingIndex >= _sequencedReadIndex)
                        _sequencedReadIndex = (int) internalPacket.OrderingIndex + 1;
                    else
                        continue;
                }
                else if (internalPacket.Reliability == PacketReliability.ReliableOrdered)
                {
                    if (internalPacket.OrderingIndex == _orderedReadIndex)
                    {
                        _orderedReadIndex++;

                        for (var i = internalPacket.OrderingChannel + 1u; _outOfOrderPackets.ContainsKey(i); i++)
                        {
                            _orderedReadIndex++;

                            yield return _outOfOrderPackets[i];

                            _outOfOrderPackets.Remove(i);
                        }
                    }
                    else if (internalPacket.OrderingIndex < _orderedReadIndex)
                        continue;
                    else
                    {
                        _outOfOrderPackets[internalPacket.OrderingIndex] = internalPacket.Data;
                    }
                }

                yield return internalPacket.Data;
            }
        }

        public void StartSendLoop()
        {
            _active = true;

            Task.Run(async () =>
            {
                while (_active)
                {
                    await Task.Delay(30);

                    var newResends = new Dictionary<uint, Resend>();

                    foreach (var messageNum in _resends.Keys)
                    {
                        var resend = _resends[messageNum];

                        if (resend.Time > Environment.TickCount / 1000f)
                            continue;

                        if (_sent >= _congestionWindow)
                            break;

                        _sent++;

                        resend.Packet.MessageNumber = messageNum;

                        await _sendPacketAsync(resend.Packet).ConfigureAwait(false);

                        if (resend.Packet.Reliability == PacketReliability.Reliable ||
                            resend.Packet.Reliability == PacketReliability.ReliableOrdered)
                            newResends[messageNum] = new Resend
                            {
                                Time = Environment.TickCount / 1000f + _retransmissionTimeout,
                                Packet = resend.Packet
                            };
                    }

                    foreach (var entry in newResends)
                        _resends[entry.Key] = entry.Value;

                    while (_sends.Count > 0)
                    {

                        if (_sent > _congestionWindow)
                            break;

                        var packet = _sends[0];
                        _sends.RemoveAt(0);
                        _sent++;

                        var messageNum = _sendIndex++;

                        packet.MessageNumber = messageNum;

                        await _sendPacketAsync(packet).ConfigureAwait(false);

                        if (packet.Reliability == PacketReliability.Reliable ||
                            packet.Reliability == PacketReliability.ReliableOrdered)
                            _resends[messageNum] = new Resend
                            {
                                Time = Environment.TickCount / 1000f + _retransmissionTimeout,
                                Packet = packet
                            };
                    }

                    if (_acks.Count <= 0) continue;

                    var ack = new BitStream();

                    ack.WriteBit(true);
                    ack.WriteUInt((uint) _remoteSystemTime);
                    ack.WriteSerializable(_acks);

                    _acks.Clear();

                    await _udp.SendAsync(ack.BaseBuffer, ack.BaseBuffer.Length, _endpoint).ConfigureAwait(false);
                }
            });
        }

        public void StopSendLoop()
        {
            _active = false;
        }

        private async Task _sendPacketAsync(InternalPacket packet)
        {
            var stream = new BitStream();

            var hasAcks = _acks.RangeCount > 0;

            stream.WriteBit(hasAcks);

            if (hasAcks)
            {
                stream.WriteUInt((uint) _remoteSystemTime);

                stream.WriteSerializable(_acks);

                _acks.Clear();
            }

            stream.WriteBit(true);
            stream.WriteUInt((uint) (Environment.TickCount - _startTime));

            stream.WriteSerializable(packet);

            await _udp.SendAsync(stream.BaseBuffer, stream.BaseBuffer.Length, _endpoint).ConfigureAwait(false);
        }

        public void Send(byte[] data, PacketReliability reliability)
        {
            var orderingIndex =
                reliability == PacketReliability.UnreliableSequenced ?_sequencedWriteIndex++ :
                reliability == PacketReliability.ReliableOrdered ? _orderedWriteIndex++ : 0;

            if (GetHeaderLength(reliability, false) + data.Length >= MtuSize - UdpHeaderSize)
            {
                var chunks = new List<byte[]>();
                var splitPacketId = _splitPacketId++;
                var offset = 0;

                while (offset < data.Length)
                {
                    var length = MtuSize - UdpHeaderSize - GetHeaderLength(reliability, true);
                    var chunk = new byte[length];

                    Buffer.BlockCopy(data, offset, chunk, 0, length);

                    chunks.Add(chunk);

                    offset += length;
                }

                foreach (var chunk in chunks)
                {
                    _sends.Add(new InternalPacket
                    {
                        Data = chunk,
                        Reliability = reliability,
                        OrderingIndex = (uint) orderingIndex,
                        OrderingChannel = 0,
                        SplitPacket = true,
                        SplitPacketId = (ushort) splitPacketId,
                        SplitPacketIndex = (uint) chunks.IndexOf(chunk),
                        SplitPacketCount = (uint) chunks.Count
                    });
                }
            }
            else
                _sends.Add(new InternalPacket
                {
                    Data = data,
                    Reliability = reliability,
                    OrderingIndex = (uint) orderingIndex,
                    OrderingChannel = 0,
                    SplitPacket = false
                });
        }

        public static int GetHeaderLength(PacketReliability reliability, bool splitPacket)
        {
            var length = 52;

            if (reliability == PacketReliability.UnreliableSequenced ||
                reliability == PacketReliability.ReliableOrdered)
                length += 37;

            if (splitPacket)
                length += 80;

            return BitStream.BitsToBytes(length);
        }
    }
}