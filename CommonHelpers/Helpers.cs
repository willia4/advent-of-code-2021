using System.Collections.Immutable;
using System.Linq;

namespace CommonHelpers
{
    public static class Helpers
    {
        public static int SafeParseInt(string s, int defaultValue = 0)
        {
            return int.TryParse(s, out var i) ? i : defaultValue;
        }

        public static int Min(params int[] values)
        {
            return values.Length == 0 ? 0 : values.Min();
        }
        
        public static int Max(params int[] values)
        {
            return values.Length == 0 ? 0 : values.Max();
        }

        public static string Trim(string s)
        {
            return s.Trim();
        }

        public static string ToUpper(string s)
        {
            return s.ToUpperInvariant();
        }

        public static bool NotEmpty(string s)
        {
            return !string.IsNullOrWhiteSpace(s);
        }

        public static bool IsEmpty(string s)
        {
            return string.IsNullOrWhiteSpace(s);
        }

        // for any keys in a dictionary, reset the values for those keys to their default value. Does not remove any keys. 
        public static void ClearDictionaryValuesInPlace<K, V>(Dictionary<K, V> dictionary, V defaultValue = default)
        {
            foreach (var k in dictionary.Keys)
            {
                dictionary[k] = defaultValue;
            }
        }

        public static void ReplaceDictionaryKeys<K, V>(Dictionary<K, V> target, Dictionary<K, V> source)
        {
            foreach (var k in source.Keys)
            {
                target[k] = source[k];
            }
        }

        public static ImmutableList<T> MakeImmutableList<T>(IEnumerable<T> source)
        {
            var r = ImmutableList<T>.Empty;
            if (source != null)
            {
                r = r.AddRange(source);
            }

            return r;
        }

        public static T Identity<T>(T t) => t;
    }
}
