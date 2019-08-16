using Microsoft.Win32;
using RemoteQueries.Exceptions;
using System;
using System.Collections.Generic;
using System.Net;

namespace RemoteQueries
{
    public static class RemoteRegistry
    {
        public static void GetInfo(IPAddress ip, IDictionary<string, object> userSet)
        {
            try
            {
                if (userSet.ContainsKey(ParameterSet.RrHostName))
                {
                    using (var reg = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, ip.ToString()))
                    using (var key = reg.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\ComputerName\ActiveComputerName"))
                    {
                        if (key != null)
                            userSet[ParameterSet.RrHostName] = (string)key.GetValue("ComputerName");
                    }
                }

                if (userSet.ContainsKey(ParameterSet.RrHostDesc))
                {
                    using (var reg = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, ip.ToString()))
                    using (var key = reg.OpenSubKey(@"System\CurrentControlSet\services\LanmanServer\Parameters"))
                    {
                        if (key != null)
                            userSet[ParameterSet.RrHostDesc] = (string)key.GetValue("srvcomment");
                    }
                }

                if (userSet.ContainsKey(ParameterSet.RrDomain))
                {
                    using (var reg = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, ip.ToString()))
                    using (var key = reg.OpenSubKey(@"System\CurrentControlSet\Services\Tcpip\Parameters"))
                    {
                        if (key != null)
                            userSet[ParameterSet.RrDomain] = (string)key.GetValue("NV Domain");
                    }
                }

                if (userSet.ContainsKey(ParameterSet.RrLastUser)) // Win7+
                {
                    using (var reg = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, ip.ToString(),
                        RegistryView.Registry64))
                    using (var key = reg.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Authentication\LogonUI"))
                    {
                        if (key != null)
                            userSet[ParameterSet.RrLastUser] = (string) key.GetValue("LastLoggedOnUser");
                    }
                }

                if (userSet.ContainsKey(ParameterSet.RrOsName))
                {
                    using (var reg = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, ip.ToString()))
                    using (var key = reg.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\"))
                    {
                        if (key != null)
                            userSet[ParameterSet.RrOsName] = (string) key.GetValue("ProductName");
                    }
                }

                if (userSet.ContainsKey(ParameterSet.RrOsArch))
                {
                    using (var reg = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, ip.ToString()))
                    using (var key = reg.OpenSubKey(@"System\CurrentControlSet\Control\Session Manager\Environment\"))
                    {
                        if (key != null)
                            userSet[ParameterSet.RrOsArch] = (string) key.GetValue("PROCESSOR_ARCHITECTURE") == "x86" ? "x86" : "x64";
                    }
                }

                if (userSet.ContainsKey(ParameterSet.RrOsSp))
                {
                    using (var reg = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, ip.ToString()))
                    using (var key = reg.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\"))
                    {
                        if (key != null)
                            userSet[ParameterSet.RrOsSp] = (string) key.GetValue("CSDVersion");
                    }
                }

                if (userSet.ContainsKey(ParameterSet.RrOsBuild))
                {
                    using (var reg = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, ip.ToString()))
                    using (var key = reg.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\"))
                    {
                        if (key != null)
                            userSet[ParameterSet.RrOsBuild] = (string)key.GetValue("CurrentBuildNumber");
                    }
                }

                if (userSet.ContainsKey(ParameterSet.RrOsInstDate))
                {
                    using (var reg = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, ip.ToString(),
                        RegistryView.Registry64))
                    using (var key = reg.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\"))
                    {
                        if (key != null)
                        {
                            var startDate = DateTime.SpecifyKind(new DateTime(1970, 1, 1, 0, 0, 0), DateTimeKind.Utc);
                            var objValue = key.GetValue("InstallDate");
                            var stringValue = objValue.ToString();
                            var regVal = Convert.ToInt64(stringValue);

                            var installDate = startDate.AddSeconds(regVal);

                            userSet[ParameterSet.RrOsInstDate] = installDate.ToLocalTime();
                        }
                    }
                }

                if (userSet.ContainsKey(ParameterSet.RrCpu))
                {
                    using (var reg = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, ip.ToString()))
                    using (var key = reg.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0\"))
                    {
                        if (key != null)
                            userSet[ParameterSet.RrCpu] = ((string)key.GetValue("ProcessorNameString")).Trim();
                    }
                }
            }
            catch (Exception e)
            {
                throw new RemoteRegistryException(e.Message, e);
            }
        }
    }
}
