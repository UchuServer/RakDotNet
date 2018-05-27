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

        public ReliabilityLayer(UdpClient sock, IPEndPoint address)
        {
            this.sock = sock;
            this.address = address;

            startTime = Process.GetCurrentProcess().TotalProcessorTime;
        }

        public IEnumerable<byte[]> HandlePacket(byte[] data)
        {
            var stream = new BitStream(data);

            if (ParsePacketHeader(stream))
                return null;

            return ParsePackets(stream);
        }

        private bool ParsePacketHeader(BitStream stream)
        {
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


            }

            return false;
        }

        private IEnumerable<byte[]> ParsePackets(BitStream stream)
        {
            yield break;
        }
    }
}
