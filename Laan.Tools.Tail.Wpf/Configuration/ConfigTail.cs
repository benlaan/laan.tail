using System;
using System.Collections.Generic;

namespace Laan.Tools.Tail.Win
{
    public class ConfigTail
    {
        public ConfigTail()
        {
            Length = 1000;
            Width = 80;
            AutoFollow = true;
            TabsIncludePath = false;
            RightTrim = 20;
            BufferDelay = 1000;
        }

        public double BufferDelay { get; set; }
        public bool AutoFollow { get; set; }
        public int Length { get; set; }
        public int Width { get; set; }
        public bool TabsIncludePath { get; set; }
        public int RightTrim { get; set; }
    }
}
