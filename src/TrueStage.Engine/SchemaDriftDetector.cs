using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using TrueStage.Core.Models;

namespace TrueStage.Engine;

public class SchemaDriftDetector
{
    public DriftResult Detect(Stream fileStream, CuMappingConfig config)
    {
        if (!config.SourceType.Equals("CSV", StringComparison.OrdinalIgnoreCase))
            return new DriftResult(DriftType.None, new List<string>());

        var expectedColumns = config.Mappings
            .Where(m => !string.IsNullOrEmpty(m.SourceCol))
            .Select(m => m.SourceCol!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        using var reader = new StreamReader(fileStream, leaveOpen: true);
        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true };
        using var csv = new CsvReader(reader, csvConfig);
        csv.Read();
        csv.ReadHeader();
        var actualColumns = csv.HeaderRecord?.ToHashSet(StringComparer.OrdinalIgnoreCase) ?? new HashSet<string>();

        var missing = expectedColumns.Except(actualColumns).ToList();
        var added = actualColumns.Except(expectedColumns).ToList();

        if (missing.Count > 0)
            return new DriftResult(DriftType.Hard, missing, $"Missing expected columns: {string.Join(", ", missing)}");

        if (added.Count > 0)
            return new DriftResult(DriftType.Soft, added, $"New columns found (will be ignored): {string.Join(", ", added)}");

        return new DriftResult(DriftType.None, new List<string>());
    }
}

public enum DriftType { None, Soft, Hard }

public record DriftResult(DriftType Type, List<string> AffectedColumns, string? Message = null);
