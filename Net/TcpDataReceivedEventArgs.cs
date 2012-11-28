using System;

namespace OSCsharp.Net {
    /// <summary>
    /// Data for Tcp data received events.
    /// </summary>
    public class TcpDataReceivedEventArgs : EventArgs {
        /// <summary>
        /// Gets the associated connection.
        /// </summary>
        public TcpConnection Connection {
            get { return mConnection; }
        }

        /// <summary>
        /// Gets the associated data.
        /// </summary>
        public byte[] Data {
            get { return mData; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpConnectionEventArgs"/> class.
        /// </summary>
        /// <param name="connection">The associated connection.</param>
        /// <param name="data">The associated data.</param>
        public TcpDataReceivedEventArgs(TcpConnection connection, byte[] data) {
            mConnection = connection;
            mData = data;
        }

        private TcpConnection mConnection;
        private byte[] mData;
    }
}