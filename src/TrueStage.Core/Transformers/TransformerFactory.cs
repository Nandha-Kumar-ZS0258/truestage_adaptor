using System;
using System.Collections.Generic;
using TrueStage.Core.Interfaces;

namespace TrueStage.Core.Transformers;

public class TransformerFactory
{
    private readonly Dictionary<string, ITransformer> _transformers;

    public TransformerFactory()
    {
        _transformers = new Dictionary<string, ITransformer>(StringComparer.OrdinalIgnoreCase)
        {
            ["trim"] = new TrimTransformer(),
            ["date"] = new DateTransformer(),
            ["to_decimal"] = new DecimalTransformer(),
            ["strip_prefix"] = new StripPrefixTransformer(),
            ["value_map"] = new ValueMapTransformer(),
            ["default"] = new DefaultValueTransformer(),
        };
    }

    public object? ApplyTransform(string? value, string transformExpression)
    {
        if (string.IsNullOrWhiteSpace(transformExpression) || transformExpression.Equals("none", StringComparison.OrdinalIgnoreCase))
            return value;

        // Support chained transforms: "trim, strip_prefix:$, to_decimal"
        object? current = value;
        foreach (var part in transformExpression.Split(','))
        {
            var trimmedPart = part.Trim();
            var colonIdx = trimmedPart.IndexOf(':');
            string name, args;
            if (colonIdx >= 0)
            {
                name = trimmedPart[..colonIdx].Trim();
                args = trimmedPart[(colonIdx + 1)..].Trim();
            }
            else
            {
                name = trimmedPart;
                args = string.Empty;
            }

            if (_transformers.TryGetValue(name, out var transformer))
                current = transformer.Apply(current?.ToString(), args);
        }
        return current;
    }
}
