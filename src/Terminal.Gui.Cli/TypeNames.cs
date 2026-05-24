namespace Terminal.Gui.Cli;

/// <summary>Maps CLR types to stable wire-format type names.</summary>
public static class TypeNames
{
    /// <summary>Returns the wire-format name for <paramref name="type" />.</summary>
    public static string WireName (Type type)
    {
        ArgumentNullException.ThrowIfNull (type);

        Type nullableType = Nullable.GetUnderlyingType (type) ?? type;

        if (nullableType == typeof (void))
        {
            return "void";
        }

        if (nullableType == typeof (string))
        {
            return "string";
        }

        if (nullableType == typeof (bool))
        {
            return "boolean";
        }

        if (nullableType == typeof (int) || nullableType == typeof (long) || nullableType == typeof (short) ||
            nullableType == typeof (byte))
        {
            return "integer";
        }

        if (nullableType == typeof (double) || nullableType == typeof (float) || nullableType == typeof (decimal))
        {
            return "number";
        }

        if (nullableType == typeof (DateTime) || nullableType == typeof (DateTimeOffset))
        {
            return "datetime";
        }

        if (nullableType.IsEnum)
        {
            return "string";
        }

        return "object";
    }
}
