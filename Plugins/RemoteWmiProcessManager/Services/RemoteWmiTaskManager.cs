using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Windows;

namespace RemoteWmiProcessManager.Services
{
    public class RemoteWmiTaskManager
    {
        private readonly ManagementScope _managementScope;

        public RemoteWmiTaskManager(IPAddress ip)
        {
            var path = new ManagementPath($"\\\\{ip}\\root\\cimv2");
            _managementScope = new ManagementScope(path,
                new ConnectionOptions { Timeout = new TimeSpan(0, 0, 0, 0, 5000) });
        }

        public Dictionary<uint, Tuple<ManagementObject, string>> GetProcesses()
        {
            var res = new Dictionary<uint, Tuple<ManagementObject, string>>();

            var query = new SelectQuery("SELECT * FROM Win32_Process");

            using (var searcher =
                new ManagementObjectSearcher(_managementScope, query,
                    new EnumerationOptions {ReturnImmediately = false}))
            {
                foreach (var o in searcher.Get())
                {
                    var queryObj = (ManagementObject)o;

                    var owner = string.Empty;

                    if (queryObj["ExecutablePath"] != null)
                    {
                        ManagementBaseObject ownerObj = queryObj.InvokeMethod("GetOwner", null, null);

                        if (ownerObj != null) owner = ownerObj["User"].ToString();
                    }

                    res.Add((uint)queryObj["ProcessId"], new Tuple<ManagementObject, string>(queryObj, owner));
                }
            }

            return res;
        }

        public void KillProcess(string processName)
        {
            var msQuery = new SelectQuery("SELECT * FROM Win32_Process Where Name = '" + processName + "'");

            using (var searchProcedure = new ManagementObjectSearcher(_managementScope, msQuery,
                new EnumerationOptions {ReturnImmediately = false}))
            {
                foreach (var item in searchProcedure.Get().Cast<ManagementObject>())
                {
                    try
                    {
                        item.InvokeMethod("Terminate", null);
                    }
                    catch (SystemException e)
                    {
                        MessageBox.Show("An Error Occurred: " + e.Message);
                    }
                }
            }
        }
    }
}
