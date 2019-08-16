using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AdmAssist.Helpers;
using AdmAssist.Models;
using MahApps.Metro.Controls.Dialogs;

namespace AdmAssist.Services
{
    public static class HostCommandArgsProcessor
    {
        public static string Process(string inputArgs, NotifyDynamicDictionary host)
        {
            var processedArgs = ProcessIpArg(inputArgs, (IPAddress)host["Ip"]);

            var fields = host.Select(f => f.Key).ToList();

            foreach (var field in fields)
            {
                if (processedArgs.ContainsIgnoreCase($"%{field}"))
                    processedArgs = processedArgs.ReplaceIgnoreCase($"%{field}", host[field].ToString());
            }

            return processedArgs;
        }

        public static string ProcessIpArg(string inputArgs, IPAddress ip)
        {
            if (!inputArgs.ContainsIgnoreCase("%ip")) return inputArgs;

            return inputArgs.ReplaceIgnoreCase("%ip", ip.ToString());
        }

        public static async Task<string> ProcessUserInputStringArg(string inputArgs, string operationName, IDialogCoordinator dc, object dcContext)
        {
            if (!inputArgs.Contains("%sarg")) return inputArgs;

            var userInput = await dc.ShowInputAsync(dcContext, "Enter argument", $"Please specify argument for the {operationName}:");

            if (userInput == null) return null;

            return inputArgs.Replace("%sarg", userInput);
        }
    }
}
