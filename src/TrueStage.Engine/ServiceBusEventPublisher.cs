using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using TrueStage.Core.Interfaces;
using TrueStage.Core.Models;

namespace TrueStage.Engine;

public class ServiceBusEventPublisher : IEventPublisher
{
    private readonly ServiceBusClient _client;

    public ServiceBusEventPublisher(string connectionString)
    {
        _client = new ServiceBusClient(connectionString);
    }

    public async Task PublishIngestionStartedAsync(IngestionContext context)
        => await PublishAsync("ingestion-started", context);

    public async Task PublishIngestionCompletedAsync(IngestionContext context)
        => await PublishAsync("ingestion-completed", context);

    public async Task PublishIngestionFailedAsync(IngestionContext context, string error)
        => await PublishAsync("ingestion-failed", new { context.JobId, context.CuId, Error = error });

    private async Task PublishAsync(string topic, object payload)
    {
        await using var sender = _client.CreateSender(topic);
        var json = JsonSerializer.Serialize(payload);
        await sender.SendMessageAsync(new ServiceBusMessage(json));
    }
}
