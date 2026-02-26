using TrueStage.Core.Interfaces;

namespace TrueStage.Core.Transformers;

public class StripPrefixTransformer : ITransformer
{
    public string TransformName => "strip_prefix";

    public object? Apply(string? value, string transformArgs)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;
        var prefix = transformArgs;
        return value.Trim().TrimStart(prefix.ToCharArray());
    }
}
