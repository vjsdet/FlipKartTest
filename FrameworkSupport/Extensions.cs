using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace FrameworkSupport
{
    public static class Extensions
    {
        private static Random random = new Random();
        private const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        public static string FullExceptionMessage(this Exception ex)
        {
            return ex.InnerException == null ? ex.Message : $"\"{ex.Message}\" -> \"{FullExceptionMessage(ex.InnerException)}\"";
        }

        public static IEnumerable<T> SelfOrNull<T>(this IEnumerable<T> items)
        {
            return items.Count() == 0 ? null : items;
        }

        public static List<List<int>> GetContiguousSubsets(this List<int> me)
        {
            List<List<int>> result = new List<List<int>>();
            List<int> numbers = me.OrderBy(x => x).ToList();
            for (int i = 1, start = 0; i <= numbers.Count; ++i)
            {
                if (i != numbers.Count && numbers[i] == numbers[i - 1] + 1)
                {
                    continue;
                }

                result.Add(numbers.GetRange(start, i - start).ToList());
                start = i;
            }

            return result;
        }

        /// <summary>
        /// Extension for 'Object' that copies the properties to a destination object.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        /// <remarks>Stolen from: http://stackoverflow.com/questions/930433/apply-properties-values-from-one-object-to-another-of-the-same-type-automaticall </remarks>
        public static void CopyProperties(this object source, object destination)
        {
            // If any this null throw an exception
            if (source == null || destination == null)
            {
                throw new Exception("Source or/and Destination Objects are null");
            }
            // Getting the Types of the objects
            Type typeDest = destination.GetType();
            Type typeSrc = source.GetType();
            // Collect all the valid properties to map
            var results = from srcProp in typeSrc.GetProperties()
                          let targetProperty = typeDest.GetProperty(srcProp.Name)
                          where srcProp.CanRead
                          && targetProperty != null
                          && targetProperty.GetSetMethod(true) != null && !targetProperty.GetSetMethod(true).IsPrivate
                          && (targetProperty.GetSetMethod().Attributes & MethodAttributes.Static) == 0
                          && targetProperty.PropertyType.IsAssignableFrom(srcProp.PropertyType)
                          select new { sourceProperty = srcProp, targetProperty = targetProperty };
            //map the properties
            foreach (var props in results)
            {
                props.targetProperty.SetValue(destination, props.sourceProperty.GetValue(source, null), null);
            }
        }

        public static string AddLeadingZeros(this string s, int minLength)
        {
            if (s.Length == minLength)
            {
                return s;
            }

            if (s.Length < minLength)
            {
                while (s.Length % minLength != 0)
                {
                    s = "0" + s;
                }
            }

            return s;
        }

        public static string RandomString(int length)
        {
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Get leftmost characters from a string
        /// </summary>
        /// <param name="str">The string to use</param>
        /// <param name="numberOfChars">The number of characters to return</param>
        /// <returns>The substring or null</returns>
        public static string Left(this string str, int numberOfChars)
        {
            return str?.Substring(0, Math.Min(str.Length, numberOfChars));
        }

        /// <summary>
        /// Get a subphrase of the passed phrase, with no words broken
        /// </summary>
        /// <param name="str">The phrase</param>
        /// <param name="numberOfChars">The number of chars max to return</param>
        /// <returns>The subphrase</returns>
        public static string BreakStringOnSpace(this string str, int numberOfChars)
        {
            return Regex.Split(str ?? "", " ").ToList()
                .Select(w => new { Continue = true, Current = w })
                .Aggregate((memo, w) =>
                {
                    if (!memo.Continue)
                    {
                        return memo;
                    }
                    else
                    {
                        string term = $"{memo.Current} {w.Current}".Trim();
                        bool ok = term.Length <= numberOfChars;

                        return memo.Current.Length <= numberOfChars
                            ? (new { Continue = ok, Current = ok ? term : memo.Current })
                            : (new { Continue = false, Current = memo.Current });
                    }
                })
                .Current
                .Left(numberOfChars);
        }

        /// <summary>
        /// Remove all the dashes in a string. Ex. a phone number 555-123-4567 would return
        /// 5551234567
        /// </summary>
        /// <param name="phoneNumber">The string that contains dashes</param>
        /// <returns></returns>
        public static string RemoveDashes(this string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return string.Empty;
            }

            string result = Regex.Replace(phoneNumber, @"[^0-9]", "");
            return result;
        }

        //stolen from top answer
        //https://stackoverflow.com/questions/388708/ascending-descending-in-linq-can-one-change-the-order-via-parameter
        public static IOrderedQueryable<TSource> OrderByWithDirection<TSource, TKey>(
            this IQueryable<TSource> source,
            Expression<Func<TSource, TKey>> keySelector,
            SortOrder sortOrder)
        {
            return sortOrder switch
            {
                SortOrder.Descending => source.OrderByDescending(keySelector),
                _ => source.OrderBy(keySelector),
            };
        }

        public static IOrderedQueryable<TSource> ThenByWithDirection<TSource, TKey>(
            this IOrderedQueryable<TSource> source,
            Expression<Func<TSource, TKey>> keySelector,
            System.Data.SqlClient.SortOrder sortOrder)
        {
            return sortOrder switch
            {
                SortOrder.Descending => source.ThenByDescending(keySelector),
                _ => source.ThenBy(keySelector),
            };
        }

        public static string GetTimestamp(this DateTime utcNow)
        {
            long timeStamp = ((long)utcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds) / 1000;
            return timeStamp.ToString();
        }

        public static bool IsValidEmailAddress(this string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            if (email.Last().Equals('.'))
            {
                return false;
            }

            /*
             * This Regex filters out certain valid email addresses (such as test@example, which doesn't have the TLD).
             * This is probably fine, but does mean that the frontend email validation (that allows test@example) doesn't
             * match the backend email validation (which doesn't allow the above address).
             */
            return Regex.IsMatch(email,
              @"[a-zA-Z0-9\+\.\-_]+@[a-zA-Z0-9\+\.\-_]+\.[a-zA-Z0-9\+\.\-_]+",
              RegexOptions.IgnoreCase);
        }

        public static string PrettySerializeObject<T>(this T value)
        {
            StringBuilder sb = new StringBuilder(256);
            StringWriter sw = new StringWriter(sb, CultureInfo.InvariantCulture);

            JsonSerializerSettings set = new JsonSerializerSettings();
            set.Converters.Add(new StringEnumConverter());

            var jsonSerializer = JsonSerializer.Create(set);
            using (JsonTextWriter jsonWriter = new JsonTextWriter(sw))
            {
                jsonWriter.Formatting = Formatting.Indented;
                jsonWriter.IndentChar = ' ';
                jsonWriter.Indentation = 4;

                jsonSerializer.Serialize(jsonWriter, value, typeof(T));
            }

            return sw.ToString();
        }

        /// <summary>
        /// Break a list of items into chunks of a specific size
        /// </summary>
        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunksize)
        {
            while (source.Any() && chunksize > 0)
            {
                yield return source.Take(chunksize);
                source = source.Skip(chunksize);
            }
        }

        /// <summary>
        /// Alternative to ForEach which yields the object and an index in a tuple
        /// </summary>
        /// <typeparam name="T">A type of the things</typeparam>
        /// <param name="items">The items</param>
        /// <param name="action">The action for each item</param>
        /// <remarks>
        /// The action will be sent a tuple with elements "item" and "index".  Item is the object of type T, and index is 1 based, so it'll go from 1..x.
        /// </remarks>
        public static void ForEachWithIndex<T>(this IList<T> items, Action<(T item, int index)> action)
        {
            int len = items.Count();

            for (int i = 0; i < len; i++)
            {
                action((items[i], i + 1));
            }
        }

        /// <summary>
        /// Does the given date fall between the passed in dates
        /// </summary>
        /// <param name="date">The date in question</param>
        /// <param name="startDate">The start period</param>
        /// <param name="endDate">The end period</param>
        /// <returns>True if the date is between them or false if not</returns>
        public static bool FallsBetweenDates(this DateTime date, DateTime startDate, DateTime endDate)
        {
            return date >= startDate && date <= endDate;
        }

        public static void AddIfNotNull<T>(this ICollection<T> list, T item)
        {
            if (item != null)
            {
                list.Add(item);
            }
        }

        public static void AddIfNotEmpty(this MultipartFormDataContent message, string content, string key)
        {
            if (!string.IsNullOrEmpty(content))
            {
                message.Add(content, key);
            }
        }

        public static void Add(this MultipartFormDataContent message, string content, string key)
        {
            message.Add(new StringContent(content), key);
        }

        /// <summary>
        /// Grab an object from an arbitrary path in a dynamic object
        /// </summary>
        /// <param name="source">The dynamic object to search, must implement IDictionary (e.g. ExpandoObject)</param>
        /// <param name="path">The path to the object to grab</param>
        /// <returns>The object</returns>
        /// <remarks>
        /// Use this if you have JSON deserialized into a dynamic variable but someone decided that using hyphens in a name
        /// would be a good idea (e.g."event-id")
        /// </remarks>
        public static object GrabFromObjectGraph(dynamic source, params string[] path)
        {
            if (source is not IDictionary<string, object> node)
            {
                return source != null ? throw new InvalidOperationException($"Passed object does not implement IDictionary") : null;
            }

            return path.Length == 1 ?
                node[path[0]]
                :
                GrabFromObjectGraph(node[path.ToList().First()], path.ToList().Skip(1).ToArray());
        }

        /// <summary>
        /// Converts Unix Epoch time into C# DateTime in UTC
        /// </summary>
        /// <param name="unixDate">The elapsed seconds (not milliseconds)</param>
        /// <returns>The UTC datetime</returns>
        public static DateTime UnixToDatetime(this long unixDate)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixDate).UtcDateTime;
        }

        /// <summary>
        /// Given a phone number string parse out the country, area code, and phone number
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <returns>country, area code, and phone number</returns>
        public static (string, string, string) ParseOutPhoneNumber(this string phoneNumber)
        {
            const string USACountryCode = "1";
            string CountryCode = "";
            string AreaCode = "";
            string Number = "";

            phoneNumber = phoneNumber.RemoveDashes();
            if (phoneNumber.Length == 10)
            {
                //123-456-7890
                CountryCode = USACountryCode;
                AreaCode = phoneNumber.SafeSubstring(0, 3);
                Number = phoneNumber.SafeSubstring(3, 7);
            }
            else if (phoneNumber.Length == 11)
            {
                CountryCode = phoneNumber.SafeSubstring(0, 1);
                AreaCode = phoneNumber.SafeSubstring(1, 3);
                Number = phoneNumber.SafeSubstring(4, 7);
            }

            return (CountryCode, AreaCode, Number);
        }

        /// <summary>
        /// Given the class and the property name, return the json property attribute or null
        /// </summary>
        /// <param name="classType"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static string GetJsonPropertyName(Type classType, string propertyName)
        {
            return classType is null
                ? null
                : (classType.GetProperty(propertyName)
                ?.GetCustomAttribute<JsonPropertyAttribute>()
                ?.PropertyName);
        }

        /// <summary>
        /// In seemingly random scenarios, the month abbreviation collection can have periods at the end. This removes them.
        /// </summary>
        /// <param name="ci">The CultureInfo object to update</param>
        private static void RemoveTrailingPeriodsFromDayAndMonthAbbreviations(this DateTimeFormatInfo cfi)
        {
            // Assume testing just the first one is good enough
            if (cfi.AbbreviatedMonthNames.First().EndsWith("."))
            {
                string[] fixedAmns = cfi.AbbreviatedMonthNames
                    .Select(c => c.EndsWith(".") ? c.Substring(0, c.Length - 1) : c)
                    .ToArray();

                cfi.AbbreviatedMonthNames = fixedAmns;

                // Fix these... other ones too... because reasons.
                cfi.AbbreviatedMonthGenitiveNames = fixedAmns;
            }

            if (cfi.AbbreviatedDayNames.First().EndsWith("."))
            {
                string[] fixedAmns = cfi.AbbreviatedDayNames
                    .Select(c => c.EndsWith(".") ? c.Substring(0, c.Length - 1) : c)
                    .ToArray();

                cfi.AbbreviatedDayNames = fixedAmns;
            }
        }

        /// <summary>
        /// Will return Full date/time pattern (short time) minus the weekday
        /// See site for the standard types: https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings
        /// </summary>
        /// <param name="dateObj"></param>
        /// <param name="cultureInfo"></param>
        /// <returns>Formatted DateTime string - Sample in en-US (August 23, 2029 7:00 PM)</returns>
        public static string ToFullDateShortTimeStringNoWeekday(this DateTime dateObj, CultureInfo cultureInfo)
        {
            DateTimeFormatInfo dtfi = cultureInfo.DateTimeFormat;

            // We don't want to change the LongDatePattern, so we need to save it.
            string originalLongDatePattern = dtfi.LongDatePattern;

            // Modify long date pattern.
            string newLongDatepattern = Regex.Replace(dtfi.LongDatePattern, @",?\s*dddd,?", String.Empty).Trim();

            dtfi.LongDatePattern = newLongDatepattern;
            dtfi.RemoveTrailingPeriodsFromDayAndMonthAbbreviations();

            string dateOutput = dateObj.ToString("f", dtfi);
            dtfi.LongDatePattern = originalLongDatePattern;

            return dateOutput;
        }

        /// <summary>
        /// Will return ToLongDate but with no weekday details but with the short month name
        /// See site for the standard types: https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings
        /// </summary>
        /// <param name="dateObj"></param>
        /// <param name="cultureInfo"></param>
        /// <returns>Formatted Date string - Sample in en-US (Aug 23, 2029)</returns>
        public static string ToShortDateStringWithShortMonth(this DateTime dateObj, CultureInfo cultureInfo)
        {
            DateTimeFormatInfo dtfi = cultureInfo.DateTimeFormat;

            // We don't want to change the LongDatePattern, so we need to save it.
            string originalLongDatePattern = dtfi.LongDatePattern;

            // Modify long date pattern.
            string newLongDatepattern = Regex.Replace(dtfi.LongDatePattern, @",?\s*dddd,?", String.Empty).Trim(); // Removes the weekday
            newLongDatepattern = Regex.Replace(newLongDatepattern, "MMMM", "MMM").Trim(); // Change the long month to the small month
            dtfi.LongDatePattern = newLongDatepattern;
            dtfi.RemoveTrailingPeriodsFromDayAndMonthAbbreviations();

            string dateOutput = dateObj.ToString(newLongDatepattern, dtfi);

            dtfi.LongDatePattern = originalLongDatePattern;

            return dateOutput;
        }

        /// <summary>
        /// Will return Full date/time pattern (short time) with a short the weekday
        /// See site for the standard types: https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings
        /// </summary>
        /// <param name="dateObj"></param>
        /// <param name="cultureInfo"></param>
        /// <returns>Formatted DateTime string - Sample in en-US (Wed Aug 23, 2029 7:00 PM)</returns>
        public static string ToFullDateWithShortMonthAndShortWeekday(this DateTime dateObj, CultureInfo cultureInfo)
        {
            DateTimeFormatInfo dtfi = cultureInfo.DateTimeFormat;

            // We don't want to change the LongDatePattern, so we need to save it.
            string originalLongDatePattern = dtfi.LongDatePattern;

            // Modify long date pattern
            string newLongDatepattern = Regex.Replace(dtfi.LongDatePattern, "dddd", "ddd").Trim(); // Change the long weekday name to the small weekday name
            newLongDatepattern = Regex.Replace(newLongDatepattern, "MMMM", "MMM").Trim(); // Change the long month to the small month

            dtfi.LongDatePattern = newLongDatepattern;
            dtfi.RemoveTrailingPeriodsFromDayAndMonthAbbreviations();

            string dateOutput = dateObj.ToString("f", dtfi);

            dtfi.LongDatePattern = originalLongDatePattern;

            return dateOutput;
        }

        /// <summary>
        /// Given a decimal value, returns the number of digits after the decimal it has
        /// </summary>
        /// <param name="myDecimal">The decimal value to check</param>
        /// <returns>The number of digits after the decimal</returns>
        public static int GetDecimalCount(this decimal myDecimal)
        {
            if (myDecimal == 0)
            {
                return 0;
            }

            if (myDecimal == myDecimal * 10)
            {
                return int.MaxValue; // no decimal.Epsilon I don't use this type enough to know why... this will work
            }

            int decimalCount = 0;
            while (myDecimal != Math.Floor(myDecimal))
            {
                myDecimal = (myDecimal - Math.Floor(myDecimal)) * 10;
                decimalCount++;
            }
            return decimalCount;
        }
    }
}