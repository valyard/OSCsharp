/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Net;
using System.Net.Sockets;
using OSCsharp.Data;

namespace OSCsharp.Net
{
    public class UDPTransmitter
    {
        public IPAddress IPAddress { get; private set; }
        public int Port { get; private set; }

        private UdpClient udpClient;

        public UDPTransmitter(string ipAddress, int port) : this(IPAddress.Parse(ipAddress), port)
        {}

        public UDPTransmitter(IPAddress ipAddress, int port)
        {
            IPAddress = ipAddress;
            Port = port;
        }

        public void Connect()
        {
            if (udpClient != null) Close();
            udpClient = new UdpClient();
            udpClient.Connect(IPAddress, Port);
        }

        public void Close()
        {
            udpClient.Close();
            udpClient = null;
        }

        public void Send(OscPacket packet)
        {
            byte[] data = packet.ToByteArray();
            try
            {
                udpClient.Send(data, data.Length);
            } catch
            {
                throw new Exception(string.Format("Error sending an OSC packet to {0}:{1}", IPAddress, Port));
            }
        }
    }
}