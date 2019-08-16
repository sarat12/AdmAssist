using System;
using System.Net;
using System.Net.NetworkInformation;
using System.ServiceProcess;

namespace AdmAssist.Helpers
{
    public static class NetTools
    {
        public static bool TryStartService(string serviceName, IPAddress ip)
        {
            try
            {
                var sc = new ServiceController(serviceName, ip.ToString());
                sc.Start();
                sc.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 0, 10));
                sc.Refresh();
                return sc.Status == ServiceControllerStatus.Running;
            }
            catch { return false; }
        }

        public static bool IsOnline(IPAddress ipAddress)
        {
            using (var p = new Ping())
            {
                try
                {
                    var pingReply = p.Send(ipAddress, 1000);
                    return pingReply != null && pingReply.Status == IPStatus.Success;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
