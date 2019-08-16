using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace RemoteQueries
{
    public static class RemoteNetBios
    {
        //Recieves an IP address and any available port to listen on 
        public static void GetInfo(IPAddress ip, int intPortNum, IDictionary<string, object> userSet)
        {
            if (!CheckIfAny(userSet))
                return;

            Socket soc = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            IPEndPoint ipEndPt = new IPEndPoint(ip, 137);
            EndPoint endPt = ipEndPt;

            IPEndPoint inIpEndPt = new IPEndPoint(ip, intPortNum);
            EndPoint inEndPt = inIpEndPt;

            soc.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 50);
            soc.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 50);

            //Datagram declaration expected to receive from host 
            byte[] dataIn = new byte[4096];


            int intTries = 0;

            bool bolResponded = false;

            do
            {
                soc.SendTo(NbQuery(), endPt);
                try
                {
                    //Get data from host 
                    soc.ReceiveFrom(dataIn, ref inEndPt);
                    bolResponded = true;
                }
                catch (SocketException se)
                {
                    intTries++;
                    if (se.ErrorCode == 10060)
                    {
                        //handleTimed out 
                    }
                }
            }
            while ((!bolResponded) && intTries < 1);

            if (bolResponded)
            {
                //parse the MAC Address from the data received from host 

                if (userSet.ContainsKey(ParameterSet.NbMac))
                    userSet[ParameterSet.NbMac] = GetMac(dataIn);

                if (userSet.ContainsKey(ParameterSet.NbHostName))
                    userSet[ParameterSet.NbHostName] = GetNebiosComputer(dataIn);

                if (userSet.ContainsKey(ParameterSet.NbGroup))
                    userSet[ParameterSet.NbGroup] = GetNebiosGroup(dataIn);

                //if ((RemoteQuerySet.Set & RemoteNetBiosQuerySet.User) == RemoteNetBiosQuerySet.User)
                //    result.Add(RemoteNetBiosQuerySet.User.GetParameterDescription(), getRemoteMachineNameTable(dataIn).Count.ToString());

                //MacAddress = getMAC(dataIn);
                //Computer = getNebiosComputer(dataIn);
                //Group = GetNebiosGroup(dataIn);
                //User = getRemoteMachineNameTable(dataIn).Count.ToString();
            }
        }

        //NBNS Request Packet Contents 
        private static byte[] NbQuery()
        {

            byte[] bdata = { 0x80, 0xff, 0x00, 0x00, 0x00,
                             0x01, 0x00, 0x00, 0x00, 0x00,
                             0x00, 0x00, 0x20, 0x43, 0x4B,
                             0x41, 0x41, 0x41, 0x41, 0x41,
                             0x41, 0x41, 0x41, 0x41, 0x41,
                             0x41, 0x41, 0x41, 0x41, 0x41,
                             0x41, 0x41, 0x41, 0x41, 0x41,
                             0x41, 0x41, 0x41, 0x41, 0x41,
                             0x41, 0x41, 0x41, 0x41, 0x41,
                             0x00, 0x00, 0x21, 0x00, 0x01};

            return bdata;
        }


        //Parse the Packet sent by the host and get the value @byt 56 
        private static string GetNebiosComputer(byte[] dtResponse)
        {
            string strNetbiosComputer = "";

            //int intStart = (int)dtResponse[56];

            int intOffset = 57;
            for (int x = 0; x < 15; x++)
            {
                strNetbiosComputer += Convert.ToChar(dtResponse[intOffset + x]);
            }
            return strNetbiosComputer;
        }

        private static List<object> GetRemoteMachineNameTable(byte[] dtResponse)
        {
            List<object> remoteMachineNames = new List<object>();

            //Get no of names in the Response Packet (position 56 of response Packet) 
            var strNoOfRemoteMachineName = Convert.ToInt32(dtResponse[56]);

            for (int remoteNameCounter = 0; remoteNameCounter <= strNoOfRemoteMachineName - 1; remoteNameCounter++)
            {
                var currentName = "";
                //Get first 15 characters after position 56 
                for (int x = 1; x <= 15; x++)
                {
                    //18 characters per name, 
                    byte newByte = dtResponse[x + (56 + remoteNameCounter * 18)];

                    //checked if printable chars 0x21 to 0x7E 
                    //0x20 = space 
                    //0x7F = Delete 
                    //TRAP GROUP NAMES - Must have a 1E NetBios Suffix 
                    if (newByte > 0x19 && newByte < 0x7E)
                        currentName += Convert.ToChar(newByte);
                    else
                        currentName += " ";
                }

                //HEC Notes 
                //NAME_FLAGS = Bytes 17 and 18 Definition 
                //Taken from http://www.faqs.org/rfcs/rfc1002.html 
                // 
                //RESERVED 7-15 Reserved for future use. Must be zero (0). 
                //PRM 6 Permanent Name Flag. If one (1) then entry 
                // is for the permanent node name. Flag is zero 
                // (0) for all other names. 
                //ACT 5 Active Name Flag. All entries have this flag 
                // set to one (1). 
                //CNF 4 Conflict Flag. If one (1) then name on this 
                // node is in conflict. 
                //DRG 3 Deregister Flag. If one (1) then this name 
                // is in the process of being deleted. 
                //ONT 1,2 Owner Node Type: 
                // 00 = B node 
                // 01 = P node 
                // 10 = M node 
                // 11 = Reserved for future use 
                //G 0 Group Name Flag. 
                // If one (1) then the name is a GROUP NetBIOS 
                // name. 
                // If zero (0) then it is a UNIQUE NetBIOS name. 

                remoteMachineNames.Add(currentName + $"{dtResponse[16 + (56 + remoteNameCounter * 18)]:X2}" + Convert.ToString(dtResponse[17 + (56 + remoteNameCounter * 18)], 2).PadLeft(8, '0'));
            }

            return remoteMachineNames;
        }

        private static string GetNebiosGroup(byte[] dtResponse)
        {
            String netbiosGroupName = "";
            foreach (string tmpString in GetRemoteMachineNameTable(dtResponse))
            {
                if (tmpString.Substring(15, 2) == "1E" || tmpString.Substring(17, 1) == "1")
                {
                    netbiosGroupName = tmpString.Substring(0, 15);
                    break;
                }

            }

            return netbiosGroupName;
        }

        private static PhysicalAddress GetMac(byte[] dtResponse)
        {
            //string strMac = "";
            //intStart reads the value @ byte 56 which contains the 
            //no of names in the Response Packet 
            int intStart = dtResponse[56];

            //1 name contains 16 bytes followed by 2 bytes name flag = 18 Bytes 
            int intOffset = 56 + intStart * 18 + 1;

            //for (int x = 0; x < 6; x++)
            //{
            //    strMac += $"{dtResponse[intOffset + x]:X2}" + ":";
            //}

            var phisAddrBytes = new byte[6];
            Array.Copy(dtResponse, intOffset, phisAddrBytes, 0, 6);

            return new PhysicalAddress(phisAddrBytes);

            //strMac = strMac.Remove(strMac.Length - 1, 1);
            //return strMac;
        }

        private static bool CheckIfAny(IDictionary<string, object> userSet)
        {
            return userSet.ContainsKey(ParameterSet.NbMac) ||
                   userSet.ContainsKey(ParameterSet.NbHostName) ||
                   userSet.ContainsKey(ParameterSet.NbGroup);
        }
    }
}
