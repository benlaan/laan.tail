using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

using Microsoft.Win32;

namespace Laan.Tools.Tail
{
    public class SystemSettings : ISystemSettings
    {
        private const string Key = @"Software\Laan Software\LaanTail";
        
        public IList<string> OpenFiles { get; set; }
        public int ActiveTabIndex { get; set; }
        public WindowState WindowState { get; set; }

        /// <summary>
        /// Initializes a new instance of the SystemSettings class.
        /// </summary>
        public SystemSettings()
        {
            OpenFiles = new List<string>();
            ActiveTabIndex = -1;
            WindowState = System.Windows.WindowState.Normal;
        }

        public static SystemSettings Load()
        {
            var systemSettings = new SystemSettings();
            var key = Registry.CurrentUser.OpenSubKey(Key);
            if (key != null)
            {
                string files = (string)key.GetValue("OpenFiles");
                systemSettings.OpenFiles = files.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                systemSettings.ActiveTabIndex = (int)key.GetValue("ActiveTabIndex");
                systemSettings.WindowState = (WindowState)(int)key.GetValue("WindowState");
            }

            return systemSettings;
        }

        public void Save()
        {
            var key = Registry.CurrentUser.OpenSubKey(Key, true) ?? Registry.CurrentUser.CreateSubKey(Key);

            key.SetValue("OpenFiles", String.Join(";", OpenFiles));
            key.SetValue("ActiveTabIndex", ActiveTabIndex);
            key.SetValue("WindowState", (int)WindowState);
        }
    }
}
