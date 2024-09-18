using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FrameworkSupport
{
    public static class StringExtensions
    {
        public static string UrlSafe(this String str)
        {
            string link = str.ToLower();

            link = Regex.Replace(link, @"[^a-z0-9\s-]", "");
            // convert multiple spaces/hyphens into one space
            link = Regex.Replace(link, @"[\s-]+", " ").Trim();
            // hyphens
            link = Regex.Replace(link, @"\s", "-");

            return link;
        }

        public static string FileNameSafe(this String str)
        {
            string fileName = str.ToLower();

            fileName = Regex.Replace(fileName, @"[^a-z0-9\s-.]", "");
            // convert multiple spaces/hyphens into one space
            fileName = Regex.Replace(fileName, @"[\s-]+", " ").Trim();
            // hyphens
            fileName = Regex.Replace(fileName, @"\s", "-");

            return fileName;
        }

        public static string CsvSafe(this string str)
        {
            if (!string.IsNullOrWhiteSpace(str))
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("\"");
                foreach (char nextChar in str)
                {
                    sb.Append(nextChar);
                    if (nextChar == '"')
                    {
                        sb.Append("\"");
                    }
                }
                sb.Append("\"");
                return sb.ToString();
            }

            return str;
        }

        /// <summary>
        /// It's pseudo-camel-case, only
        /// lowercases the first letter.
        /// Assumes the string is already in Pascal case
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToCamelCase(this string str)
        {
            if (string.IsNullOrWhiteSpace(str) || str.Length == 0)
            {
                return str;
            }

            Char firstChar = Char.ToLowerInvariant(str[0]);

            return str.Length == 1 ? firstChar.ToString() : $"{firstChar}{str.Substring(1)}";
        }

        /// <summary>
        /// Turn a string into a CSV cell output, only quoting values if necessary
        /// </summary>
        /// <param name="str">String to output</param>
        /// <returns>The CSV cell formatted string</returns>
        public static string CsvSafeUnquoted(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return str;
            }

            bool mustQuote = str.Contains(",") || str.Contains("\"") || str.Contains("\r") || str.Contains("\n");
            bool alreadyQuoted = str[0] == '\"' && str[str.Length - 1] == '\"';
            if (mustQuote)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("\"");
                foreach (char nextChar in str)
                {
                    sb.Append(nextChar);
                    if (nextChar == '"' && !alreadyQuoted)
                    {
                        sb.Append("\"");
                    }
                }
                sb.Append("\"");
                return sb.ToString();
            }

            return str;
        }

        public static string ReplaceSpaces(this string str)
        {
            return str.Replace(' ', '_');
        }

        public static string RemoveUnderScores(this string str)
        {
            return str.Replace('_', ' ');
        }

        public static string GetCode(this String str)
        {
            return str.Split('-').Last();
        }

        public static string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            var random = new Random(Guid.NewGuid().GetHashCode());
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string SafeSubstring(this string str, int startIndex, int length)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return string.Empty;
            }

            int endIndex = length - startIndex;

            return str.Length < endIndex ? str.Substring(startIndex) : str.Substring(startIndex, length);
        }

        /// <summary>
        /// Get rightmost characters from a string
        /// </summary>
        /// <param name="str">The string to use</param>
        /// <param name="numberOfChars">The number of characters to return</param>
        /// <returns>The substring or null</returns>
        public static string Last(this string str, int numberOfChars)
        {
            int startIndex = Math.Max(str.Length - numberOfChars, 0);
            return str.Substring(startIndex);
        }

        public static (string, string) SafeNameParse(this string fullName)
        {
            var firstName = "";
            var lastName = "";

            if (fullName == null)
            {
                return (firstName, lastName);
            }

            var split = fullName.Split(new char[] { ' ' }, 2);
            if (split.Length == 1)
            {
                firstName = "";
                lastName = split[0];
            }
            else
            {
                firstName = split[0];
                lastName = split[1];
            }

            return (firstName, lastName);
        }

        /// <summary>
        /// Given the string base 64 encode it
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        /// <summary>
        /// Given a base 64 encoded string, get the not encoded version
        /// </summary>
        /// <param name="encodedString"></param>
        /// <returns></returns>
        public static string Base64StringDecode(string encodedString)
        {
            var bytes = Convert.FromBase64String(encodedString);

            var decodedString = Encoding.UTF8.GetString(bytes);

            return decodedString;
        }

        /// <summary>
        /// Given the world pay data, parse out the order key from the list
        /// </summary>
        /// <param name="worldPayData">The string of world pay data returned from world pay in the URL address bar</param>
        /// <returns>Null or the order key from world pay</returns>
        public static string ParseWorldPayOrderKey(string worldPayData)
        {
            if (string.IsNullOrWhiteSpace(worldPayData))
            {
                return null;
            }

            try
            {
                return worldPayData?.Split('^')[2]; // Changing this to be 2 since WP changed: ex. DISNEY^TIXTRACKTESTECOM^0039a9fac8684cb397811d7d240ff650
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}