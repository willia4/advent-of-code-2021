namespace CommonHelpers
{
    public static class Helpers
    {
        public static int SafeParseInt(string s, int defaultValue = 0)
        {
            return int.TryParse(s, out var i) ? i : defaultValue;
        }
    }
}
