using System;
using System.Diagnostics;
using System.IO;

namespace TightVncViewerLauncher
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var shortcutPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp.vnc");
                var templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "shortcut.template");

                var template = File.ReadAllText(templatePath).Replace("%ip", args[0]);

                File.WriteAllText(shortcutPath, template);

                Process.Start(shortcutPath);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Fail to start TightVncViewer on host " + args[0] + ": " + e.Message);
            }
        }
    }
}
