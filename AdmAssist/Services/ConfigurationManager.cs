using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Windows;
using AdmAssist.Converters;
using AdmAssist.Interfaces;
using AdmAssist.Models;
using Newtonsoft.Json;
using RemoteQueries;
using RemoteQueries.Types;

namespace AdmAssist.Services
{
    public delegate void ConfigurationLoaded();
    public delegate void ConfigurationSaved();

    public static class ConfigurationManager
    {
        public static event ConfigurationLoaded ConfigurationLoaded;
        public static event ConfigurationSaved ConfigurationSaved;

        public static Configuration Configuration { get; private set; }

        #region Public Methods

        public static void LoadDefaults()
        {
            Configuration = new Configuration
            {
                MaxThreads = 50,
                Theme = "BaseLight",
                Accent = "Blue"
            };
            
            LoadDefaultQuerySet();
            LoadDefaultScanningOptions();
            LoadDefaultIpAddressRanges();
            LoadDefaultHostCommandsMenuTree();

            ConfigurationLoaded?.Invoke();
        }

        /// <summary>
        /// Loads configuration from file
        /// </summary>
        /// <param name="requireNotification">Specifies if ConfigurationLoaded event should be invoked</param>
        public static void LoadConfiguration(bool requireNotification)
        {
            try
            {
                Configuration = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText("cfg"),
                    GetJsonSerializerSettings());

                if (requireNotification)
                    ConfigurationLoaded?.Invoke();
            }
            catch (Exception e)
            {
                MessageBox.Show(
                    $"Fail to load configuration file. Reason: {e.Message} \n\n Default configuration will be loaded.");
                LoadDefaults();
            }
        }

        /// <summary>
        /// Writes configuration
        /// </summary>
        /// <param name="requireNotification">Specifies if ConfigurationSaved event should be invoked</param>
        public static void StoreConfiguration(bool requireNotification)
        {
            try
            {
                File.WriteAllText("cfg", JsonConvert.SerializeObject(Configuration, GetJsonSerializerSettings()));

                if (requireNotification)
                    ConfigurationSaved?.Invoke();
            }
            catch (Exception e)
            {
                MessageBox.Show(
                    $"Fail to write configuration file. Reason: {e.Message}");
            }
        }

        #endregion

        #region Private Methods

        private static void LoadDefaultQuerySet()
        {
            Configuration.UserQuerySet = new List<string>();
            foreach (var remoteQueryParameter in ParameterSet.Set)
            {
                if (remoteQueryParameter.Group == RemoteQueryGroups.Wmi)
                    Configuration.UserQuerySet.Add(remoteQueryParameter.Name);
            }
        }

        private static void LoadDefaultScanningOptions()
        {
            Configuration.ScanningOptions = new ScanningOptions { AllowSupervising = true };
        }

        private static void LoadDefaultIpAddressRanges()
        {
            Configuration.IpAddressRanges = new List<IpAddressRange>
            {
                new IpAddressRange(IPAddress.Parse("192.168.1.1"), IPAddress.Parse("192.168.1.1"))
            };
        }

        private static void LoadDefaultHostCommandsMenuTree()
        {
            Configuration.MenuTree = new ObservableCollection<IMenuTreeNode>
            {
                new HostCommandNode {Name = "Rescan Now", Shortcut = "Ctrl+R", Executable = "%rescan"},
                new HostCommandNode {Name = "Manage", Shortcut = "Ctrl+M", Executable = "mmc", Argumets = "compmgmt.msc /a /computer=%ip"},
                new HostCommandNode {Name = "Run TightVNC viewer", Shortcut = "Ctrl+V", Executable = "plugins\\TightVncViewerLauncher\\TightVncViewerLauncher.exe", Argumets = "%ip", HidePocessWindow = true, IsDoubleClick = true },
                new HostCommandNode {Name = "Change Computer Description", Shortcut = "Ctrl+D", Executable = "WMIC", Argumets = "/node:%ip os set description='%sarg'", HidePocessWindow = true, WaitProcessExit = true , RedirectOutputToLog = true, RequiresRescanAfterExecution = true},
                new HostCommandNode {Name = "CommandLine", Executable = "cmd", Argumets = "plugins\\paexec.exe \\\\%ip -s cmd.exe"},
                new HostCommandNode {Name = "Task Manager", Executable = "plugins\\RemoteWmiProcessManager\\RemoteWmiProcessManager.exe", Argumets = "%ip"},
                new HostCommandNode {Name = "Software Viewer", Executable = "plugins\\RemoteInstalledSoftwareViewer\\RemoteInstalledSoftwareViewer.exe", Argumets = "%ip"}
            };
        }

        private static JsonSerializerSettings GetJsonSerializerSettings()
        {
            var jsonSerializerSettings = new JsonSerializerSettings { Formatting = Formatting.Indented, TypeNameHandling = TypeNameHandling.All };
            jsonSerializerSettings.Converters.Add(new IpAddressJsonConverter());

            return jsonSerializerSettings;
        }

        #endregion

    }
}
