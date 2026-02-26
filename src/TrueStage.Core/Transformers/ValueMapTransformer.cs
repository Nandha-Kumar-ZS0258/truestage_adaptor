using System.Collections.Generic;
using TrueStage.Core.Interfaces;

namespace TrueStage.Core.Transformers;

public class ValueMapTransformer : ITransformer
{
    public string TransformName => "value_map";

    public object? Apply(string? value, string transformArgs)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;
        // transformArgs format: "A->ACTIVE,I->INACTIVE,C->CLOSED"
        var map = new Dictionary<string, string>();
        foreach (var pair in transformArgs.Split(','))
        {
            var parts = pair.Trim().Split("->");
            if (parts.Length == 2)
                map[parts[0].Trim()] = parts[1].Trim();
        }
        return map.TryGetValue(value.Trim(), out var mapped) ? mapped : value;
    }
}
