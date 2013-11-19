using System;
using OSCsharp.Data;

namespace OSCsharp
{
    public class OscPacketReceivedEventArgs : EventArgs
    {
        public OscPacket Packet { get; private set; }

        public OscPacketReceivedEventArgs(OscPacket packet)
        {
            Packet = packet;
        }
    }
}