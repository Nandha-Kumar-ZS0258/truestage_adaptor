# Skill: Generate Integration Tests for a CU

Given: CU_ID, config path, sample file path

## Generate `/tests/TrueStage.Integration.Tests/{CuId}IntegrationTest.cs`

Reference pattern: `CuAlphaIntegrationTest.cs`

The test must:
1. Load config from `/configs/{cu_id_lower}_mapping.json` via `LocalFileConfigLoader`
2. Parse sample CSV from `/samples/{cu_id_lower}_sample.csv`
3. Map all rows using `CsvSourceReader` + `ColumnMapper`
4. Assert zero errors
5. Assert specific mapped values for row 1 (member_id, first_name, account_balance, member_status)
6. Assert record_hash is 64 chars (SHA256)

## Template
```csharp
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using TrueStage.ConfigLoader;
using TrueStage.Core.Mappers;
using TrueStage.Core.Readers;
using TrueStage.Core.Transformers;

namespace TrueStage.Integration.Tests;

public class {CuId}IntegrationTest
{
    [Fact]
    public async Task {CuId}_MapsCsvRowsCorrectly()
    {
        var configsPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
            "..", "..", "..", "..", "..", "configs"));
        if (!Directory.Exists(configsPath)) return;

        var loader = new LocalFileConfigLoader(configsPath);
        var config = await loader.LoadConfigAsync("{cu_id_lower}");

        var samplesPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
            "..", "..", "..", "..", "..", "samples", "{cu_id_lower}_sample.csv"));
        if (!File.Exists(samplesPath)) return;

        using var stream = File.OpenRead(samplesPath);
        var reader = new CsvSourceReader();
        var mapper = new ColumnMapper(new TransformerFactory());

        var rows = new System.Collections.Generic.List<Core.Models.MappedRow>();
        int rowNum = 0;
        foreach (var sourceRow in reader.ReadRows(stream, config))
        {
            rowNum++;
            rows.Add(mapper.MapRow(sourceRow, config, rowNum));
        }

        Assert.True(rows.Count > 0, "No rows mapped");
        Assert.All(rows, r => Assert.False(r.HasError, $"Row {r.RowNumber} error: {r.ErrorMessage}"));

        var first = rows[0];
        Assert.NotNull(first.Data["source_member_id"]);
        Assert.NotNull(first.RecordHash);
        Assert.Equal(64, first.RecordHash!.Length);
    }
}
```
