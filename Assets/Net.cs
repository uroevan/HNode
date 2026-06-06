using System;
using System.Net;
using System.Net.Sockets;

namespace HNode.Net.ArtNet
{
    public class ArtNetSender : IDisposable
    {
        private readonly UdpClient _udp;
        private readonly IPEndPoint _endPoint;

        public string TargetIp { get; private set; }
        public int Port { get; private set; }

        public ArtNetSender(string targetIp = "255.255.255.255", int port = 6454)
        {
            TargetIp = targetIp;
            Port = port;

            _udp = new UdpClient();
            _udp.EnableBroadcast = true;

            _endPoint = new IPEndPoint(IPAddress.Parse(TargetIp), Port);
        }

        public void SendUniverse(byte[] dmxData, ushort universe)
        {
            if (dmxData == null) return;
            int length = Math.Min(dmxData.Length, 512);

            byte[] packet = new byte[18 + length];

            // ID "Art-Net\0"
            packet[0] = (byte)'A';
            packet[1] = (byte)'r';
            packet[2] = (byte)'t';
            packet[3] = (byte)'-';
            packet[4] = (byte)'N';
            packet[5] = (byte)'e';
            packet[6] = (byte)'t';
            packet[7] = 0x00;

            // OpCode = 0x5000 (ArtDMX)
            packet[8] = 0x00;
            packet[9] = 0x50;

            // Protocol version 14
            packet[10] = 0x00;
            packet[11] = 0x0E;

            // Sequence (0 = unused)
            packet[12] = 0x00;

            // Physical (0 for virtual)
            packet[13] = 0x00;

            // Universe (low, high)
            packet[14] = (byte)(universe & 0xFF);
            packet[15] = (byte)((universe >> 8) & 0xFF);

            // Length (high, low)
            packet[16] = (byte)((length >> 8) & 0xFF);
            packet[17] = (byte)(length & 0xFF);

            Buffer.BlockCopy(dmxData, 0, packet, 18, length);

            _udp.Send(packet, packet.Length, _endPoint);
        }

        public void Dispose()
        {
            _udp?.Close();
        }
    }
}
