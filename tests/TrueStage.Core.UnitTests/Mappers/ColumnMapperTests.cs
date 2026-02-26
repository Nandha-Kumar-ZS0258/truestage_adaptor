using System.Collections.Generic;
using Xunit;
using TrueStage.Core.Mappers;
using TrueStage.Core.Models;
using TrueStage.Core.Transformers;

namespace TrueStage.Core.UnitTests.Mappers;

public class ColumnMapperTests
{
    private readonly ColumnMapper _mapper = new(new TransformerFactory());

    private static CuMappingConfig BuildConfig() => new()
    {
        CuId = "CU_TEST",
        Mappings = new()
        {
            new() { SourceCol = "mem_no", TargetCol = "source_member_id", Transform = "trim" },
            new() { SourceCol = "f_name", TargetCol = "first_name", Transform = "trim" },
            new() { SourceCol = "balance", TargetCol = "account_balance", Transform = "to_decimal" },
        }
    };

    [Fact]
    public void MapRow_MapsColumnsCorrectly()
    {
        var source = new Dictionary<string, string>
        {
            ["mem_no"] = "  M001  ",
            ["f_name"] = "  Jane  ",
            ["balance"] = "1500.00"
        };

        var result = _mapper.MapRow(source, BuildConfig(), 1);

        Assert.False(result.HasError);
        Assert.Equal("M001", result.Data["source_member_id"]);
        Assert.Equal("Jane", result.Data["first_name"]);
        Assert.Equal(1500.00m, result.Data["account_balance"]);
    }

    [Fact]
    public void MapRow_SetsRecordHash()
    {
        var source = new Dictionary<string, string>
        {
            ["mem_no"] = "M001", ["f_name"] = "Jane", ["balance"] = "1500"
        };
        var result = _mapper.MapRow(source, BuildConfig(), 1);
        Assert.NotNull(result.RecordHash);
        Assert.Equal(64, result.RecordHash!.Length); // SHA256 hex
    }

    [Fact]
    public void MapRow_RequiredMissingField_SetsError()
    {
        var config = new CuMappingConfig
        {
            CuId = "CU_TEST",
            Mappings = new() { new() { SourceCol = "mem_no", TargetCol = "source_member_id", Required = true } }
        };
        var source = new Dictionary<string, string>(); // mem_no missing
        var result = _mapper.MapRow(source, config, 1);
        Assert.True(result.HasError);
    }
}
