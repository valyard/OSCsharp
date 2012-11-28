using System;
using OSCsharp.Utils;

namespace OSCsharp {
    /// <summary>
    /// Arguments for packet received events.
    /// </summary>
    public class OscPacketReceivedEventArgs : EventArgs {
        /// <summary>
        /// Gets the <see cref="OscPacket"/> received.
        /// </summary>
        public OscPacket Packet {
            get { return mPacket; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OscMessageReceivedEventArgs"/> class.
        /// </summary>
        /// <param name="packet">The <see cref="OscPacket"/> received.</param>
        public OscPacketReceivedEventArgs(OscPacket packet) {
            Assert.ParamIsNotNull(packet);

            mPacket = packet;
        }

        private OscPacket mPacket;
    }
}