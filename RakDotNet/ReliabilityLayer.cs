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
        public const int MtuSize = 1228;
        public const int UdpHeaderSize = 28;

        private readonly UdpClient _udp;
        private readonly IPEndPoint _endpoint;
        private readonly DateTimeOffset _startTime;
        private readonly RangeList<uint> _acks;
        private readonly List<uint> _lastReliabilityReceived;
        private readonly Dictionary<uint, byte[]> _outOfOrderPackets;
        private readonly Dictionary<uint, byte[][]> _splitPacketQueue;
        private readonly List<Packet> _sends;
        private readonly Dictionary<uint, Resend> _resends;

        private bool _active;
        private DateTimeOffset _lastAckTime;
        private uint _splitPacketId;
        private DateTimeOffset _remoteSystemTime;
        private long _retransmissionTimeout;
        private long _smoothedRoundTripTime;
        private long _roundTripTimeVariation;
        private long _congestionWindow;
        private long _slowStartThreshold;
        private int _sent;
        private uint _sendIndex;
        private int _sequencedWriteIndex;
        private int _sequencedReadIndex;
        private int _orderedWriteIndex;
        private int _orderedReadIndex;

        public DateTimeOffset StartTime => _startTime;
        public DateTimeOffset LastAckTime => _lastAckTime;
        public IPEndPoint Endpoint => _endpoint;
        public int Sent => _sent;

        public ReliabilityLayer(UdpClient udp, IPEndPoint endpoint)
        {
            _udp = udp;
            _endpoint = endpoint;
            _startTime = DateTimeOffset.Now;
            _acks = new UIntRangeList();
            _lastReliabilityReceived = new List<uint>();
            _outOfOrderPackets = new Dictionary<uint, byte[]>();
            _splitPacketQueue = new Dictionary<uint, byte[][]>();
            _sends = new List<Packet>();
            _resends = new Dictionary<uint, Resend>();
            _active = false;
        }

        public IEnumerable<byte[]> HandleDatagram(byte[] buffer)
        {
            if (buffer.Length <= 2)
                yield break;

            var stream = new BitStream(buffer);

            if (stream.ReadBit()) // has acks
            {
                var ourSystemTime = (long) stream.ReadULong();
                var roundTripTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() - _startTime.ToUnixTimeMilliseconds() -
                                    ourSystemTime;

                if (_smoothedRoundTripTime == -1)
                {
                    _smoothedRoundTripTime = roundTripTime;
                    _roundTripTimeVariation = roundTripTime / 2;
                }
                else
                {
                    var alpha = 0.125f;
                    var beta = 0.25f;

                    _roundTripTimeVariation = (long) ((1 - beta) * _roundTripTimeVariation +
                                                      beta * Math.Abs(_smoothedRoundTripTime - roundTripTime));
                    _smoothedRoundTripTime = (long) ((1 - alpha) * _smoothedRoundTripTime + alpha * roundTripTime);
                }

                _retransmissionTimeout = Math.Max(1, _smoothedRoundTripTime + 4 * _roundTripTimeVariation);

                var acks = new UIntRangeList();

                stream.ReadSerializable(acks);

                uint? lastMax = null;
                var holes = 0;

                foreach (var ack in acks)
                {
                    for (var i = ack.Min; i >= ack.Min && i <= ack.Max; i++)
                    {
                        if (_resends.ContainsKey(i))
                            _resends.Remove(i);
                    }

                    if (lastMax != null)
                    {
                        for (var i = (uint) lastMax + 1; i >= (uint) lastMax + 1 && i <= ack.Min; i++)
                        {
                            if (_resends.ContainsKey(i))
                                holes++;
                        }
                    }

                    lastMax = ack.Max;
                }

                if (holes > 0)
                {
                    _slowStartThreshold = _congestionWindow / 2;
                    _congestionWindow = _slowStartThreshold;
                }
                else
                {
                    if (_sent >= _congestionWindow)
                    {
                        _congestionWindow += acks.Count > _slowStartThreshold
                            ? acks.Count / _congestionWindow
                            : acks.Count;
                    }
                }

                _sent = 0;
                _lastAckTime = DateTimeOffset.Now;
            }

            if (stream.AllRead)
                yield break;

            if (stream.ReadBit())
                _remoteSystemTime = DateTimeOffset.FromUnixTimeMilliseconds((long) stream.ReadULong());

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
                        _lastReliabilityReceived.RemoveAt(0);
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

        public async Task StartSendLoopAsync()
        {
            _active = true;

            while (_active)
            {
                await Task.Delay(30);

                foreach (var entry in _resends)
                {
                    var messageNum = entry.Key;
                    var resend = entry.Value;

                    if (resend.Time > DateTimeOffset.Now)
                        continue;

                    if (_sent >= _congestionWindow)
                        break;

                    _sent++;

                    resend.Packet.MessageNumber = messageNum;

                    await _sendPacketAsync(resend.Packet).ConfigureAwait(false);

                    if (resend.Packet.Reliability == PacketReliability.Reliable ||
                        resend.Packet.Reliability == PacketReliability.ReliableOrdered)
                        _resends[messageNum] = new Resend
                        {
                            Time = DateTimeOffset.Now.AddMilliseconds(_retransmissionTimeout),
                            Packet = resend.Packet
                        };
                }

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
                            Time = DateTimeOffset.Now.AddMilliseconds(_retransmissionTimeout),
                            Packet = packet
                        };
                }

                if (_acks.Count > 0)
                {
                    var ack = new BitStream();

                    ack.WriteBit(true);
                    ack.WriteUInt((uint) _remoteSystemTime.ToUnixTimeMilliseconds());
                    ack.WriteSerializable(_acks);

                    _acks.Clear();

                    await _udp.SendAsync(ack.BaseBuffer, ack.BaseBuffer.Length, _endpoint).ConfigureAwait(false);
                }
            }
        }

        public void StopSendLoop()
        {
            _active = false;
        }

        private async Task _sendPacketAsync(Packet packet)
        {
            var stream = new BitStream();

            var hasAcks = _acks.Count > 0;

            stream.WriteBit(hasAcks);

            if (hasAcks)
            {
                stream.WriteUInt((uint) _remoteSystemTime.ToUnixTimeMilliseconds());
                stream.WriteSerializable(_acks);

                _acks.Clear();
            }

            stream.WriteBit(true);

            var time = DateTimeOffset.Now.ToUnixTimeMilliseconds() - _startTime.ToUnixTimeMilliseconds();
            stream.WriteUInt((uint) time);
            stream.WriteUInt(packet.MessageNumber);
            stream.WriteBits(new[] {(byte) packet.Reliability}, 3);

            if (packet.Reliability == PacketReliability.UnreliableSequenced ||
                packet.Reliability == PacketReliability.ReliableOrdered)
            {
                stream.WriteBits(new[] {packet.OrderingChannel}, 5);
                stream.WriteUInt(packet.OrderingIndex);
            }

            stream.WriteBit(packet.SplitPacket);

            if (packet.SplitPacket)
            {
                stream.WriteUShort(packet.SplitPacketId);
                stream.WriteUIntCompressed(packet.SplitPacketIndex);
                stream.WriteUIntCompressed(packet.SplitPacketCount);
            }

            stream.WriteUShortCompressed((ushort) BitStream.BytesToBits(packet.Data.Length));
            stream.AlignWrite();
            stream.Write(packet.Data);

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
                    _sends.Add(new Packet
                    {
                        Data = chunk,
                        Reliability = reliability,
                        OrderingIndex = (uint) orderingIndex,
                        SplitPacket = true,
                        SplitPacketId = (ushort) splitPacketId,
                        SplitPacketIndex = (uint) chunks.IndexOf(chunk),
                        SplitPacketCount = (uint) chunks.Count
                    });
                }
            }
            else
                _sends.Add(new Packet
                {
                    Data = data,
                    Reliability = reliability,
                    OrderingIndex = (uint) orderingIndex,
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