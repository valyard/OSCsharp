using System;

namespace OSCsharp.Net {
    /// <summary>
    /// Data for Tcp connection-related events.
    /// </summary>
    public class TcpConnectionEventArgs : EventArgs {
        /// <summary>
        /// Gets the associated connection.
        /// </summary>
        public TcpConnection Connection {
            get { return mConnection; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpConnectionEventArgs"/> class.
        /// </summary>
        /// <param name="connection">The associated connection.</param>
        public TcpConnectionEventArgs(TcpConnection connection) {
            mConnection = connection;
        }

        private TcpConnection mConnection;
    }
}