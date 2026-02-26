using TrueStage.Core.Interfaces;

namespace TrueStage.Core.Transformers;

public class TrimTransformer : ITransformer
{
    public string TransformName => "trim";
    public object? Apply(string? value, string transformArgs) => value?.Trim();
}
