using System.Collections.Generic;

namespace CommonHelpers
{
    public static class StringExtensions
    {
        public static string[] Lines(this string s)
        {
            return s.Split("\n");
        }

        public static IEnumerable<string> Strings(this string s)
        {
            return s.Select((_, i) => s.Substring(i, 1));
        }
    }
}