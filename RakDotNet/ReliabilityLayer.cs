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
        private readonly UdpClient _udp;
        private readonly IPEndPoint _address;
        private readonly DateTimeOffset _startTime;
        private readonly List<long> _acks;
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
        private int _sendIndex;
        private int _sequencedWriteIndex;
        private int _sequencedReadIndex;
        private int _orderedWriteIndex;
        private int _orderedReadIndex;

        public DateTimeOffset StartTime => _startTime;
        public DateTimeOffset LastAckTime => _lastAckTime;
        public IPEndPoint Address => _address;
        public int Sent => _sent;

        public ReliabilityLayer(UdpClient udp, IPEndPoint address)
        {
            _udp = udp;
            _address = address;
            _startTime = DateTimeOffset.Now;
            _acks = new List<long>();
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
            
            while (true)
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

                    await _sendPacketAsync(resend.Packet);
                    
                    // TODO
                }
            }
        }

        public void StopSendLoop()
        {
            _active = false;
        }

        private async Task _sendPacketAsync(Packet packet)
        {
            
        }

        public void Send(byte[] data, PacketReliability reliability)
        {
            
        }
    }
}