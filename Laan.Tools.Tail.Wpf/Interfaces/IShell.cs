using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Laan.Tools.Tail
{
    public interface IShell 
    {
        void ApplyConfiguration();
        ObservableCollection<string> FileNames { get; set; }
        int ActiveTabIndex { get; set; }
        void BringLastTabToFront();
    }
}