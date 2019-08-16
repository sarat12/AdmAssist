using RemoteQueries.Exceptions;
using SnmpSharpNet;
using System;
using System.Collections.Generic;
using System.Net;

namespace RemoteQueries
{
    public static class RemoteSnmp
    {
        public static void GetAllInfo(IPAddress ip, IDictionary<string, object> userSet)
        {
            if (!CheckIfAny(userSet))
                return;

            try
            {
                OctetString community = new OctetString("public");

                // Define agent parameters class
                AgentParameters param = new AgentParameters(community) {Version = SnmpVersion.Ver1};
                // Set SNMP version to 1 (or 2)
                // Construct the agent address object
                // IpAddress class is easy to use here because
                //  it will try to resolve constructor parameter if it doesn't
                //  parse to an IP address

                // Construct target
                UdpTarget target = new UdpTarget(ip, 161, 1000, 1);

                // Pdu class used for all requests
                Pdu pdu = new Pdu(PduType.Get);
                if (userSet.ContainsKey(ParameterSet.SnmpDesc))
                    pdu.VbList.Add("1.3.6.1.2.1.1.1.0"); //sysDescr
                if (userSet.ContainsKey(ParameterSet.SnmpObjId))
                    pdu.VbList.Add("1.3.6.1.2.1.1.2.0"); //sysObjectID
                if (userSet.ContainsKey(ParameterSet.SnmpUpTime))
                    pdu.VbList.Add("1.3.6.1.2.1.1.3.0"); //sysUpTime
                if (userSet.ContainsKey(ParameterSet.SnmpContact))
                    pdu.VbList.Add("1.3.6.1.2.1.1.4.0"); //sysContact
                if (userSet.ContainsKey(ParameterSet.SnmpHostName))
                    pdu.VbList.Add("1.3.6.1.2.1.1.5.0"); //sysName

                // Make SNMP request
                SnmpV1Packet result = (SnmpV1Packet)target.Request(pdu, param);

                // If result is null then agent didn't reply or we couldn't parse the reply.
                if (result != null)
                {
                    // ErrorStatus other then 0 is an error returned by 
                    // the Agent - see SnmpConstants for error definitions
                    if (result.Pdu.ErrorStatus != 0)
                    {
                        // agent reported an error with the request
                        throw new RemoteSnmpException(
                            $"Error in SNMP reply. Error {result.Pdu.ErrorStatus} index {result.Pdu.ErrorIndex}");
                    }

                    // Reply variables are returned in the same order as they were added
                    //  to the VbList
                    var id = 0;
                    if (userSet.ContainsKey(ParameterSet.SnmpDesc))
                        userSet[ParameterSet.SnmpDesc] = result.Pdu.VbList[id++].Value.ToString();
                    if (userSet.ContainsKey(ParameterSet.SnmpObjId))
                        userSet[ParameterSet.SnmpObjId] = result.Pdu.VbList[id++].Value.ToString();
                    if (userSet.ContainsKey(ParameterSet.SnmpUpTime))
                        userSet[ParameterSet.SnmpUpTime] = result.Pdu.VbList[id++].Value.ToString();
                    if (userSet.ContainsKey(ParameterSet.SnmpContact))
                        userSet[ParameterSet.SnmpContact] = result.Pdu.VbList[id++].Value.ToString();
                    if (userSet.ContainsKey(ParameterSet.SnmpHostName))
                        userSet[ParameterSet.SnmpHostName] = result.Pdu.VbList[id].Value.ToString();

                }
                else
                {
                    throw new RemoteSnmpException("No response received from SNMP agent.");
                }
                target.Close();
            }
            catch (Exception e)
            {
                throw new RemoteSnmpException(e.Message, e);
            }
        }

        private static bool CheckIfAny(IDictionary<string, object> userSet)
        {
            return userSet.ContainsKey(ParameterSet.SnmpDesc) ||
                   userSet.ContainsKey(ParameterSet.SnmpObjId) ||
                   userSet.ContainsKey(ParameterSet.SnmpUpTime) ||
                   userSet.ContainsKey(ParameterSet.SnmpContact) ||
                   userSet.ContainsKey(ParameterSet.SnmpHostName);
        }
    }
}
