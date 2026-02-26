namespace TrueStage.Core.Interfaces;

public interface ITransformer
{
    string TransformName { get; }
    object? Apply(string? value, string transformArgs);
}
