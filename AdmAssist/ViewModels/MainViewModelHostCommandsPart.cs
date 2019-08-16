using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using AdmAssist.Enums;
using AdmAssist.Helpers;
using AdmAssist.Interfaces;
using AdmAssist.Models;
using AdmAssist.Services;
using AdmAssist.ViewModels.Commands;
using MahApps.Metro.Controls.Dialogs;

namespace AdmAssist.ViewModels
{
    public partial class MainViewModel
    {
        public ICommand DoubleClickCommand
        {
            get
            {
                var dcCommandNode = GetIsDoubleClickHostCommand();
                return dcCommandNode?.HostCommand;
            }
        }

        #region Private Methods

        private void InitHostCommands(IEnumerable<IMenuTreeNode> commandTree)
        {
            foreach (var iMenuTreeNode in commandTree)
            {
                if (iMenuTreeNode is HostCommandNode)
                {
                    var hostCommandNode = iMenuTreeNode as HostCommandNode;

                    hostCommandNode.PropertyChanged += HostCommandNode_PropertyChanged; //for checking unique parameters

                    hostCommandNode.HostCommand = new HostCommand(ProceedHostCommand, hostCommandNode);
                }
                else if (iMenuTreeNode is MenuTreeNode)
                {
                    var menuTreeNode = iMenuTreeNode as MenuTreeNode;

                    InitHostCommands(menuTreeNode.Children);
                }
            }
        }

        private void HostCommandNode_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var hostCommandNode = (HostCommandNode) sender;

            if (e.PropertyName.Equals(nameof(HostCommandNode.IsDoubleClick)))
                EnsureDoubleClickHostCommandIsUnique(hostCommandNode);
            if (e.PropertyName.Equals(nameof(HostCommandNode.Shortcut)))
                EnsureShortcutIsUnique(hostCommandNode);
        }

        private void EnsureShortcutIsUnique(HostCommandNode hostCommandNode)
        {
            if (string.IsNullOrEmpty(hostCommandNode.Shortcut)) return;

            var commands = HostOperationsMenuTree.CommandNodesToEnumerable();

            foreach (var commandNode in commands)
            {
                if (commandNode.Shortcut.Equals(hostCommandNode.Shortcut) && commandNode != hostCommandNode)
                {
                    _dialogCoordinator.ShowMessageAsync(this, "Oops..", $"The '{commandNode.Shortcut}' is already taken by '{commandNode.Name}' operation, please change it first.");

                    hostCommandNode.Shortcut = string.Empty;
                }
            }
        }

        private void EnsureDoubleClickHostCommandIsUnique(HostCommandNode hostCommandNode)
        {
            if (!hostCommandNode.IsDoubleClick) return;

            var commands = HostOperationsMenuTree.CommandNodesToEnumerable();

            foreach (var commandNode in commands)
            {
                if (commandNode.IsDoubleClick && commandNode != hostCommandNode)
                {
                    _dialogCoordinator.ShowMessageAsync(this, "Oops..", $"The '{commandNode.Name}' is already set as double click operation, please uncheck it first.");

                    hostCommandNode.IsDoubleClick = false;
                }
            }
        }

        private async Task<MessageDialogResult> PromtUserForMultiHostSelectionConfirmation(HostCommandNode hostCommandNode, List<NotifyDynamicDictionary> hostsList)
        {
            return await _dialogCoordinator.ShowMessageAsync(this, "Confirm",
                $"You have selected {hostsList.Count} hosts, are you completely sure you want to run the '{hostCommandNode.Name}' operation for all of them?!",
                MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings()
                {
                    AffirmativeButtonText = "Yes I'm sure",
                    NegativeButtonText = "Cancel",
                    DefaultButtonFocus = MessageDialogResult.Negative
                });
        }

        #endregion

        private async void ProceedHostCommand(IEnumerable<NotifyDynamicDictionary> selectedHosts, HostCommandNode hostCommandNode)
        {
            if (string.IsNullOrWhiteSpace(hostCommandNode.Executable)) return;

            var hostsList = selectedHosts.ToList();

            if (hostsList.Count > 1)
            {
                var diagRes = await PromtUserForMultiHostSelectionConfirmation(hostCommandNode, hostsList);

                if (diagRes != MessageDialogResult.Affirmative) return;
            }

            if (hostCommandNode.Executable.Equals("%rescan", StringComparison.InvariantCultureIgnoreCase))
            {
                RescanHost(hostsList);
                return;
            }

            foreach (var host in hostsList)
            {
                var args = hostCommandNode.Argumets;
                args = HostCommandArgsProcessor.Process(args, host);
                args = await HostCommandArgsProcessor.ProcessUserInputStringArg(args, hostCommandNode.Name, _dialogCoordinator, this);

                if (args == null) continue; // null is returned by input dialog if canceled

                var process = new Process
                {
                    StartInfo =
                    {
                        FileName = hostCommandNode.Executable,
                        Arguments = args
                    }
                };

                if (hostCommandNode.HidePocessWindow)
                {
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                }

                if (hostCommandNode.WaitProcessExit && hostCommandNode.RedirectOutputToLog)
                {
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.OutputDataReceived += (sender, eventArgs) =>
                    {
                        Loggers.HostOperationsLogger.Information($"{hostCommandNode.Name} on {(IPAddress)host["Ip"]}: {eventArgs.Data}");
                    };
                    process.ErrorDataReceived += (sender, eventArgs) =>
                    {
                        if (string.IsNullOrWhiteSpace(eventArgs.Data)) return;
                        Loggers.HostOperationsLogger.Error($"{hostCommandNode.Name} on {(IPAddress)host["Ip"]}: {eventArgs.Data}");
                    };
                }

                await Task.Run(() =>
                {
                    try
                    {
                        process.Start();

                        if (hostCommandNode.WaitProcessExit)
                        {
                            if (hostCommandNode.RedirectOutputToLog)
                            {
                                process.BeginOutputReadLine();
                                process.BeginErrorReadLine();
                            }

                            process.WaitForExit();
                        }

                        if (hostCommandNode.RequiresRescanAfterExecution)
                            RescanHost(new List<NotifyDynamicDictionary> {host});
                    }
                    catch (Exception e)
                    {
                        Loggers.HostOperationsLogger.Error(e.Message);
                    }
                });
            }
        }

        private void RescanHost(IEnumerable<NotifyDynamicDictionary> selectedHosts)
        {
            if (AppStats.ApplicationState != AppState.Supervising && AppStats.ApplicationState != AppState.Chilling)
                return;

            new Thread(() => RescanProc(selectedHosts)).Start();
        }

        public HostCommandNode GetHostCommandByShortcut(string shortcut)
        {
            var commands = HostOperationsMenuTree.CommandNodesToEnumerable();

            var foundCommand = commands.FirstOrDefault(c => !string.IsNullOrWhiteSpace(c.Shortcut) && c.Shortcut.Equals(shortcut));

            return foundCommand;
        }

        public HostCommandNode GetIsDoubleClickHostCommand()
        {
            var commands = HostOperationsMenuTree.CommandNodesToEnumerable();

            var foundCommand = commands.FirstOrDefault(c => c.IsDoubleClick);

            return foundCommand;
        }
    }
}
