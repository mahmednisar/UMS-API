using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UMS.Helper.helper
{
    public static class Maper
    {
        public static void Map(ExpandoObject source, object destination)
        {
            source = source ?? throw new ArgumentNullException(nameof(source));
            destination = destination ?? throw new ArgumentNullException(nameof(destination));

            string normalizeName(string name) => name.ToLowerInvariant();

            IDictionary<string, object> dict = source;
            var type = destination.GetType();

            var setters = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite && p.GetSetMethod() != null)
                .ToDictionary(p => normalizeName(p.Name));

            foreach (var item in dict)
            {
                if (setters.TryGetValue(normalizeName(item.Key), out var setter))
                {
                    var value = setter.PropertyType.ChangeType(item.Value);
                    setter.SetValue(destination, value);
                }
            }

        }


        public static List<T> MapList<T>(List<dynamic> source)
        {
            string normalizeName(string name) => name.ToLowerInvariant();
            var obj = new List<T>();
            for (int i = 0; i < source.Count; i++)
            { 
                IDictionary<string, object> dict = source[i];
                var data = (T)Activator.CreateInstance(typeof(T));
                Type type = typeof(T);
                var setters = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanWrite && p.GetSetMethod() != null)
                    .ToDictionary(p => normalizeName(p.Name));
                foreach (var item in dict)
                {
                    if (setters.TryGetValue(normalizeName(item.Key), out var setter))
                    {
                        var value = setter.PropertyType.ChangeType(item.Value);
                        setter.SetValue(data, value);
                    }
                }
                obj.Add(data);
            }
            return obj;
        }
    }
}
