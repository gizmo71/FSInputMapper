using System;
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
}
