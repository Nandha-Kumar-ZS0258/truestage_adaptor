using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TrueStage.Core.Interfaces;

namespace TrueStage.Function;

public class FileArrivedMessage
{
    public string CuId { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}

public class FileRouterFunction
{
    private readonly IConfigLoader _configLoader;
    private readonly ILogger<FileRouterFunction> _logger;

    public FileRouterFunction(IConfigLoader configLoader, ILogger<FileRouterFunction> logger)
    {
        _configLoader = configLoader;
        _logger = logger;
    }

    /// <summary>
    /// Entry point triggered by Service Bus 'file-arrived' topic.
    /// Validates CU is active, loads config, fires 'ingestion-ready' event.
    /// </summary>
    public async Task RunAsync(string messageBody)
    {
        var message = JsonSerializer.Deserialize<FileArrivedMessage>(messageBody)
            ?? throw new InvalidOperationException("Failed to deserialize FileArrivedMessage");

        _logger.LogInformation("FileRouter received file for CU {CuId}: {File}", message.CuId, message.FileName);

        var config = await _configLoader.LoadConfigAsync(message.CuId);

        _logger.LogInformation("Loaded config v{Version} for CU {CuId}, target table: {Table}",
            config.AdapterVersion, message.CuId, config.TargetTable);

        // TODO: check CU_Registry status, fire ingestion-ready event to Service Bus
    }
}
