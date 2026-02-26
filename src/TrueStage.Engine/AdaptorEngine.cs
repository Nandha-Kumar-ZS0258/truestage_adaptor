using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TrueStage.Core.Interfaces;
using TrueStage.Core.Models;

namespace TrueStage.Engine;

public class AdaptorEngine
{
    private readonly IConfigLoader _configLoader;
    private readonly IColumnMapper _columnMapper;
    private readonly ITargetWriter _targetWriter;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<AdaptorEngine> _logger;

    public AdaptorEngine(
        IConfigLoader configLoader,
        IColumnMapper columnMapper,
        ITargetWriter targetWriter,
        IEventPublisher eventPublisher,
        ILogger<AdaptorEngine> logger)
    {
        _configLoader = configLoader;
        _columnMapper = columnMapper;
        _targetWriter = targetWriter;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task ProcessFileAsync(string cuId, Stream fileStream, string sourceFilePath, ISourceReader reader)
    {
        var config = await _configLoader.LoadConfigAsync(cuId);
        var context = new IngestionContext
        {
            CuId = cuId,
            SourceFilePath = sourceFilePath,
            SourceFileName = Path.GetFileName(sourceFilePath),
            Config = config
        };

        _logger.LogInformation("Starting ingestion job {JobId} for CU {CuId}, file {File}",
            context.JobId, cuId, context.SourceFileName);

        await _targetWriter.CreateIngestionJobAsync(context);
        await _eventPublisher.PublishIngestionStartedAsync(context);

        try
        {
            var sourceRows = reader.ReadRows(fileStream, config).ToList();
            context.TotalRows = sourceRows.Count;

            var mappedRows = new List<MappedRow>();
            var errorRows = new List<MappedRow>();

            int rowNum = 0;
            foreach (var sourceRow in sourceRows)
            {
                rowNum++;
                var mappedRow = _columnMapper.MapRow(sourceRow, config, rowNum);

                if (mappedRow.HasError)
                {
                    errorRows.Add(mappedRow);
                    context.FailedRows++;
                    _logger.LogWarning("Row {Row} failed for CU {CuId}: {Error}", rowNum, cuId, mappedRow.ErrorMessage);
                }
                else
                {
                    mappedRows.Add(mappedRow);
                    context.SuccessRows++;
                }
            }

            await _targetWriter.WriteRowsAsync(mappedRows, context);

            foreach (var errorRow in errorRows)
                await _targetWriter.WriteErrorAsync(errorRow, context, errorRow.ErrorMessage ?? "Unknown error");

            await _targetWriter.CloseIngestionJobAsync(context, "COMPLETED");
            await _eventPublisher.PublishIngestionCompletedAsync(context);

            _logger.LogInformation("Completed job {JobId}: {Total} rows, {Success} success, {Failed} failed",
                context.JobId, context.TotalRows, context.SuccessRows, context.FailedRows);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in job {JobId} for CU {CuId}", context.JobId, cuId);
            await _targetWriter.CloseIngestionJobAsync(context, "FAILED");
            await _eventPublisher.PublishIngestionFailedAsync(context, ex.Message);
            throw;
        }
    }
}
