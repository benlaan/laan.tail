using System;
using System.Collections.Generic;

using MahApps.Metro;

namespace Laan.Tools.Tail
{
    public class ConfigApplication
    {
        public ConfigApplication()
        {
            SingleInstance = true;
            RememberOpenFiles = true;
            RememberWindowState = true;
            WordWrap = true;
        }

        public bool SingleInstance { get; set; }
        public bool RememberOpenFiles { get; set; }
        public bool RememberWindowState { get; set; }
        
        public bool WordWrap { get; set; }
    }
}