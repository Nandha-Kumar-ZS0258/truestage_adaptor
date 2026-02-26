using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using TrueStage.ConfigLoader;
using TrueStage.Core.Mappers;
using TrueStage.Core.Readers;
using TrueStage.Core.Transformers;

namespace TrueStage.Integration.Tests;

/// <summary>
/// Integration test for CU_GAMMA adaptor — validates full mapping pipeline
/// using the sample config file.
/// Note: member_name (full name) is mapped to first_name as no split transform exists.
/// </summary>
public class CuGammaIntegrationTest
{
    [Fact]
    public async Task CuGamma_MapsCsvRowsCorrectly()
    {
        // Load config from local configs folder
        var configsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "configs");
        if (!Directory.Exists(configsPath))
        {
            // Skip if configs not available in test environment
            return;
        }

        var loader = new LocalFileConfigLoader(configsPath);
        var config = await loader.LoadConfigAsync("cu_gamma");

        var csv = """
            entity_uuid,member_id,member_name,birth_date,street_name,building_number,postal_code,town_name,state,country,tax_id_masked,email,phone_number
            ad485c18-0729-42cd-9728-34bbd26c78cd,CC00000001,Jesse Baker,1987-03-02,Ryan Branch,85875,97562,Virginia Beach,VA,US,XXX-XX-0001,kfranco@example.org,423.890.2844
            b269b4a3-3872-4bb6-affc-a558049901ff,CC00000002,Kenneth Mclaughlin,1945-05-24,Castillo Circle,85629,50135,Newport News,VA,US,XXX-XX-0002,paige89@example.com,987-938-2337
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
        Assert.Equal("CC00000001", first.Data["source_member_id"]);
        Assert.Equal("Jesse Baker", first.Data["first_name"]);
        Assert.Equal("kfranco@example.org", first.Data["email"]);
        Assert.Equal("423.890.2844", first.Data["phone"]);

        // Verify date parsed correctly (1987-03-02)
        var dob = Assert.IsType<System.DateTime>(first.Data["date_of_birth"]);
        Assert.Equal(1987, dob.Year);
        Assert.Equal(3, dob.Month);
        Assert.Equal(2, dob.Day);
    }
}
