using System.Collections.Generic;

namespace CommonHelpers
{
    public static class StringExtensions
    {
        public static string[] Lines(this string s)
        {
            return s.Split("\n");
        }
    }
}