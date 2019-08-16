using System.Collections.Generic;
using System.Collections.ObjectModel;
using AdmAssist.Interfaces;

namespace AdmAssist.Models
{
    public class Configuration
    {
        public List<string> UserQuerySet { get; set; }

        public ScanningOptions ScanningOptions { get; set; }

        public List<IpAddressRange> IpAddressRanges { get; set; }

        public byte MaxThreads { get; set; }

        public string Theme { get; set; }
        public string Accent { get; set; }

        public ObservableCollection<IMenuTreeNode> MenuTree { get; set; }
    }
}
