using System;
using System.Net;

namespace OSCsharp.Net {
    /// <summary>
    /// Helper methods for IP servers.
    /// </summary>
    public abstract class IPServer {
        /// <summary>
        /// Get the local IP addresses bound to this computer.
        /// </summary>
        /// <returns>The list of IP addresses bound to this computer.</returns>
        /// <exception cref="Exception">Thrown if no local IP addresses are found.</exception>
        public static IPAddress[] GetLocalIPAddress() {
            IPAddress[] localAddresses = Dns.GetHostAddresses(Dns.GetHostName());
            if (localAddresses.Length == 0) {
                throw new Exception("No local IP Address address found.");
            }

            return localAddresses;
        }

        /// <summary>
        /// Determine if the specified end point is available.
        /// </summary>
        /// <param name="ipAddress">The IP address to check.</param>
        /// <param name="port">The port to check.</param>
        /// <returns>true if the specified end point is available; otherwise, false.</returns>
        public static bool IsIPEndPointAvailable(IPAddress ipAddress, int port) {
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, port);
            return IsIPEndPointAvailable(ipEndPoint);
        }

        /// <summary>
        /// Determine if the specified end point is available.
        /// </summary>
        /// <param name="ipEndPoint">The IP end point to check.</param>
        /// <returns>true if the specified end point is available; otherwise, false.</returns>
        public static bool IsIPEndPointAvailable(IPEndPoint ipEndPoint) {
            bool isIPEndPointAvailable = true;

            System.Net.NetworkInformation.IPGlobalProperties ipGlobalProperties = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] activeUdpListeners = ipGlobalProperties.GetActiveUdpListeners();
            foreach (IPEndPoint activeUdpListener in activeUdpListeners) {
                if ((activeUdpListener.Address == ipEndPoint.Address) && (activeUdpListener.Port == ipEndPoint.Port)) {
                    isIPEndPointAvailable = false;
                    break;
                }
            }

            return isIPEndPointAvailable;
        }
    }
}