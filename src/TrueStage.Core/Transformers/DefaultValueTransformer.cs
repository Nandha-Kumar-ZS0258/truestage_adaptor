using System;
using TrueStage.Core.Interfaces;

namespace TrueStage.Core.Transformers;

public class DefaultValueTransformer : ITransformer
{
    public string TransformName => "default";

    public object? Apply(string? value, string transformArgs)
    {
        if (!string.IsNullOrWhiteSpace(value)) return value;
        return transformArgs?.ToLower() == "utcnow" ? (object)DateTime.UtcNow : transformArgs;
    }
}
