using System.Threading.Tasks;
using TrueStage.Core.Interfaces;
using TrueStage.Core.Models;

namespace TrueStage.Engine;

/// <summary>No-op publisher for local dev and testing.</summary>
public class NoOpEventPublisher : IEventPublisher
{
    public Task PublishIngestionStartedAsync(IngestionContext context) => Task.CompletedTask;
    public Task PublishIngestionCompletedAsync(IngestionContext context) => Task.CompletedTask;
    public Task PublishIngestionFailedAsync(IngestionContext context, string error) => Task.CompletedTask;
}
