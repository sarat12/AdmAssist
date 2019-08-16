using System;
using System.Net;
using AdmAssist.Helpers;
using AdmAssist.Models;
using RemoteQueries;
using Dns = RemoteQueries.Dns;

namespace AdmAssist.Services
{
    public static class NodesServer
    {
        public static void ScanNode(ScanTask task)
        {
            task.Node[Constants.StatusColumnName] = null;

            if (!task.Node.ContainsKey(Constants.IpColumnName) || !(task.Node[Constants.IpColumnName] is IPAddress))
                throw new ArgumentException("Node doesn't contain ip adress!");

            var ip = (IPAddress) task.Node[Constants.IpColumnName];

            bool isOnline = NetTools.IsOnline(ip);

            if (isOnline || ConfigurationManager.Configuration.ScanningOptions.ScanOfflineHost)
            {
                try
                {
                    Dns.Resolve(ip, task.Node);
                }
                catch
                {
                    // do nothing by desing
                }
                try
                {
                    RemoteArp.GetInfo(ip, task.Node);
                }
                catch
                {
                    // do nothing by desing
                }
                try
                {
                    RemoteNetBios.GetInfo(ip, 137, task.Node);
                }
                catch
                {
                    // do nothing by desing
                }
                try
                {
                    RemoteRegistry.GetInfo(ip, task.Node);
                }
                catch
                {
                    // do nothing by desing
                }
                try
                {
                    RemoteSnmp.GetAllInfo(ip, task.Node);
                }
                catch
                {
                    // do nothing by desing
                }
                try
                {
                    RemoteWmi.GetInfo(ip, task.Node);
                }
                catch
                {
                    // do nothing by desing
                }
            }
            task.Node[Constants.StatusColumnName] = isOnline;
        }
    }
}
