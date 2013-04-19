using System;
using System.Collections.Generic;
using System.Linq;

namespace Laan.Tools.Tail
{
    public static class ComparableOfTExtensions
    {
        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0)
                return min;

            if (val.CompareTo(max) > 0)
                return max;
            
            return val;
        }
    }
}
