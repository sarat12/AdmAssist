using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using RemoteQueries.Types;

namespace RemoteQueries
{
    public static class ParameterSet
    {
        #region Consts

        public const string DnsName = "Host Name (DNS)";
        public const string ArpMac = "Mac Adress (Arp)";
        public const string NbMac = "Mac Adress (NetBios)";
        public const string NbHostName = "Host Name (NetBios)";
        public const string NbGroup = "Group (NetBios)";
        public const string RrHostName = "Host Name (Rm Reg)";
        public const string RrHostDesc = "Host Description (Rm Reg)";
        public const string RrDomain = "Domain (Rm Reg)";
        public const string RrLastUser = "Last User (Rm Reg)";
        public const string RrOsName = "OS Name (Rm Reg)";
        public const string RrOsArch = "OS Architecture (Rm Reg)";
        public const string RrOsSp = "OS Service Pack (Rm Reg)";
        public const string RrOsBuild = "OS Build (Rm Reg)";
        public const string RrOsInstDate = "OS Install Date (Rm Reg)";
        public const string RrCpu = "CPU (Rm Reg)";
        public const string SnmpDesc = "Description (SNMP)";
        public const string SnmpObjId = "Object Id (SNMP)";
        public const string SnmpUpTime = "Up Time (SNMP)";
        public const string SnmpContact = "Contact (SNMP)";
        public const string SnmpHostName = "Host Name (SNMP)";
        public const string WmiHostName = "Host Name (Wmi)";
        public const string WmiHostDesc = "Host Description (Wmi)";
        public const string WmiDomain = "Domain (Wmi)";
        public const string WmiLastUser = "Last User (Wmi)";
        public const string WmiOsName = "OS Name (Wmi)";
        public const string WmiOsArch = "OS Architecture (Wmi)";
        public const string WmiOsSp = "OS Service Pack (Wmi)";
        public const string WmiOsBuild = "OS Build (Wmi)";
        public const string WmiOsInstDate = "OS Install Date (Wmi)";
        public const string WmiLastBootTime = "Last Boot Time (Wmi)";
        public const string WmiCpu = "CPU (Wmi)";
        public const string WmiCpuCoresCnt = "CPU Cores Count (Wmi)";
        public const string WmiCpuLogicalCnt = "Logical Poccessors Count (Wmi)";
        public const string WmiSysDrvFreeSpace = "System Drive Free Space (Wmi)";
        public const string WmiPhysMemory = "Physical Memory (Wmi)";

        #endregion

        private static List<QueryParameter> _set;

        public static List<QueryParameter> Set
        {
            get => _set ?? (_set = GetAvaliableSet());
            set => _set = value;
        }

        public static List<QueryParameter> GetAvaliableSet()
        {
            return new List<QueryParameter>
            {
                new QueryParameter(DnsName, typeof(string), RemoteQueryGroups.Dns),
                new QueryParameter(ArpMac, typeof(PhysicalAddress), RemoteQueryGroups.Arp),
                new QueryParameter(NbMac, typeof(PhysicalAddress), RemoteQueryGroups.NetBios),
                new QueryParameter(NbHostName, typeof(string), RemoteQueryGroups.NetBios),
                new QueryParameter(NbGroup, typeof(string), RemoteQueryGroups.NetBios),
                new QueryParameter(RrHostName, typeof(string), RemoteQueryGroups.RemoteRegistry),
                new QueryParameter(RrHostDesc, typeof(string), RemoteQueryGroups.RemoteRegistry),
                new QueryParameter(RrDomain, typeof(string), RemoteQueryGroups.RemoteRegistry),
                new QueryParameter(RrLastUser, typeof(string), RemoteQueryGroups.RemoteRegistry),
                new QueryParameter(RrOsName, typeof(string), RemoteQueryGroups.RemoteRegistry),
                new QueryParameter(RrOsArch, typeof(string), RemoteQueryGroups.RemoteRegistry),
                new QueryParameter(RrOsSp, typeof(string), RemoteQueryGroups.RemoteRegistry),
                new QueryParameter(RrOsBuild, typeof(string), RemoteQueryGroups.RemoteRegistry),
                new QueryParameter(RrOsInstDate, typeof(DateTime), RemoteQueryGroups.RemoteRegistry),
                new QueryParameter(RrCpu, typeof(string), RemoteQueryGroups.RemoteRegistry),
                new QueryParameter(SnmpDesc, typeof(string), RemoteQueryGroups.Snmp),
                new QueryParameter(SnmpObjId, typeof(string), RemoteQueryGroups.Snmp),
                new QueryParameter(SnmpUpTime, typeof(string), RemoteQueryGroups.Snmp),
                new QueryParameter(SnmpContact, typeof(string), RemoteQueryGroups.Snmp),
                new QueryParameter(SnmpHostName, typeof(string), RemoteQueryGroups.Snmp),
                new QueryParameter(WmiHostName, typeof(string), RemoteQueryGroups.Wmi),
                new QueryParameter(WmiHostDesc, typeof(string), RemoteQueryGroups.Wmi),
                new QueryParameter(WmiDomain, typeof(string), RemoteQueryGroups.Wmi),
                new QueryParameter(WmiLastUser, typeof(string), RemoteQueryGroups.Wmi),
                new QueryParameter(WmiOsName, typeof(string), RemoteQueryGroups.Wmi),
                new QueryParameter(WmiOsArch, typeof(string), RemoteQueryGroups.Wmi),
                new QueryParameter(WmiOsSp, typeof(string), RemoteQueryGroups.Wmi),
                new QueryParameter(WmiOsBuild, typeof(string), RemoteQueryGroups.Wmi),
                new QueryParameter(WmiOsInstDate, typeof(DateTime), RemoteQueryGroups.Wmi),
                new QueryParameter(WmiLastBootTime, typeof(DateTime), RemoteQueryGroups.Wmi),
                new QueryParameter(WmiCpu, typeof(string), RemoteQueryGroups.Wmi),
                new QueryParameter(WmiCpuCoresCnt, typeof(int), RemoteQueryGroups.Wmi),
                new QueryParameter(WmiCpuLogicalCnt, typeof(int), RemoteQueryGroups.Wmi),
                new QueryParameter(WmiSysDrvFreeSpace, typeof(ulong), RemoteQueryGroups.Wmi),
                new QueryParameter(WmiPhysMemory, typeof(ulong), RemoteQueryGroups.Wmi)
            };
        }
    }
}
