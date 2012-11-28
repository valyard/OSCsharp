using System.Net;
using System.Net.Sockets;
using OSCsharp.Net;

namespace OSCsharp {
    /// <summary>
    /// Represents a TCP/IP client-side connection.
    /// </summary>
    public class OscClient {
        /// <summary>
        /// Gets the IP address of the server-side of the connection.
        /// </summary>
        public IPAddress ServerIPAddress {
            get { return mServerIPAddress; }
        }

        /// <summary>
        /// Gets the port of the server-side of the connection.
        /// </summary>
        public int ServerPort {
            get { return mServerPort; }
        }

        /// <summary>
        /// Gets the underlying <see cref="TcpClient"/>.
        /// </summary>
        public TcpClient Client {
            get { return mClient; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OscClient"/> class.
        /// </summary>
        public OscClient() {
            mClient = new TcpClient();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OscClient"/> class.
        /// </summary>
        /// <param name="serverEndPoint">The server-side endpoint of the connection.</param>
        public OscClient(IPEndPoint serverEndPoint)
            : this(serverEndPoint.Address, serverEndPoint.Port) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="OscClient"/> class.
        /// </summary>
        /// <param name="serverIPAddress">The server-side IP address of the connection.</param>
        /// <param name="serverPort">The server-side port of the connection.</param>
        public OscClient(IPAddress serverIPAddress, int serverPort)
            : this() {
            mServerIPAddress = serverIPAddress;
            mServerPort = serverPort;
        }

        /// <summary>
        /// Connect to the previously specified server-side endpoint.
        /// </summary>
        public void Connect() {
            Connect(mServerIPAddress, mServerPort);
        }

        /// <summary>
        /// Connect to the previously specified server-side endpoint.
        /// </summary>
        /// <param name="serverEndPoint">The server-side endpoint to connect to.</param>
        public void Connect(IPEndPoint serverEndPoint) {
            Connect(serverEndPoint.Address, serverEndPoint.Port);
        }

        /// <summary>
        /// Connect to a server.
        /// </summary>
        /// <param name="serverIPAddress">The server-side IP address to connect to.</param>
        /// <param name="serverPort">The server-side port to connect to.</param>
        public void Connect(IPAddress serverIPAddress, int serverPort) {
            mServerIPAddress = serverIPAddress;
            mServerPort = serverPort;

            mClient.Connect(mServerIPAddress, mServerPort);
            mTcpConnection = new TcpConnection(mClient.Client, OscPacket.LittleEndianByteOrder);
        }

        /// <summary>
        /// Close the connection.
        /// </summary>
        public void Close() {
            mTcpConnection.Dispose();
            mTcpConnection = null;
            mClient.Close();
        }

        /// <summary>
        /// Send an OscPacket over the connection.
        /// </summary>
        /// <param name="packet">The <see cref="OscPacket"/> to send.</param>
        public void Send(OscPacket packet) {
            byte[] packetData = packet.ToByteArray();
            mTcpConnection.Writer.Write(OscPacket.ValueToByteArray(packetData));
        }

        private IPAddress mServerIPAddress;
        private int mServerPort;
        private TcpClient mClient;
        private TcpConnection mTcpConnection;
    }
}