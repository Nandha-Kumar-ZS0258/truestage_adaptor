using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using TrueStage.Core.Interfaces;
using TrueStage.Core.Models;

namespace TrueStage.Core.Readers;

public class CsvSourceReader : ISourceReader
{
    public IEnumerable<Dictionary<string, string>> ReadRows(Stream stream, CuMappingConfig config)
    {
        using var reader = new StreamReader(stream, leaveOpen: true);
        var csvConfig = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = config.FileDelimiter,
            HasHeaderRecord = config.HasHeader,
            MissingFieldFound = null,
            BadDataFound = null
        };
        using var csv = new CsvReader(reader, csvConfig);

        if (config.HasHeader)
        {
            csv.Read();
            csv.ReadHeader();
        }

        while (csv.Read())
        {
            var row = new Dictionary<string, string>();
            if (csv.HeaderRecord != null)
            {
                foreach (var header in csv.HeaderRecord)
                {
                    row[header] = csv.GetField(header) ?? string.Empty;
                }
            }
            yield return row;
        }
    }
}
