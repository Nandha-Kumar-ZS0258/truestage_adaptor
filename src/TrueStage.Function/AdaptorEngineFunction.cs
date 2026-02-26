using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TrueStage.Core.Readers;
using TrueStage.Engine;

namespace TrueStage.Function;

public class AdaptorEngineFunction
{
    private readonly AdaptorEngine _engine;
    private readonly ILogger<AdaptorEngineFunction> _logger;

    public AdaptorEngineFunction(AdaptorEngine engine, ILogger<AdaptorEngineFunction> logger)
    {
        _engine = engine;
        _logger = logger;
    }

    /// <summary>
    /// Triggered by Service Bus 'ingestion-ready' topic.
    /// Downloads file from blob, runs AdaptorEngine pipeline.
    /// </summary>
    public async Task RunAsync(string cuId, Stream fileStream, string sourceFilePath)
    {
        _logger.LogInformation("AdaptorEngine processing file {File} for CU {CuId}", sourceFilePath, cuId);

        var reader = new CsvSourceReader();
        await _engine.ProcessFileAsync(cuId, fileStream, sourceFilePath, reader);
    }
}
