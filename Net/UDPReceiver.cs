/*
 * @author Paul Varcholik / pvarchol@bespokesoftware.org
 * @author Valentin Simonov / http://va.lent.in/
 * @author Stefan Schlupek / http://monoflow.org
 */

using System;
using System.Net;
using System.Net.Sockets;
using OSCsharp.Data;
using OSCsharp.Utils;

namespace OSCsharp.Net
{
    public class UDPReceiver
    {
        private class UdpState
        {
            public UdpClient Client { get; private set; }

            public IPEndPoint IPEndPoint { get; private set; }

            public UdpState(UdpClient client, IPEndPoint ipEndPoint)
            {
                Client = client;
                IPEndPoint = ipEndPoint;
            }
        }

        public event EventHandler<OscPacketReceivedEventArgs> PacketReceived;
        public event EventHandler<OscBundleReceivedEventArgs> BundleReceived;
        public event EventHandler<ExceptionEventArgs> ErrorOccured;

        public event EventHandler<OscMessageReceivedEventArgs> MessageReceived
        {
            add { messageReceivedInvoker += value; }
            remove { messageReceivedInvoker -= value; }
        }

        //IOS version based on http://forum.unity3d.com/threads/113750-ExecutionEngineException-on-iOS-only
        //works only if you compile with VS2008 or VS2013!
        private EventHandler<OscMessageReceivedEventArgs> messageReceivedInvoker;

        public IPAddress IPAddress { get; private set; }
        public int Port { get; private set; }
        public IPAddress MulticastAddress { get; private set; }
        public IPEndPoint IPEndPoint { get; private set; }
        public TransmissionType TransmissionType { get; private set; }
        public bool ConsumeParsingExceptions { get; set; }

        public bool IsRunning
        {
            get { return acceptingConnections; }
        }

        private UdpClient udpClient;
        private volatile bool acceptingConnections;
        private AsyncCallback callback;

        public UDPReceiver(int port, bool consumeParsingExceptions) : this(IPAddress.Any, port, consumeParsingExceptions)
        {}

        public UDPReceiver(int port, IPAddress multicastAddress, bool consumeParsingExceptions) : this(IPAddress.Loopback, port, TransmissionType.Multicast, multicastAddress, consumeParsingExceptions)
        {}

        public UDPReceiver(string ipAddress, int port, bool consumeParsingExceptions) : this(IPAddress.Parse(ipAddress), port, consumeParsingExceptions)
        {}

        public UDPReceiver(IPAddress ipAddress, int port, bool consumeParsingExceptions) : this(ipAddress, port, TransmissionType.Unicast, null, consumeParsingExceptions)
        {}


        public UDPReceiver(IPAddress ipAddress, int port, TransmissionType transmissionType, IPAddress multicastAddress, bool consumeParsingExceptions)
        {
            IPAddress = ipAddress;
            Port = port;
            TransmissionType = transmissionType;

            if (TransmissionType == TransmissionType.Multicast)
            {
                if (multicastAddress == null) throw new ArgumentException("Multicast address is not set!");
                MulticastAddress = multicastAddress;
            }

            ConsumeParsingExceptions = consumeParsingExceptions;
            callback = new AsyncCallback(endReceive);
        }

        public void Start()
        {
            switch (TransmissionType)
            {
                case TransmissionType.Unicast:
                    IPEndPoint = new IPEndPoint(IPAddress, Port);
                    udpClient = new UdpClient(IPEndPoint);
                    break;

                case TransmissionType.Multicast:
                    IPEndPoint = new IPEndPoint(IPAddress.Any, Port);

                    Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
                    socket.Bind(IPEndPoint);

                    udpClient = new UdpClient();
                    udpClient.Client = socket;
                    udpClient.JoinMulticastGroup(MulticastAddress);
                    break;

                case TransmissionType.Broadcast:
                case TransmissionType.LocalBroadcast:
                    IPEndPoint = new IPEndPoint(IPAddress.Any, Port);
                    udpClient = new UdpClient(IPEndPoint);
                    break;

                default:
                    throw new Exception();
            }

            UdpState udpState = new UdpState(udpClient, IPEndPoint);

            acceptingConnections = true;
            udpClient.BeginReceive(callback, udpState);
        }

        public void Stop()
        {
            acceptingConnections = false;

            if (udpClient != null)
            {
                if (TransmissionType == TransmissionType.Multicast)
                {
                    udpClient.DropMulticastGroup(MulticastAddress);
                }

                udpClient.Close();
            }
        }

        private void endReceive(IAsyncResult asyncResult)
        {
            try
            {
                UdpState udpState = (UdpState)asyncResult.AsyncState;
                UdpClient udpClient = udpState.Client;
                IPEndPoint ipEndPoint = udpState.IPEndPoint;

                byte[] data = udpClient.EndReceive(asyncResult, ref ipEndPoint);
                if (data != null && data.Length > 0)
                {
                    parseData(ipEndPoint, data);
                }

                if (acceptingConnections)
                {
                    udpClient.BeginReceive(callback, udpState);
                }
            } catch (ObjectDisposedException)
            {
                // Suppress error
                var a = 2;
            }
        }

        private void parseData(IPEndPoint sourceEndPoint, byte[] data)
        {
            try
            {
                OscPacket packet = OscPacket.FromByteArray(data);
                onPacketReceived(packet);

                if (packet.IsBundle)
                {
                    onBundleReceived(packet as OscBundle);
                } else
                {
                    onMessageReceived(packet as OscMessage);
                }
            } catch (Exception ex)
            {
                if (!ConsumeParsingExceptions) onError(ex);
            }
        }

        private void onPacketReceived(OscPacket packet)
        {
            if (PacketReceived != null) PacketReceived(this, new OscPacketReceivedEventArgs(packet));
        }

        private void onBundleReceived(OscBundle bundle)
        {
            if (BundleReceived != null) BundleReceived(this, new OscBundleReceivedEventArgs(bundle));

            var count = bundle.Data.Count;
            for (var i = 0; i < count; i++)
            {
                object value = bundle.Data[i];
                if (value is OscBundle)
                {
                    // Raise events for nested bundles
                    onBundleReceived((OscBundle)value);
                } else if (value is OscMessage)
                {
                    // Raised events for contained messages
                    OscMessage message = (OscMessage)value;
                    onMessageReceived(message);
                }
            }
        }

        private void onMessageReceived(OscMessage message)
        {
            if (messageReceivedInvoker != null) messageReceivedInvoker(this, new OscMessageReceivedEventArgs(message));
        }

        private void onError(Exception ex)
        {
            if (ErrorOccured != null) ErrorOccured(this, new ExceptionEventArgs(ex));
        }
    }
}