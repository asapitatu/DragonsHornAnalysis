using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public static class EnumExtensions
{
    public static IEnumerable<E> GetValues<E>()
    {
        foreach (E value in Enum.GetValues(typeof(E)))
        {
            yield return value;
        }
    }

    public static T GetAttributeOfType<T, E>(E enumVal) where T : Attribute
    {
        var type = typeof(E);
        var memInfo = type.GetMember(enumVal.ToString());
        var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
        return (attributes.Length > 0) ? (T)attributes[0] : null;
    }

    public static bool HasName<E>(E enumVal, string name)
    {
        if (enumVal.ToString().Equals(name, StringComparison.InvariantCultureIgnoreCase))
            return true;

        var nameAttribute = GetAttributeOfType<EnumValueNameAttribute, E>(enumVal);
        if (nameAttribute != null && nameAttribute.Names.Contains(name, StringComparer.InvariantCultureIgnoreCase))
            return true;

        return false;
    }

    public static E Parse<E>(string text) where E : struct
    {
        E? match = GetValues<E>().FirstOrDefault(e => HasName(e, text));
        if (!match.HasValue)
            throw new ArgumentOutOfRangeException(nameof(text), $"No enum value of type {typeof(E)} matches the input \"{text}\"");
        return match.Value;
    }
}