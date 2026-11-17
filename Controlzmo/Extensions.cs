using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Controlzmo
{
    public static class AttributeExtensions
    {
        public static TAttribute GetAttribute<TAttribute>(this Enum value) where TAttribute : Attribute
        {
            var enumType = value.GetType();
            var name = Enum.GetName(enumType, value);
            return enumType.GetField(name!)!.GetCustomAttributes(false).OfType<TAttribute>().Single();
        }
    }

    public static class Int32Extensions
    {
        public static Int32 Parse(this string? value, Int32 defaultValue)
        {
            return Int32.TryParse(value, NumberStyles.Integer, null, out Int32 parsed) ? parsed : defaultValue;
        }
    }

    public static class IEnumerableExtensions
    {
        public static T? SingleDistinctValueOrNull<T>(this IEnumerable<T?> enumerable) where T : struct
        {
            var allDistinctValues = enumerable.Distinct().ToArray(); // Inefficient but who cares?
            T? value = allDistinctValues.Count() == 1 ? allDistinctValues[0] : null;
            return value;
        }
    }
}
