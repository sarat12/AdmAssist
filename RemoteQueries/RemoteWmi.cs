using RemoteQueries.Exceptions;
using System;
using System.Collections.Generic;
using System.Management;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;

namespace RemoteQueries
{
    public static class RemoteWmi
    {
        public static void GetInfo(IPAddress ip, string userName, SecureString password, IDictionary<string, object> userSet)
        {
            var opt = new ConnectionOptions
            {
                Username = userName,
                SecurePassword = password,
                Impersonation = ImpersonationLevel.Impersonate, //recomended by Microsoft
                Timeout = TimeSpan.FromSeconds(2)
            };

            var scope = InitManagementScope(ip, opt);

            GetAllInfo(scope, userSet);
        }
        public static void GetInfo(IPAddress ip, IDictionary<string, object> userSet)
        {
            var opt = new ConnectionOptions
            {
                Impersonation = ImpersonationLevel.Impersonate, //recomended by Microsoft
                Timeout = TimeSpan.FromSeconds(2)
            };

            var scope = InitManagementScope(ip, opt);

            GetAllInfo(scope, userSet);
        }

        private static ManagementScope InitManagementScope(IPAddress ip, ConnectionOptions opt)
        {
            var path = new ManagementPath($@"\\{ip}\root\cimv2");

            return new ManagementScope(path, opt);
        }

        private static void GetAllInfo(ManagementScope scope, IDictionary<string, object> userSet)
        {
            if (!(CheckIfAnyOfComputerBranch(userSet) || 
                CheckIfAnyOfOsBranch(userSet) || 
                CheckIfAnyOfCpuBranch(userSet) || 
                userSet.ContainsKey(ParameterSet.WmiSysDrvFreeSpace)))
                return;

            try
            {
                scope.Connect();

                if (CheckIfAnyOfComputerBranch(userSet)) // Any of ComputerSystem branch
                {
                    var query = new ObjectQuery("SELECT * FROM Win32_ComputerSystem");

                    using (var searcher = new ManagementObjectSearcher(scope, query))
                    {
                        using (ManagementObjectCollection queryCollection = searcher.Get())
                        {
                            foreach (var o in queryCollection)
                            {
                                var prop = (ManagementObject) o;

                                if (userSet.ContainsKey(ParameterSet.WmiHostName))
                                    if (prop["Name"] != null)
                                        userSet[ParameterSet.WmiHostName] = prop["Name"].ToString();
                                if (userSet.ContainsKey(ParameterSet.WmiDomain))
                                    if (prop["Domain"] != null)
                                        userSet[ParameterSet.WmiDomain] = prop["Domain"].ToString();
                                if (userSet.ContainsKey(ParameterSet.WmiLastUser))
                                    if (prop["UserName"] != null)
                                        userSet[ParameterSet.WmiLastUser] = prop["UserName"].ToString();
                            }
                        }
                    }
                }

                if (CheckIfAnyOfOsBranch(userSet)) // Any of OperatingSystem branch
                {
                    var query = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");

                    using (var searcher = new ManagementObjectSearcher(scope, query))
                    {
                        using (ManagementObjectCollection queryCollection = searcher.Get())
                        {
                            foreach (var o in queryCollection)
                            {
                                var prop = (ManagementObject) o;

                                if (userSet.ContainsKey(ParameterSet.WmiOsName))
                                    if (prop["Caption"] != null)
                                        userSet[ParameterSet.WmiOsName] = prop["Caption"].ToString();
                                if (userSet.ContainsKey(ParameterSet.WmiHostDesc))
                                    if (prop["Description"] != null)
                                        userSet[ParameterSet.WmiHostDesc] = prop["Description"].ToString();
                                if (userSet.ContainsKey(ParameterSet.WmiOsSp))
                                    if (prop["CSDVersion"] != null)
                                        userSet[ParameterSet.WmiOsSp] = prop["CSDVersion"].ToString();
                                if (userSet.ContainsKey(ParameterSet.WmiOsBuild))
                                    if (prop["BuildNumber"] != null)
                                        userSet[ParameterSet.WmiOsBuild] = prop["BuildNumber"].ToString();
                                if (userSet.ContainsKey(ParameterSet.WmiOsInstDate))
                                    if (prop["InstallDate"] != null)
                                        userSet[ParameterSet.WmiOsInstDate] =
                                            ManagementDateTimeConverter.ToDateTime(prop["InstallDate"].ToString());
                                if (userSet.ContainsKey(ParameterSet.WmiLastBootTime))
                                    if (prop["LastBootUpTime"] != null)
                                        userSet[ParameterSet.WmiLastBootTime] =
                                            ManagementDateTimeConverter.ToDateTime(prop["LastBootUpTime"].ToString());
                                if (userSet.ContainsKey(ParameterSet.WmiPhysMemory))
                                    if (prop["TotalVisibleMemorySize"] != null)
                                        userSet[ParameterSet.WmiPhysMemory] =
                                            (ulong)prop["TotalVisibleMemorySize"];
                            }
                        }
                    }
                }

                if (CheckIfAnyOfCpuBranch(userSet)) // Any of Processor branch
                {
                    var query = new ObjectQuery("SELECT * FROM Win32_Processor");

                    using (var searcher = new ManagementObjectSearcher(scope, query))
                    {
                        using (ManagementObjectCollection queryCollection = searcher.Get())
                        {
                            foreach (var o in queryCollection)
                            {
                                var prop = (ManagementObject) o;

                                if (userSet.ContainsKey(ParameterSet.WmiCpu))
                                    if (prop["Name"] != null)
                                        userSet[ParameterSet.WmiCpu] = prop["Name"].ToString().Trim();
                                if (userSet.ContainsKey(ParameterSet.WmiOsArch) && prop["AddressWidth"] != null)
                                    userSet[ParameterSet.WmiOsArch] = prop["AddressWidth"].ToString() == "32" ? "x86" : "x64";
                                if (userSet.ContainsKey(ParameterSet.WmiCpuCoresCnt))
                                    if (prop["NumberOfCores"] != null)
                                        userSet[ParameterSet.WmiCpuCoresCnt] = Convert.ToInt32(prop["NumberOfCores"]);
                                if (userSet.ContainsKey(ParameterSet.WmiCpuLogicalCnt))
                                    if (prop["NumberOfLogicalProcessors"] != null)
                                        userSet[ParameterSet.WmiCpuLogicalCnt] = Convert.ToInt32(prop["NumberOfLogicalProcessors"]);
                            }
                        }
                    }
                }

                if (userSet.ContainsKey(ParameterSet.WmiSysDrvFreeSpace)) // Any of LogicalDisk branch
                {
                    var query = new ObjectQuery("SELECT FreeSpace from Win32_LogicalDisk Where DeviceID = 'C:'");

                    using (var searcher = new ManagementObjectSearcher(scope, query))
                    {
                        using (ManagementObjectCollection queryCollection = searcher.Get())
                        {
                            foreach (var o in queryCollection)
                            {
                                var prop = (ManagementObject) o;

                                if (userSet.ContainsKey(ParameterSet.WmiSysDrvFreeSpace))
                                    if (prop["FreeSpace"] != null)
                                        userSet[ParameterSet.WmiSysDrvFreeSpace] = (ulong) prop["FreeSpace"];
                            }
                        }
                    }
                }
            }
            catch (COMException e)
            {
                throw new RemoteWmiException("Couldn't connect to remote WMI service.", e);
            }
            catch (UnauthorizedAccessException e)
            {
                throw new RemoteWmiException("Access denied.", e);
            }
            catch (Exception e)
            {
                throw new RemoteWmiException("Unknown exception", e);
            }
        }

        private static bool CheckIfAnyOfComputerBranch(IDictionary<string, object> userSet)
        {
            return userSet.ContainsKey(ParameterSet.WmiHostName) ||
                   userSet.ContainsKey(ParameterSet.WmiDomain) ||
                   userSet.ContainsKey(ParameterSet.WmiLastUser);
        }

        private static bool CheckIfAnyOfOsBranch(IDictionary<string, object> userSet)
        {
            return userSet.ContainsKey(ParameterSet.WmiOsName) ||
                   userSet.ContainsKey(ParameterSet.WmiHostDesc) ||
                   userSet.ContainsKey(ParameterSet.WmiOsSp) ||
                   userSet.ContainsKey(ParameterSet.WmiOsBuild) ||
                   userSet.ContainsKey(ParameterSet.WmiOsInstDate) ||
                   userSet.ContainsKey(ParameterSet.WmiLastBootTime) ||
                   userSet.ContainsKey(ParameterSet.WmiPhysMemory);
        }

        private static bool CheckIfAnyOfCpuBranch(IDictionary<string, object> userSet)
        {
            return userSet.ContainsKey(ParameterSet.WmiCpu) ||
                   userSet.ContainsKey(ParameterSet.WmiOsArch) ||
                   userSet.ContainsKey(ParameterSet.WmiCpuCoresCnt) ||
                   userSet.ContainsKey(ParameterSet.WmiCpuLogicalCnt);
        }
    }
}
