using System.Net;

namespace AdmAssist.Models
{
    public class IpAddressRange
    {
        public IPAddress Begin { get; set; }
        public IPAddress End { get; set; }

        public IpAddressRange(IPAddress begin, IPAddress end)
        {
            Begin = begin;
            End = end;
        }
    }
}
