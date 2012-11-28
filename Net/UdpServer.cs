using System;
using System.Net;
using System.Net.Sockets;
using OSCsharp.Utils;

namespace OSCsharp.Net {
    /// <summary>
    /// A Udp server.
    /// </summary>
    public class UdpServer : IPServer {
        /// <summary>
        /// Udp state class whose instances are passed between asynchronous BeginReceieve/EndReceive calls.
        /// </summary>
        private class UdpState {
            /// <summary>
            /// Gets the associated client.
            /// </summary>
            public UdpClient Client {
                get { return mClient; }
            }

            /// <summary>
            /// Gets the associted end point.
            /// </summary>
            public IPEndPoint IPEndPoint {
                get { return mIPEndPoint; }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="UdpState"/> class.
            /// </summary>
            /// <param name="client">The associated client.</param>
            /// <param name="ipEndPoint">The associated end point.</param>
            public UdpState(UdpClient client, IPEndPoint ipEndPoint) {
                mClient = client;
                mIPEndPoint = ipEndPoint;
            }

            private UdpClient mClient;
            private IPEndPoint mIPEndPoint;
        }

        #region Events

        /// <summary>
        /// Raised when data is received.
        /// </summary>
        public event EventHandler<UdpDataReceivedEventArgs> DataReceived;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the IP address the Udp server is bound to.
        /// </summary>
        public IPAddress IPAddress {
            get { return mIPAddress; }
        }

        /// <summary>
        /// Gets the port the Udp server is bound to.
        /// </summary>
        public int Port {
            get { return mPort; }
        }

        /// <summary>
        /// Gets the multicast address the server is joined to.
        /// </summary>
        public IPAddress MulticastAddress {
            get { return mMulticastAddress; }
        }

        /// <summary>
        /// Gets the state of the server.
        /// </summary>
        public bool IsRunning {
            get { return mAcceptingConnections; }
        }

        /// <summary>
        /// Gets the associated transmission type.
        /// </summary>
        public TransmissionType TransmissionType {
            get { return mTransmissionType; }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpServer"/> class.
        /// </summary>
        /// <param name="port">The port to bind to.</param>
        /// <remarks>Binds the server to the loopback address using TransmissionType.LocalBroadcast.</remarks>
        public UdpServer(int port)
            : this(IPAddress.Loopback, port, null, TransmissionType.LocalBroadcast) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpServer"/> class.
        /// </summary>
        /// <param name="port">The port to bind to.</param>
        /// <param name="multicastAddress">The multicast address to join.</param>
        /// <remarks>Binds the server to the loopback address and joins the specified multicast address.</remarks>
        public UdpServer(int port, IPAddress multicastAddress)
            : this(IPAddress.Loopback, port, multicastAddress, TransmissionType.Multicast) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpServer"/> class.
        /// </summary>
        /// <param name="ipAddress">The IP address to bind to.</param>
        /// <param name="port">The port to bind to.</param>
        /// <remarks>Binds the server to the specified IP address using TransmissionType.Unicast".</remarks>
        public UdpServer(IPAddress ipAddress, int port)
            : this(ipAddress, port, null, TransmissionType.Unicast) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpServer"/> class.
        /// </summary>
        /// <param name="ipAddress">The IP address to bind to.</param>
        /// <param name="port">The port to bind to.</param>
        /// <param name="multicastAddress">The multicast address to join.</param>
        /// <param name="transmissionType">The associated transmission type.</param>
        public UdpServer(IPAddress ipAddress, int port, IPAddress multicastAddress, TransmissionType transmissionType) {
            mPort = port;
            mIPAddress = ipAddress;
            mTransmissionType = transmissionType;

            if (mTransmissionType == TransmissionType.Multicast) {
                Assert.ParamIsNotNull(multicastAddress);
                mMulticastAddress = multicastAddress;
            }

            mAsynCallback = new AsyncCallback(EndReceive);
        }

        /// <summary>
        /// Start the Udp server and begin receiving data.
        /// </summary>
        public void Start() {
            IPEndPoint ipEndPoint;

            switch (mTransmissionType) {
                case TransmissionType.Unicast:
                    ipEndPoint = new IPEndPoint(mIPAddress, mPort);
                    mUdpClient = new UdpClient(ipEndPoint);
                    break;

                case TransmissionType.Multicast:
                    ipEndPoint = new IPEndPoint(IPAddress.Any, mPort);

                    Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
                    socket.Bind(ipEndPoint);

                    mUdpClient = new UdpClient();
                    mUdpClient.Client = socket;
                    mUdpClient.JoinMulticastGroup(mMulticastAddress);
                    break;

                case TransmissionType.Broadcast:
                case TransmissionType.LocalBroadcast:
                    ipEndPoint = new IPEndPoint(IPAddress.Any, mPort);
                    mUdpClient = new UdpClient(ipEndPoint);
                    break;

                default:
                    throw new Exception();
            }

            UdpState udpState = new UdpState(mUdpClient, ipEndPoint);

            mAcceptingConnections = true;
            mUdpClient.BeginReceive(mAsynCallback, udpState);
        }

        /// <summary>
        /// Stop the Udp server.
        /// </summary>
        public void Stop() {
            mAcceptingConnections = false;

            if (mUdpClient != null) {
                if (mTransmissionType == TransmissionType.Multicast) {
                    mUdpClient.DropMulticastGroup(mMulticastAddress);
                }

                mUdpClient.Close();
            }
        }

        #region Private Methods

        /// <summary>
        /// EndReceive paired call.
        /// </summary>
        /// <param name="asyncResult">Paired result object from the BeginReceive call.</param>
        private void EndReceive(IAsyncResult asyncResult) {
            try {
                UdpState udpState = (UdpState) asyncResult.AsyncState;
                UdpClient udpClient = udpState.Client;
                IPEndPoint ipEndPoint = udpState.IPEndPoint;

                byte[] data = udpClient.EndReceive(asyncResult, ref ipEndPoint);
                if (data != null && data.Length > 0) {
                    OnDataReceived(new UdpDataReceivedEventArgs(ipEndPoint, data));
                }

                if (mAcceptingConnections) {
                    udpClient.BeginReceive(mAsynCallback, udpState);
                }
            } catch (ObjectDisposedException) {
                // Suppress error
            }
        }

        /// <summary>
        /// Raise the DataReceived event.
        /// </summary>
        /// <param name="e">An EventArgs object that contains the event data.</param>
        private void OnDataReceived(UdpDataReceivedEventArgs e) {
            if (DataReceived != null) {
                DataReceived(this, e);
            }
        }

        #endregion

        private IPAddress mIPAddress;
        private int mPort;
        private IPAddress mMulticastAddress;
        private TransmissionType mTransmissionType;
        private UdpClient mUdpClient;
        private AsyncCallback mAsynCallback;

        private volatile bool mAcceptingConnections;
    }
}