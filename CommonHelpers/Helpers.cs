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
    }
}
