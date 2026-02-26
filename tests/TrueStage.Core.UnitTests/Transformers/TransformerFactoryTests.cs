using Xunit;
using TrueStage.Core.Transformers;

namespace TrueStage.Core.UnitTests.Transformers;

public class TransformerFactoryTests
{
    private readonly TransformerFactory _factory = new();

    [Fact]
    public void Trim_RemovesWhitespace()
    {
        var result = _factory.ApplyTransform("  hello  ", "trim");
        Assert.Equal("hello", result);
    }

    [Fact]
    public void ToDecimal_ParsesFormattedAmount()
    {
        var result = _factory.ApplyTransform("$1,234.56", "to_decimal");
        Assert.Equal(1234.56m, result);
    }

    [Fact]
    public void Date_ParsesWithFormat()
    {
        var result = _factory.ApplyTransform("01/15/1990", "date:MM/dd/yyyy");
        Assert.IsType<System.DateTime>(result);
    }

    [Fact]
    public void ValueMap_MapsCorrectly()
    {
        var result = _factory.ApplyTransform("A", "value_map:A->ACTIVE,I->INACTIVE");
        Assert.Equal("ACTIVE", result);
    }

    [Fact]
    public void ValueMap_ReturnsOriginalIfNoMatch()
    {
        var result = _factory.ApplyTransform("X", "value_map:A->ACTIVE");
        Assert.Equal("X", result);
    }

    [Fact]
    public void Default_ReturnsDefaultWhenNull()
    {
        var result = _factory.ApplyTransform(null, "default:UNKNOWN");
        Assert.Equal("UNKNOWN", result);
    }

    [Fact]
    public void Default_ReturnsOriginalWhenNotNull()
    {
        var result = _factory.ApplyTransform("active", "default:UNKNOWN");
        Assert.Equal("active", result);
    }

    [Fact]
    public void None_ReturnsValueUnchanged()
    {
        var result = _factory.ApplyTransform("hello", "none");
        Assert.Equal("hello", result);
    }

    [Fact]
    public void ChainedTransforms_ApplyInOrder()
    {
        var result = _factory.ApplyTransform("  $1,200.00  ", "trim, to_decimal");
        Assert.Equal(1200.00m, result);
    }
}
