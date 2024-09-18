using System.Collections.Generic;

namespace FrameworkSupport
{
    public static class StaticHelpers
    {
        public static int? IntSafeParse(string value)
        {
            int result;

            return int.TryParse(value, out result) ? result : null;
        }

        public static int SafeParse(string val, int defaultValue)
        {
            int result;
            return int.TryParse(val, out result) ? result : defaultValue;
        }

        public static T Max<T>(T first, T second)
        {
            return Comparer<T>.Default.Compare(first, second) > 0 ? first : second;
        }
    }
}