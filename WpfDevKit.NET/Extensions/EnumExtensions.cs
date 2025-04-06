using System;
using System.ComponentModel;

namespace WpfDevKit.Extensions
{
    /// <summary>
    /// Provides extension methods for working with enumerations.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Converts an object value to an enumeration value of type <typeparamref name="T"/>.
        /// Supports conversion from enum name or <see cref="DescriptionAttribute"/> value.
        /// </summary>
        /// <typeparam name="T">The target enumeration type.</typeparam>
        /// <param name="value">The value to convert.</param>
        /// <param name="returnDefault">
        /// If <c>true</c>, returns the default value of <typeparamref name="T"/> when the conversion fails;
        /// otherwise, throws an <see cref="ApplicationException"/>.
        /// </param>
        /// <returns>The converted enumeration value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        /// <exception cref="ApplicationException">
        /// Thrown when the value cannot be converted and <paramref name="returnDefault"/> is <c>false</c>.
        /// </exception>
        public static T ToEnum<T>(this object value, bool returnDefault = true)
            where T : struct, IComparable, IConvertible, IFormattable
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var enumType = typeof(T);
            if (!enumType.IsEnum)
                throw new ArgumentException($"Type '{enumType.FullName}' is not an enum.", nameof(T));

            var stringValue = value.ToString();
            if (string.IsNullOrWhiteSpace(stringValue))
            {
                if (returnDefault)
                    return default;
                throw new ApplicationException("Value is null or empty.");
            }

            foreach (var field in enumType.GetFields())
            {
                if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attr)
                {
                    if (attr.Description.Equals(stringValue, StringComparison.OrdinalIgnoreCase))
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name.Equals(stringValue, StringComparison.OrdinalIgnoreCase))
                        return (T)field.GetValue(null);
                }
            }

            if (returnDefault)
                return default;

            var ex = new ApplicationException($"Value '{stringValue}' not found in enum type '{enumType.FullName}'.");
            ex.Data["Value"] = value;
            ex.Data["Type"] = enumType.FullName;
            throw ex;
        }
    }
}
