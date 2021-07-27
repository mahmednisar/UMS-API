using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using static System.String;

namespace UMS.Helper
{
    public static class ExtensionHelper
    {
    
        #region "Dictionary"
        public static void AddOrReplace(this IDictionary<string, object> dict, string key, object value)
        {
            if (dict.ContainsKey(key))
                dict[key] = value;
            else
                dict.Add(key, value);
        }

        public static dynamic GetObjectOrDefault(this IDictionary<string, object> dict, string key)
        {
            return dict.ContainsKey(key) ? dict[key] : null;
        }

        public static T GetObjectOrDefault<T>(this IDictionary<string, object> dict, string key)
        {
            return dict.ContainsKey(key) ? (T) Convert.ChangeType(dict[key], typeof(T)) : default(T);
        }
        #endregion

        #region "String"
        public static string ToSelfUrl(this string text)
        {
            if (IsNullOrWhiteSpace(text))
                return text;

            var outputStr = text.Trim().Replace(":", "").Replace("&", "").Replace(" ", "-").Replace("'", "").Replace(",", "").Replace("(", "").Replace(")", "").Replace("--", "").Replace(".", "");
            return Regex.Replace(outputStr.Trim().ToLower().Replace("--", ""), "[^a-zA-Z0-9_-]+", "", RegexOptions.Compiled);
        }

        public static string ToId(this string text)
        {
            return IsNullOrWhiteSpace(text) ? text : text.Replace(" ", "").Trim().ToLower();
        }


        public static string TrimLength(this string input, int length, bool incomplete = true)
        {
            return IsNullOrEmpty(input) ? Empty :
                input.Length > length ? Concat(input[..length], incomplete ? "..." : "") : input;
        }

        public static string ToTitle(this string input)
        {
            return IsNullOrEmpty(input) ? Empty : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLower());
        }

        public static bool ContainsAny(this string input, params string[] values)
        {
            return !IsNullOrEmpty(input) && values.Any(input.Contains);
        }
        public static string InsertBr(this string text)
        {
            return IsNullOrWhiteSpace(text) ? text : text.Replace("\n", "<br />");
        }
        public static string InsertNewLine(this string text)
        {
            return IsNullOrWhiteSpace(text) ? text : text.Replace("<br />", "\n");
        }
        #endregion

        #region "Collection"
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            var enumerable = source as T[] ?? source.ToArray();
            foreach (var element in enumerable)
            {
                action(element);
            }
        }

        public static bool IsNotNullAndNotEmpty<T>(this ICollection<T> source)
        {
            return source != null && source.Any();
        }
        #endregion

        #region "Enums"
        public static IDictionary<string, int> EnumToDictionary(this Type t)
        {
            if (t == null) throw new NullReferenceException();
            if (!t.IsEnum) throw new InvalidCastException("object is not an Enumeration");

            var names = Enum.GetNames(t);
            var values = Enum.GetValues(t);

            var array = values;
            return (Enumerable.Range(0, names.Length)
                    .Where(i => true)
                    .Select(i => new {Key = names[i], Value = (int) array.GetValue(i)}))
                        .ToDictionary(k => k.Key, k => k.Value);
        }

        public static IDictionary<string, int> EnumToDictionaryWithDescription(this Type t)
        {
            if (t == null) throw new NullReferenceException();
            if (!t.IsEnum) throw new InvalidCastException("object is not an Enumeration");

            var names = Enum.GetNames(t);
            var values = Enum.GetValues(t);

            return Enumerable.Range(0, names.Length)
                .Select(i => new
                {
                    Key = ((Enum) values.GetValue(i)).GetDescription(),
                    Value = Convert.ToInt32((Enum) values.GetValue(i))
                })
                .ToDictionary(k => k.Key, k => k.Value);

        }

        public static string GetDescription(this String value, Type t)
        {
            if (!t.IsEnum) return "";
            var fields = t.GetFields(BindingFlags.Static | BindingFlags.Public);
            var values = from f
                    in fields
                let attribute = Attribute.GetCustomAttribute(f, typeof(DisplayAttribute)) as DisplayAttribute
                where attribute != null && attribute.ShortName == value
                select f.GetValue(null);
            return values.First()?.ToString();
        }

        public static string GetDescription(this Enum value)
        {
            var fi = value.GetType().GetField(value.ToString());

            var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            return attributes.Length > 0 ? attributes[0].Description : value.ToString();
        }

        public static string GetLongDescription(this Enum value)
        {
            var fi = value.GetType().GetField(value.ToString());

            var attributes = (DisplayAttribute[])fi.GetCustomAttributes(typeof(DisplayAttribute), false);

            return attributes.Length > 0 ? attributes[0].Description : value.ToString();
        }

        public static string GetShortName(this Enum value)
        {
            var fi = value.GetType().GetField(value.ToString());

            var attributes = (DisplayAttribute[])fi.GetCustomAttributes(typeof(DisplayAttribute), false);

            return attributes.Length > 0 ? attributes[0].ShortName : value.ToString();
        }

        public static string GetShortString(this string value, int length = 150)
        {
            return value.Substring(0, Math.Min(value.Length, length));
        }

        public static string ReplaceSpecialCharString(this string value)
        {
            return Regex.Replace(value, @"\s+", "")
                               .Replace("[", Empty)
                               .Replace("]", Empty)
                               .Replace("%", Empty)
                               .Replace("*", Empty)
                               .Replace("-", Empty)
                               .Replace("_", Empty)
                               .Replace("^", Empty)
                               .Replace("'", Empty)
                               .Replace(" ", Empty);

        }
        #endregion

        #region "Decimal To String"    
        public static decimal ToDecimalUnit(this string value)
        {
            return value switch
            {
                "thousands" => 1000M,
                "lacs" => 100000M,
                "crores" => 10000000M,
                _ => 1M
            };
        }

        public static string ToStringFormat(this decimal number)
        {
            //return string.Format("{0:0.00}", number).Replace(".00", "");
            return number.ToString("0.#####");
        }
        #endregion

        #region "DateTime"
        public static DateTime ToDateTime(this string str, bool isWithTime = false)
        {
            return IsNullOrWhiteSpace(str) ? DateTime.Now : DateTime.ParseExact(str, isWithTime ? "dd-MMM-yyyy hh:mm:ss" : "dd-MMM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None);
        }

        public static string ToFormatString(this DateTime? date)
        {
            return !date.HasValue ? Empty : date.Value.ToString("dd MMM, yyyy");
        }

        public static string ToFormatString(this DateTime date)
        {
            return date.ToString("dd MMM, yyyy");
        }

        public static string ToFormatCustomString(this DateTime date)
        {
            return date.ToString("dd-MMM-yyyy");
        }

        public static string ToFormatCustomString(this DateTime date, string dateformat)
        {
            return date.ToString(dateformat);
        }

        public static string ToFormatCustomString(this DateTime? date)
        {
            return !date.HasValue ? Empty : date.Value.ToString("dd-MMM-yyyy");
        }

        public static string ToFormatCustomString(this DateTime? date, string dateformat)
        {
            return !date.HasValue ? Empty : date.Value.ToString(dateformat);
        }

        public static string ToFormatDateString(this DateTime? date, bool isIncludeTime = false, bool showUtcTime = false)
        {
            if (!date.HasValue) return Empty;
            var dateTime = GetMelbourneDateTime(date.Value, showUtcTime);

            return dateTime.ToString(isIncludeTime ? "dd-MMM-yyyy hh:mm tt" : "dd-MMM-yyyy");

        }

        public static string ToFormatDateString(this DateTime date, bool isIncludeTime = false, bool showUtcTime = false)
        {
            var dateTime = GetMelbourneDateTime(date, showUtcTime);

            return dateTime.ToString(isIncludeTime ? "dd-MMM-yyyy hh:mm tt" : "dd-MMM-yyyy");
        }

        public static DateTime GetMelbourneDateTime(DateTime dateTime, bool showUtcTime)
        {
            if (!showUtcTime)
            {
                var cstZone = TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time");
                var melbourneDateTime = TimeZoneInfo.ConvertTimeFromUtc(dateTime, cstZone);
                return melbourneDateTime;
            }
            else
            {
                return dateTime;
            }
        }

        public static string FormatDateString(this DateTime date, bool showUtcTime = false)
        {
            var dateTime = GetMelbourneDateTime(date, showUtcTime);
            return dateTime.ToString("dd-MM-yyyy");
        }

        public static string FormatDateStringWithSpace(this DateTime date, bool showUtcTime = false)
        {
            var dateTime = GetMelbourneDateTime(date, showUtcTime);
            return dateTime.ToString("dd MM yyyy");
        }

        public static string ToFormatDateStringInHours(this DateTime date, bool showUtcTime = false)
        {
            var dateTime = GetMelbourneDateTime(date, showUtcTime);
            return dateTime.ToString("hh:mm tt");
        }

        public static string ToDayFormatDateString(this DateTime date)
        {
            return date.ToLocalTime().ToString("dddd, dd MMMM yyyy");
        }

        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            var diff = dt.DayOfWeek - startOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }

            return dt.AddDays(-1 * diff).Date;
        }

        public static string ToDateTimeFileString()
        {
            return ToDateTimeFileString(new DateTime());
        }

        public static string ToDateTimeFileString(this DateTime dateTime)
        {
            try
            {
                dateTime = DateTime.Now;
                return dateTime.Ticks.ToString();
            }
            catch (Exception)
            {
                return Empty;
            }

        }

        public static DateTime? ToDateTimeParseInDayMonthYear(this string dateTime)
        {
            try
            {
                if (IsNullOrEmpty(dateTime)) return null;
                return DateTime.ParseExact(dateTime, "dd-MMM-yyyy", CultureInfo.InvariantCulture);

            }
            catch (Exception)
            {
                return null;
            }

        }

        public static DateTime ToDateTimeInDayMonthYear(this string dateTime, bool showUtcTime = false)
        {
            try
            {

                var dt = DateTime.ParseExact(dateTime, "dd-MMM-yyyy", CultureInfo.InvariantCulture);

                return GetMelbourneDateTime(dt, showUtcTime);

            }
            catch (Exception)
            {
                // ignored
            }

            return DateTime.UtcNow;

        }

        public static DateTime ToFormatDateTime(this DateTime date, bool showUtcTime = false)
        {
            var dateTime = GetMelbourneDateTime(date, showUtcTime);
            return dateTime;
        }

        public static string TimeAgo(this DateTime dateTime, bool showUtcTime = false)
        {
            var date = GetMelbourneDateTime(dateTime, showUtcTime);

            var subtractTime = GetMelbourneDateTime(DateTime.UtcNow, showUtcTime);

            string result;
            var timeSpan = subtractTime.Subtract(date);

            if (timeSpan <= TimeSpan.FromSeconds(60))
            {
                result = $"{timeSpan.Seconds} sec ago";
            }
            else if (timeSpan <= TimeSpan.FromMinutes(60))
            {
                result = timeSpan.Minutes > 1 ? $"{timeSpan.Minutes} mins ago"
                    :
                    "1 min ago";
            }
            else if (timeSpan <= TimeSpan.FromHours(24))
            {
                result = timeSpan.Hours > 1 ? $"{timeSpan.Hours} hours ago"
                    :
                    "1 hour ago";
            }
            else if (timeSpan <= TimeSpan.FromDays(30))
            {
                result = timeSpan.Days > 1 ? $"{timeSpan.Days} days ago"
                    :
                    "yesterday";
            }
            else if (timeSpan <= TimeSpan.FromDays(365))
            {
                result = timeSpan.Days > 30 ? $"{timeSpan.Days / 30} months ago"
                    :
                    "1 month ago";
            }
            else
            {
                result = timeSpan.Days > 365 ? $"{timeSpan.Days / 365} years ago"
                    :
                    "1 year ago";
            }

            return result;
        }

        public static string TimeAgoShort(this DateTime dateTime, bool showUtcTime = false)
        {
            var date = GetMelbourneDateTime(dateTime, showUtcTime);

            var subtractTime = GetMelbourneDateTime(DateTime.UtcNow, showUtcTime);

            string result;
            var timeSpan = subtractTime.Subtract(date);

            if (timeSpan <= TimeSpan.FromSeconds(60))
            {
                result = $"{timeSpan.Seconds}s";
            }
            else if (timeSpan <= TimeSpan.FromMinutes(60))
            {
                result = $"{timeSpan.Minutes}m";
            }
            else if (timeSpan <= TimeSpan.FromHours(24))
            {
                result = $"{timeSpan.Hours}h";
            }
            else if (timeSpan <= TimeSpan.FromDays(30))
            {
                result = $"{timeSpan.Days}d";
            }
            else if (timeSpan <= TimeSpan.FromDays(365))
            {
                result = $"{timeSpan.Days / 30}M";
            }
            else
            {
                result = $"{timeSpan.Days / 365}Y";
            }

            return result;
        }

        public static string FormatDateInString(this DateTime date, bool showUtcTime = false)
        {
            var dateTime = GetMelbourneDateTime(date, showUtcTime);
            return dateTime.ToString("MMM dd, h:mm tt");
        }

        public static string FormatDateInTime(this DateTime date, bool showUtcTime = false)
        {
            var dateTime = GetMelbourneDateTime(date, showUtcTime);
            return dateTime.ToString("t");
        }
        #endregion

        public static string StripHtml(this string input)
        {
            if (IsNullOrEmpty(input))
                return Empty;

            return Regex.Replace(input, "<[^>]*>", Empty);
        }

        public static string ToUnique(this string fileName)
        {
            return $"{DateTime.Now.Ticks}{Path.GetExtension(fileName.ToLower())}";
        }
    }
}