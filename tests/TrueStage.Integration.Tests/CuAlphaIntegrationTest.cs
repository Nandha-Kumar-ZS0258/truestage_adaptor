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

/// <summary>
/// Integration test for CU_ALPHA adaptor — validates full mapping pipeline
/// using the sample config file.
/// </summary>
public class CuAlphaIntegrationTest
{
    [Fact]
    public async Task CuAlpha_MapsCsvRowsCorrectly()
    {
        // Load config from local configs folder
        var configsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "configs");
        if (!Directory.Exists(configsPath))
        {
            // Skip if configs not available in test environment
            return;
        }

        var loader = new LocalFileConfigLoader(configsPath);
        var config = await loader.LoadConfigAsync("cu_alpha");

        var csv = """
            MemberID,FName,LName,DOB,Email,Phone,Bal,Status,Branch
            M001,John,Doe,01/15/1985,john.doe@email.com,555-1234,$1200.50,A,BR01
            M002,Jane,Smith,03/22/1990,jane.smith@email.com,555-5678,$850.00,I,BR02
            """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
        var reader = new CsvSourceReader();
        var mapper = new ColumnMapper(new TransformerFactory());

        var rows = new System.Collections.Generic.List<Core.Models.MappedRow>();
        int rowNum = 0;
        foreach (var sourceRow in reader.ReadRows(stream, config))
        {
            rowNum++;
            rows.Add(mapper.MapRow(sourceRow, config, rowNum));
        }

        Assert.Equal(2, rows.Count);
        Assert.All(rows, r => Assert.False(r.HasError));

        var first = rows[0];
        Assert.Equal("M001", first.Data["source_member_id"]);
        Assert.Equal("John", first.Data["first_name"]);
        Assert.Equal(1200.50m, first.Data["account_balance"]);
        Assert.Equal("ACTIVE", first.Data["member_status"]);
    }
}
