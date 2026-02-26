using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TrueStage.Core.Interfaces;
using TrueStage.Core.Models;
using TrueStage.Core.Transformers;

namespace TrueStage.Core.Mappers;

public class ColumnMapper : IColumnMapper
{
    private readonly TransformerFactory _factory;

    public ColumnMapper(TransformerFactory factory)
    {
        _factory = factory;
    }

    public MappedRow MapRow(Dictionary<string, string> sourceRow, CuMappingConfig config, int rowNumber)
    {
        var mapped = new MappedRow
        {
            RowNumber = rowNumber,
            RawData = JsonSerializer.Serialize(sourceRow)
        };

        try
        {
            foreach (var mapping in config.Mappings)
            {
                string? rawValue = null;

                if (!string.IsNullOrEmpty(mapping.SourceCol))
                {
                    sourceRow.TryGetValue(mapping.SourceCol, out rawValue);
                    if (string.IsNullOrWhiteSpace(rawValue) && !string.IsNullOrEmpty(mapping.DefaultValue))
                        rawValue = mapping.DefaultValue;
                }
                else
                {
                    rawValue = mapping.DefaultValue;
                }

                if (mapping.Required && string.IsNullOrWhiteSpace(rawValue))
                    throw new InvalidOperationException($"Required column '{mapping.TargetCol}' (source: '{mapping.SourceCol}') is null or empty");

                var transformed = _factory.ApplyTransform(rawValue, mapping.Transform);
                mapped.Data[mapping.TargetCol] = transformed;
            }

            // Compute record hash for change detection
            mapped.RecordHash = ComputeHash(mapped.Data);
        }
        catch (Exception ex)
        {
            mapped.HasError = true;
            mapped.ErrorMessage = ex.Message;
        }

        return mapped;
    }

    private static string ComputeHash(Dictionary<string, object?> data)
    {
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = false });
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(json));
        return Convert.ToHexString(bytes).ToLower();
    }
}
