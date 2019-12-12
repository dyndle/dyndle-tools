using System;
using System.Collections.Generic;

namespace Dyndle.Tools.Core.Utils
{
    public static class Extensions
    {
     
        // Selectively skip some elements from the input sequence based on their key uniqueness.
        // If several elements share the same key value, skip all but the 1-st one.
        public static IEnumerable<tSource> UniqueBy<tSource, tKey>(this IEnumerable<tSource> src, Func<tSource, tKey> keySelecta)
        {
            HashSet<tKey> res = new HashSet<tKey>();
            foreach (tSource e in src)
            {
                tKey k = keySelecta(e);
                if (res.Contains(k))
                    continue;
                res.Add(k);
                yield return e;
            }
        }
    }
}
