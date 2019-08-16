using System.Collections.Generic;
using System.Net;

namespace RemoteQueries
{
    public static class Dns
    {
        public static void Resolve(IPAddress ip, IDictionary<string, object> userSet)
        {
            if (userSet.ContainsKey(ParameterSet.DnsName))
            {
                userSet[ParameterSet.DnsName] = System.Net.Dns.GetHostEntry(ip).HostName;
            }
        }
    }
}
