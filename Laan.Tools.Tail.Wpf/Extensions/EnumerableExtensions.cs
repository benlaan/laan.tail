using System;
using System.Collections.Generic;
using System.Linq;

namespace Laan.Tools.Tail
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Tail<T>(this IEnumerable<T> collection, int last)
        {
            return collection
                .Skip(Math.Max(0, collection.Count() - last))
                .Take(last);
        }

        public static string Join<T>(this IEnumerable<T> collection, string seperator)
        {
            return String.Join(seperator, collection);
        }
    }
}