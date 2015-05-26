using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Laan.Tools.Tail
{
    public interface IUserSettings
    {
        bool WordWrap { get; set; }
        IShell Shell { get; set; }
        TextWrapping TextWrapping { get; }
        ScrollBarVisibility ScrollbarVisibilty { get; }
        void Save();
        
        List<Highlighter> Highlighters { get; set; }
        Configuration Tail { get; set; }
        ConfigAppearance Appearance { get; set; }
        ConfigApplication Application { get; set; }
    }
}
