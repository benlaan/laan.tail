using System;
using System.Collections.Generic;
using MahApps.Metro;

namespace Laan.Tools.Tail.Win
{
    public class ConfigAppearance
    {
        public ConfigAppearance()
        {
            Theme = MahApps.Metro.Theme.Light;
            AccentColor = "Blue";
            FontFamily = "Consolas";
            FontSize = 12;
        }

        public Theme Theme { get; set; }
        public string AccentColor { get; set; }
        public string FontFamily { get; set; }
        public int FontSize { get; set; }
    }
}
