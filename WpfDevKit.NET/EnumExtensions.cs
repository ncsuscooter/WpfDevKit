using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

/// <summary>
/// Provides extension methods for working with enumerations.
/// </summary>
[DebuggerStepThrough]
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
    /// otherwise, throws an <see cref="InvalidOperationException"/>.
    /// </param>
    /// <returns>The converted enumeration value.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is not an <see cref="Enum"/> type.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c> or an empty string (including whitespaces).</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the value cannot be converted and <paramref name="returnDefault"/> is <c>false</c>.
    /// </exception>
    public static T ToEnum<T>(this object value, bool returnDefault = true) where T : struct, IComparable, IConvertible, IFormattable
    {
        var type = typeof(T);
        if (!type.IsEnum)
            throw new ArgumentException($"Type '{type.FullName}' is not an enum.", nameof(T));

        var valueAsString = Convert.ToString(value);
        if (string.IsNullOrWhiteSpace(valueAsString))
            return returnDefault ? (T)default : throw new ArgumentNullException(nameof(value));

        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
        foreach (var f in fields)
            if ((Attribute.GetCustomAttribute(f, typeof(DescriptionAttribute)) is DescriptionAttribute attr
                && attr.Description.Equals(valueAsString, StringComparison.OrdinalIgnoreCase)) ||
                f.Name.Equals(valueAsString, StringComparison.OrdinalIgnoreCase))
                return (T)f.GetValue(null);

        if (returnDefault)
            return default;

        var ex = new InvalidOperationException("Value not found in enum type");
        ex.Data["Value"] = value;
        ex.Data["Type"] = type.FullName;
        throw ex;
    }
}
