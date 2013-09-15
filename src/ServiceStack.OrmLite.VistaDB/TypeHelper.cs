using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServiceStack.OrmLite.VistaDB
{
    static class TypeHelper
    {
        public static bool IsNullableValueType(Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

        public static bool TypeAllowsNullValue(Type type)
        {
            return (!type.IsValueType || IsNullableValueType(type));
        }

        public static object GetDefaultValue(Type type)
        {
            return (TypeAllowsNullValue(type)) ? null : Activator.CreateInstance(type);
        }

        public static bool IsDefaultValue(object value)
        {
            return value == null || value.Equals(GetDefaultValue(value.GetType()));
        }
    }
}
