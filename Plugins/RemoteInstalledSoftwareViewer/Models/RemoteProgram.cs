using System;

namespace RemoteInstalledSoftwareViewer.Models
{
    public class RemoteProgram
    {
        public string Name { get; set; }
        
        public string Version { get; set; }
        
        public string Vendor { get; set; }

        public string IstallLocation { get; set; }
        
        public DateTime? InstallationDate { get; set; }
        
        public string UninstallString { get; set; }

        public RemoteProgram(string uinstallString, DateTime? installationDate, string istallLocation, string name, string vendor, string version)
        {
            UninstallString = uinstallString;
            InstallationDate = installationDate;
            IstallLocation = istallLocation;
            Name = name;
            Vendor = vendor;
            Version = version;
        }
    }
}
