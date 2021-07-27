using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UMS.Helper.helper
{
    public static class TypeExtension
    {
        public static bool IsNullable(this Type type)
        {
            type = type ?? throw new ArgumentNullException(nameof(type));
            return type.IsGenericType
                && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
        }

        public static bool IsNullAssignable(this Type type)
        {
            type = type ?? throw new ArgumentNullException(nameof(type));
            return type.IsNullable() || !type.IsValueType;
        }

        public static object ChangeType(this Type type, object instance)
        {
            type = type ?? throw new ArgumentNullException(nameof(type));
            if (instance == null)
            {
                if (!type.IsNullAssignable())
                {
                    throw new InvalidCastException($"{type.FullName} is not null-assignable");
                }
                return null;
            }
            if (type.IsNullable())
            {
                type = Nullable.GetUnderlyingType(type);
            }
            return Convert.ChangeType(instance, type);
        }
    }
}
