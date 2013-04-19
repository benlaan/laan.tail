using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Laan.Tools.Tail.Win
{
    public interface IShell 
    {
        void ApplyConfig();
        ObservableCollection<string> FileNames { get; set; }
        int ActiveTabIndex { get; set; }
        void BringLastTabToFront();
    }
}