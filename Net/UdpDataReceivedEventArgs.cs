using System;
using System.Net;

namespace OSCsharp.Net {
    /// <summary>
    /// Data for Udp data received events.
    /// </summary>
    public class UdpDataReceivedEventArgs : EventArgs {
        /// <summary>
        /// Gets the associated source end point.
        /// </summary>
        public IPEndPoint SourceEndPoint {
            get { return mSourceEndPoint; }
        }

        /// <summary>
        /// Gets the associated data.
        /// </summary>
        public byte[] Data {
            get { return mData; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpDataReceivedEventArgs"/> class.
        /// </summary>
        /// <param name="sourceEndPoint">The associated source endpoiint.</param>
        /// <param name="data">The associated data.</param>
        public UdpDataReceivedEventArgs(IPEndPoint sourceEndPoint, byte[] data) {
            mSourceEndPoint = sourceEndPoint;
            mData = data;
        }

        private IPEndPoint mSourceEndPoint;
        private byte[] mData;
    }
}