using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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

        public static string UCFirst(this string input)
        {
            return input.Substring(0, 1).ToUpperInvariant() + input.Substring(1);
        }
        public static string LCFirst(this string input)
        {
            return input.Substring(0, 1).ToLowerInvariant() + input.Substring(1, input.Length - 1);
        }

        public static string GetSingular(this string input, bool forceDifferentValue = true)
        {
            if (input.EndsWith("ies"))
            {
                return input.Substring(0, input.Length - 3) + "y";
            }
            if (input.EndsWith("s"))
            {
                return input.Substring(0, input.Length - 1);
            }
            // note: when forcing a different value, the output cannot be the same as the input
            return forceDifferentValue ? input + "Item" : input;
        }


        public static string ParseId(this string uri)
        {
            if (string.IsNullOrEmpty(uri))
            {
                return uri;
            }
            TcmUri tcmUri = new TcmUri(uri);
            return Convert.ToString(tcmUri.ItemId);
        }

        public static string ToPublicationId(this string uri, string targetUri)
        {
            if (string.IsNullOrEmpty(uri) || string.IsNullOrEmpty(targetUri))
            {
                return uri;
            }
            TcmUri tcmUri = new TcmUri(uri);
            TcmUri targetTcmUri = new TcmUri(targetUri);
            return tcmUri.ToPublication(targetTcmUri).ToString();
        }

    }
}
