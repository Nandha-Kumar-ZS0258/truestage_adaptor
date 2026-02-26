using System;
using System.Globalization;
using TrueStage.Core.Interfaces;

namespace TrueStage.Core.Transformers;

public class DecimalTransformer : ITransformer
{
    public string TransformName => "to_decimal";

    public object? Apply(string? value, string transformArgs)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var cleaned = value.Trim().Replace("$", "").Replace(",", "");
        if (decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return result;
        throw new FormatException($"Cannot parse decimal '{value}'");
    }
}
