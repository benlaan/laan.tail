using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Laan.Tools.Tail.Win
{
    public interface ISystemSettings
    {
        IList<string> OpenFiles { get; set; }
        int ActiveTabIndex { get; set; }
        WindowState WindowState { get; set; }
        void Save();
    }
}
