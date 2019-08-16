using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace RemoteQueries
{
    public static class RemoteArp
    {
        // http://www.codeproject.com/KB/IP/host_info_within_network.aspx
        [System.Runtime.InteropServices.DllImport("iphlpapi.dll", ExactSpelling = true)]
        static extern int SendARP(int DestIP, int SrcIP, byte[] pMacAddr, ref int PhyAddrLen);

        /// <summary>
        /// Gets the MAC address (<see cref="PhysicalAddress"/>) associated with the specified IP.
        /// </summary>
        /// <param name="ipAddress">The remote IP address.</param>
        /// <param name="userSet"></param>
        /// <returns>The remote machine's MAC address.</returns>
        public static void GetInfo(IPAddress ipAddress, IDictionary<string, object> userSet)
        {
            if (!userSet.ContainsKey(ParameterSet.ArpMac))
                return;

            const int macAddressLength = 6;
            int length = macAddressLength;
            var macBytes = new byte[macAddressLength];
            SendARP(BitConverter.ToInt32(ipAddress.GetAddressBytes(), 0), 0, macBytes, ref length);

            if (!macBytes.All(@byte => @byte == 0))
                userSet[ParameterSet.ArpMac] = new PhysicalAddress(macBytes);
        }
    }
}
