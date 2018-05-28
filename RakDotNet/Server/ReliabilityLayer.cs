using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace RakDotNet
{
    public class ReliabilityLayer
    {
        private readonly UdpClient sock;
        private readonly IPEndPoint address;
        private readonly TimeSpan startTime;
        private TimeSpan srtt;
        private TimeSpan rttVar;
        private TimeSpan rto;
        private TimeSpan remoteSysTime;

        public ReliabilityLayer(UdpClient sock, IPEndPoint address)
        {
            this.sock = sock;
            this.address = address;

            startTime = Process.GetCurrentProcess().TotalProcessorTime;
        }

        public IEnumerable<byte[]> HandlePacket(byte[] data)
        {
            var stream = new BitStream(data);

            var hasAcks = stream.ReadBit();

            if (hasAcks)
            {
                var oldTime = stream.ReadUInt32();
                var rtt = Process.GetCurrentProcess().TotalProcessorTime.Subtract(startTime).Subtract(new TimeSpan(oldTime));

                if (srtt == null)
                {
                    srtt = rtt;
                    rttVar = rtt.Divide(2);
                }
                else
                {
                    var alpha = 0.125;
                    var beta = 0.25;

                    rttVar = rttVar.Multiply(1 - beta).Add(new TimeSpan((long)(beta * Math.Abs(srtt.Milliseconds - rtt.Milliseconds))));
                    srtt = srtt.Multiply(1 - alpha).Add(rtt.Multiply(alpha));
                }

                rto = new TimeSpan(Math.Max(1, srtt.Milliseconds + 4 * rttVar.Milliseconds));

                var acks = stream.ReadSerializable<RangeList>();

                // TODO: implement resends
            }

            if (stream.UnreadBitCount <= 0)
                yield break;

            if (stream.ReadBit())
                remoteSysTime = new TimeSpan(stream.ReadUInt32());

            foreach (var p in ParsePackets(stream))
                yield return p;
        }

        private IEnumerable<byte[]> ParsePackets(BitStream stream)
        {
            while (stream.UnreadBitCount > 0)
            {
                var msgNum = stream.ReadUInt32();
                var reliability = stream.ReadBits(3);
            }

            yield break;
        }
    }
}
