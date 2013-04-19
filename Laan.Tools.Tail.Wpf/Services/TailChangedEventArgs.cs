using System;
using System.Collections.Generic;
using System.Linq;

namespace Laan.Tools.Tail
{
    public class TailChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the TailChangedEventArgs class.
        /// </summary>
        public TailChangedEventArgs(string fileName)
        {
            FileName = fileName;
        }

        public string FileName { get; private set; }
    }

    public delegate void TailChangedEventHandler(object sender, TailChangedEventArgs e);
}
