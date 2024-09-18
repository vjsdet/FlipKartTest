using System.Collections.Generic;

namespace FrameworkSupport
{
    public static class DictionaryExtensions
    {
        public static void SafeAdd<Tkey, TValue>(this Dictionary<Tkey, int> dict, Tkey key)
        {
            if (dict.ContainsKey(key))
            {
                dict[key]++;
            }
            else
            {
                dict.Add(key, 1);
            }
        }

        public static TValue SafeGet<Tkey, TValue>(this Dictionary<Tkey, TValue> dict, Tkey key)
        {
            return key != null && dict.ContainsKey(key) ? dict[key] : default(TValue);
        }

        public static TValue SafeGet<Tkey, TValue>(this Dictionary<Tkey, TValue> dict, Tkey key, TValue defaultValue)
        {
            return key != null && dict.ContainsKey(key) ? dict[key] : defaultValue;
        }

        public static void SafeAdd<Tkey, TValue>(this Dictionary<Tkey, decimal> dict, Tkey key, decimal amount)
        {
            if (dict.ContainsKey(key))
            {
                dict[key] += amount;
            }
            else
            {
                dict.Add(key, amount);
            }
        }

        public static void SafeAdd<Tkey, TValue>(this Dictionary<Tkey, string> dict, Tkey key, string value)
        {
            if (dict.ContainsKey(key))
            {
                dict[key] = value;
            }
            else
            {
                dict.Add(key, value);
            }
        }
    }
}