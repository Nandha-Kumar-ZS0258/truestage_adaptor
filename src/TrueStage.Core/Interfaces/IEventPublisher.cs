using System.Threading.Tasks;
using TrueStage.Core.Models;

namespace TrueStage.Core.Interfaces;

public interface IEventPublisher
{
    Task PublishIngestionStartedAsync(IngestionContext context);
    Task PublishIngestionCompletedAsync(IngestionContext context);
    Task PublishIngestionFailedAsync(IngestionContext context, string error);
}
