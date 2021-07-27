using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace UMS.Helper
{
    public static class Helper
    {
        public static List<T> DataTableToList<T>(this DataTable dataTable) where T : new()
        {
            var dataList = new List<T>();
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            var objFieldNames = typeof(T).GetProperties(flags).Select(item => new
            {
                item.Name,
                Type = Nullable.GetUnderlyingType(item.PropertyType) ?? item.PropertyType
            }).ToList();
            var dtlFieldNames = dataTable.Columns.Cast<DataColumn>().Select(item => new
            {
                Name = item.ColumnName,
                Type = item.DataType
            }).ToList();
            foreach (var dataRow in dataTable.AsEnumerable().ToList())
            {
                var classObj = new T();
                foreach (var dtField in dtlFieldNames)
                {
                    var propertyInfos = classObj.GetType().GetProperty(dtField.Name);
                    var field = objFieldNames.Find(x => x.Name == dtField.Name);
                    if (field == null || propertyInfos is null) continue;
                    if (propertyInfos.PropertyType == typeof(DateTime))
                    {
                        propertyInfos.SetValue(classObj, ConvertToDateTime(dataRow[dtField.Name]), null);
                    }
                    else if (propertyInfos.PropertyType == typeof(DateTime?))
                    {
                        propertyInfos.SetValue(classObj, ConvertToDateTime(dataRow[dtField.Name]), null);
                    }
                    else if (propertyInfos.PropertyType == typeof(int))
                    {
                        propertyInfos.SetValue(classObj, ConvertToInt(dataRow[dtField.Name]), null);
                    }
                    else if (propertyInfos.PropertyType == typeof(long))
                    {
                        propertyInfos.SetValue(classObj, ConvertToLong(dataRow[dtField.Name]), null);
                    }
                    else if (propertyInfos.PropertyType == typeof(decimal))
                    {
                        propertyInfos.SetValue(classObj, ConvertToDecimal(dataRow[dtField.Name]), null);
                    }
                    else if (propertyInfos.PropertyType == typeof(string))
                    {
                        propertyInfos.SetValue(classObj,
                            dataRow[dtField.Name] is DateTime
                                ? ConvertToDateString(dataRow[dtField.Name])
                                : ConvertToString(dataRow[dtField.Name]), null);
                    }
                    else
                    {
                        propertyInfos.SetValue(classObj, Convert.ChangeType(dataRow[dtField.Name], propertyInfos.PropertyType), null);
                    }
                }
                dataList.Add(classObj);
            }

            return dataList;
        }

        private static string ConvertToDateString(object date)
        {
            return date == null ? string.Empty : Convert.ToDateTime(date).ConvertDate();
        }

        private static string ConvertToString(object value)
        {
            return Convert.ToString(ReturnEmptyIfNull(value));
        }

        private static int ConvertToInt(object value)
        {
            return Convert.ToInt32(ReturnZeroIfNull(value));
        }

        private static long ConvertToLong(object value)
        {
            return Convert.ToInt64(ReturnZeroIfNull(value));
        }

        private static decimal ConvertToDecimal(object value)
        {
            return Convert.ToDecimal(ReturnZeroIfNull(value));
        }

        private static DateTime ConvertToDateTime(object date)
        {
            return Convert.ToDateTime(ReturnDateTimeMinIfNull(date));
        }

        public static string ConvertDate(this DateTime dateTime, bool excludeHoursAndMinutes = false)
        {
            return dateTime != DateTime.MinValue ? dateTime.ToString(excludeHoursAndMinutes ? "yyyy-MM-dd" : "yyyy-MM-dd HH:mm:ss.fff") : null;
        }

        public static object ReturnEmptyIfNull(this object value)
        {
            return value == DBNull.Value ? string.Empty : value ?? string.Empty;
        }

        public static object ReturnZeroIfNull(this object value)
        {
            return value == DBNull.Value ? 0 : value ?? 0;
        }

        public static object ReturnDateTimeMinIfNull(this object value)
        {
            return value == DBNull.Value ? DateTime.MinValue : value ?? DateTime.MinValue;
        }
    }
}