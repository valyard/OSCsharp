using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace OSCsharp.Net {
    /// <summary>
    /// A multi-threaded Tcp server.
    /// </summary>
    /// <remarks>Data can be received automatically, by the connections established by the <see cref="TcpServer"/> 
    /// by setting <see cref="ReceiveDataInline"/> to true during instantiation (the default behavior). This establishes
    /// asynchronous reception of Tcp messages framed using a 4-byte integer containing the length of the message. The
    /// <see cref="DataReceived"/> event is raised as complete messages are received. Users can perform their own
    /// message handling by setting <see cref="ReceiveDataInline"/> to false and listening for <see cref="Connected" /> events.</remarks>
    public class TcpServer : IPServer, IDisposable {
        /// <summary>
        /// The maximum number of connections that can be pending.
        /// </summary>
        public static readonly int MaxPendingConnections = 3;

        #region Events

        /// <summary>
        /// Raised when a connection is established.
        /// </summary>
        public event EventHandler<TcpConnectionEventArgs> Connected;

        /// <summary>
        /// Raised when a connection is disconnected.
        /// </summary>
        public event EventHandler<TcpConnectionEventArgs> Disconnected;

        /// <summary>
        /// Raised when data is received.
        /// </summary>
        public event EventHandler<TcpDataReceivedEventArgs> DataReceived;

        #endregion

        /// <summary>
        /// Gets the IP address the Tcp server is bound to.
        /// </summary>
        public IPAddress IPAddress {
            get { return mIPAddress; }
        }

        /// <summary>
        /// Gets the port the Tcp server is bound to.
        /// </summary>
        public int Port {
            get { return mPort; }
        }

        /// <summary>
        /// Gets the state of the server.
        /// </summary>
        public bool IsRunning {
            get { return mAcceptingConnections; }
        }

        /// <summary>
        /// Gets the number of active connections.
        /// </summary>
        public int ActiveConnectionCount {
            get { return mClientConnections.Count; }
        }

        /// <summary>
        /// Gets the list of active connections.
        /// </summary>
        public ReadOnlyCollection<TcpConnection> ActiveConnections {
            get { return mClientConnections.AsReadOnly(); }
        }

        /// <summary>
        /// Gets the data reception mode applied to the Tcp server.
        /// </summary>
        public bool ReceiveDataInline {
            get { return mReceiveDataInline; }
        }

        /// <summary>
        /// Gets or sets the expected endianness of integral value types.
        /// </summary>
        public bool LittleEndianByteOrder {
            get { return mLittleEndianByteOrder; }
            set { mLittleEndianByteOrder = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpServer"/> class.
        /// </summary>
        /// <param name="port">The port to bind to.</param>
        /// <remarks>Uses the loopback address, inline data reception and little endian byte order.</remarks>
        public TcpServer(int port)
            : this(IPAddress.Loopback, port, true, true) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpServer"/> class.
        /// </summary>
        /// <param name="ipAddress">The IP address to bind to.</param>
        /// <param name="port">The port to bind to.</param>
        /// <param name="receiveDataInline">The mode of automatic data reception.</param>
        /// <param name="littleEndianByteOrder">The expected endianness of integral value types.</param>
        public TcpServer(IPAddress ipAddress, int port, bool receiveDataInline = true, bool littleEndianByteOrder = true) {
            mPort = port;
            mIPAddress = ipAddress;
            mReceiveDataInline = receiveDataInline;
            mClientConnections = new List<TcpConnection>();
            mConnectionsToClose = new List<TcpConnection>();
            mIsShuttingDown = false;
            mListenerReady = new ManualResetEvent(false);
            mLittleEndianByteOrder = littleEndianByteOrder;
        }

        /// <summary>
        /// Release the resources associated with this object.
        /// </summary>
        public void Dispose() {
            lock (this) {
                if (mIsShuttingDown == false) {
                    Stop();
                }

                foreach (TcpConnection connection in mClientConnections) {
                    connection.Dispose();
                }

                mClientConnections.Clear();
                mClientConnections = null;

                mConnectionsToClose.Clear();
                mConnectionsToClose = null;

                if (mListenerReady != null) {
                    mListenerReady.Close();
                    mListenerReady = null;
                }
            }
        }

        /// <summary>
        /// Start the Tcp server.
        /// </summary>
        /// <remarks>This is a blocking call and should be considered for use within a separate thread.</remarks>
        public void Start() {
            TcpListener listener = null;

            try {
                mIsShuttingDown = false;
                mAcceptingConnections = true;

                listener = new TcpListener(mIPAddress, mPort);
                listener.Start(MaxPendingConnections);

                while (mAcceptingConnections) {
                    mListenerReady.Reset();
                    listener.BeginAcceptSocket(EndAcceptSocket, listener);
                    mListenerReady.WaitOne();
                }
            } finally {
                if (listener != null) {
                    listener.Stop();
                }

                lock (mClientConnections) {
                    foreach (TcpConnection connection in mClientConnections) {
                        MarkConnectionForClose(connection);
                    }

                    CloseMarkedConnections();
                }

                mIsShuttingDown = true;
            }
        }

        /// <summary>
        /// Stop the Tcp server.
        /// </summary>
        public void Stop() {
            mAcceptingConnections = false;
            mIsShuttingDown = true;
            mListenerReady.Set();
        }

        /// <summary>
        /// Close an active connection.
        /// </summary>
        /// <param name="connection">The connection to close.</param>
        public void CloseConnection(TcpConnection connection) {
            try {
                connection.Dispose();
            } catch {
                // Igore any exceptions
            } finally {
                mClientConnections.Remove(connection);
            }
        }

        #region Private Methods

        /// <summary>
        /// Asynchronous callback paired with TcpListener.BeginAcceptSocket()
        /// </summary>
        /// <param name="asyncResult"></param>
        private void EndAcceptSocket(IAsyncResult asyncResult) {
            try {
                TcpListener listener = (TcpListener) asyncResult.AsyncState;
                Socket socket = listener.EndAcceptSocket(asyncResult);

                TcpConnection connection = new TcpConnection(socket, mLittleEndianByteOrder);
                connection.Disconnected += new EventHandler<TcpConnectionEventArgs>(OnDisconnected);
                connection.DataReceived += new EventHandler<TcpDataReceivedEventArgs>(OnDataReceived);

                if (mReceiveDataInline) {
                    connection.ReceiveDataAsynchronously();
                }

                mClientConnections.Add(connection);
                OnConnected(new TcpConnectionEventArgs(connection));
            } catch (ObjectDisposedException) {
                // Consume exception
            } finally {
                mListenerReady.Set();
            }
        }

        /// <summary>
        /// Raise the Connected event.
        /// </summary>
        /// <param name="e">An <see cref="TcpConnectionEventArgs"/> object that contains the event data.</param>
        private void OnConnected(TcpConnectionEventArgs e) {
            if (Connected != null) {
                Connected(this, e);
            }
        }

        /// <summary>
        /// Raise the Disconnected event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">An <see cref="TcpConnectionEventArgs"/> object that contains the event data.</param>
        private void OnDisconnected(object sender, TcpConnectionEventArgs e) {
            if (Disconnected != null) {
                Disconnected(this, e);
            }
        }

        /// <summary>
        /// Raise the DataReceived event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">An <see cref="TcpDataReceivedEventArgs"/> object that contains the event data.</param>
        private void OnDataReceived(object sender, TcpDataReceivedEventArgs e) {
            if (DataReceived != null) {
                DataReceived(this, e);
            }
        }

        /// <summary>
        /// Mark a connection to be closed.
        /// </summary>
        /// <param name="connection">The connection to mark.</param>
        private void MarkConnectionForClose(TcpConnection connection) {
            mConnectionsToClose.Add(connection);
        }

        /// <summary>
        /// Close marked connections.
        /// </summary>
        private void CloseMarkedConnections() {
            foreach (TcpConnection connection in mConnectionsToClose) {
                CloseConnection(connection);
            }

            mConnectionsToClose.Clear();
        }

        #endregion

        private IPAddress mIPAddress;
        private int mPort;
        private List<TcpConnection> mClientConnections;
        private List<TcpConnection> mConnectionsToClose;
        private bool mReceiveDataInline;
        private volatile bool mIsShuttingDown;
        private volatile bool mAcceptingConnections;
        private ManualResetEvent mListenerReady;
        private bool mLittleEndianByteOrder;
    }
}