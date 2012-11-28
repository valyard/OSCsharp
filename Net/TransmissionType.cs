namespace OSCsharp.Net {
    /// <summary>
    /// Types of transmission.
    /// </summary>
    public enum TransmissionType {
        /// <summary>
        /// Transmit to subscribed clients only. Includes heartbeat exchanges.
        /// </summary>
        Unicast,

        /// <summary>
        /// Transmit via Udp multicast. No heartbeat exchanges.
        /// </summary>
        Multicast,

        /// <summary>
        /// Transmit via Udp broadcast. No heartbeat exchanges.
        /// </summary>
        Broadcast,

        /// <summary>
        /// Local unicast without subcription or heartbeat exchanges.
        /// </summary>
        LocalBroadcast
    }
}