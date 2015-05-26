using System;
using System.Collections.Generic;
using System.Linq;

namespace Laan.Tools.Tail
{
    /// <summary>
    /// Holds a list of arguments given to an application at startup.
    /// </summary>
    public class ArgumentsReceivedEventArgs : EventArgs
    {
        public String[] Args { get; set; }
    }
}
