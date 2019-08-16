using System;
using Microsoft.Win32;

namespace RemoteRegistryComuterDescriptionChanger
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                using (var reg = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, args[0]))
                using (var key = reg.OpenSubKey(@"System\CurrentControlSet\services\LanmanServer\Parameters\", true))
                {
                    key?.SetValue("srvcomment", string.Join(" ", args, 1, args.Length - 1));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
