using System;
using System.Globalization;
using TrueStage.Core.Interfaces;

namespace TrueStage.Core.Transformers;

public class DateTransformer : ITransformer
{
    public string TransformName => "date";

    public object? Apply(string? value, string transformArgs)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var format = string.IsNullOrWhiteSpace(transformArgs) ? "MM/dd/yyyy" : transformArgs;
        if (DateTime.TryParseExact(value.Trim(), format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            return dt;
        if (DateTime.TryParse(value.Trim(), out var dt2))
            return dt2;
        throw new FormatException($"Cannot parse date '{value}' with format '{format}'");
    }
}
