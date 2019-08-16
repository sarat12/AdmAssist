using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;
using AdmAssist.Interfaces;
using AdmAssist.Models;

namespace AdmAssist.Helpers
{
    public static class Extensions
    {
        public static bool ContainsIgnoreCase(this string source, string toCheck)
        {
            return source.IndexOf(toCheck, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static string ReplaceIgnoreCase(this string input, string search, string replacement)
        {
            string result = Regex.Replace(
                input,
                Regex.Escape(search),
                replacement.Replace("$", "$$"),
                RegexOptions.IgnoreCase
            );
            return result;
        }

        public static int ToInt(this IPAddress ipAddress)
        {
            var bytes = ipAddress.GetAddressBytes();
            return BitConverter.ToInt32(new[] { bytes[3], bytes[2], bytes[1], bytes[0] }, 0);
        }

        public static string ToString(this PhysicalAddress address, string separator)
        {
            return string.Join(separator, address.GetAddressBytes()
                .Select(x => x.ToString("X2")));
        }

        public static bool SetSelectedItem(this TreeView treeView, object item)
        {
            return SetSelected(treeView, item);
        }

        private static bool SetSelected(ItemsControl parent, object child)
        {
            if (parent == null || child == null)
                return false;

            TreeViewItem childNode = parent.ItemContainerGenerator
                .ContainerFromItem(child) as TreeViewItem;

            if (childNode != null)
            {
                childNode.Focus();
                childNode.IsSelected = true;
                childNode.IsExpanded = true;
                return true;
            }

            if (parent.Items.Count > 0)
            {
                foreach (object childItem in parent.Items)
                {
                    ItemsControl childControl = parent
                            .ItemContainerGenerator
                            .ContainerFromItem(childItem)
                        as ItemsControl;

                    if (SetSelected(childControl, child))
                        return true;
                }
            }

            return false;
        }

        public static KeyGesture ToInputGesture(this string s)
        {
            if (string.IsNullOrEmpty(s)) return new KeyGesture(Key.None);

            KeyGesture res = null;

            s = s.Replace("Ctrl", "Control");

            var keys = s.Split('+');

            switch (keys.Length)
            {
                case 1: res = new KeyGesture((Key)Enum.Parse(typeof(Key), keys[0])); break;
                case 2: res = new KeyGesture((Key)Enum.Parse(typeof(Key), keys[1]), (ModifierKeys)Enum.Parse(typeof(ModifierKeys), keys[0])); break;
                case 3: res = new KeyGesture((Key)Enum.Parse(typeof(Key), keys[2]), (ModifierKeys)Enum.Parse(typeof(ModifierKeys), keys[0]) | (ModifierKeys)Enum.Parse(typeof(ModifierKeys), keys[1])); break;
                case 4: res = new KeyGesture((Key)Enum.Parse(typeof(Key), keys[3]), (ModifierKeys)Enum.Parse(typeof(ModifierKeys), keys[0]) | (ModifierKeys)Enum.Parse(typeof(ModifierKeys), keys[1]) | (ModifierKeys)Enum.Parse(typeof(ModifierKeys), keys[2])); break;
            }
            return res;
        }

        public static IEnumerable<HostCommandNode> CommandNodesToEnumerable(this IEnumerable<IMenuTreeNode> iMenuTreeNodes)
        {
            return HostCommandsToList(iMenuTreeNodes);
        }

        public static IEnumerable<HostCommandNode> HostCommandsToList(IEnumerable<IMenuTreeNode> menuTree)
        {
            foreach (var iMenuTreeNode in menuTree)
            {
                if (iMenuTreeNode is HostCommandNode)
                {
                    yield return iMenuTreeNode as HostCommandNode;
                }
                else if (iMenuTreeNode is MenuTreeNode)
                {
                    var menuTreeNode = iMenuTreeNode as MenuTreeNode;

                    foreach (var hostCommandNode in HostCommandsToList(menuTreeNode.Children))
                        yield return hostCommandNode;
                }
            }
        }
    }
}
