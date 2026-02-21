using Zeta.Core;

namespace Zeta.Tests;

public class IntSchemaTests
{
    [Fact]
    public async Task Min_Valid_ReturnsSuccess()
    {
        var schema = Z.Int().Min(10);
        var result = await schema.ValidateAsync(10);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Min_Invalid_ReturnsFailure()
    {
        var schema = Z.Int().Min(10);
        var result = await schema.ValidateAsync(9);
        
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "min_value");
    }

    [Fact]
    public async Task Max_Valid_ReturnsSuccess()
    {
        var schema = Z.Int().Max(10);
        var result = await schema.ValidateAsync(10);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Max_Invalid_ReturnsFailure()
    {
        var schema = Z.Int().Max(10);
        var result = await schema.ValidateAsync(11);
        
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "max_value");
    }

    [Fact]
    public async Task Range_Valid_ReturnsSuccess()
    {
        var schema = Z.Int().Range(10, 20);
        var result = await schema.ValidateAsync(15);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Range_Invalid_ReturnsMinOrMaxFailure()
    {
        var schema = Z.Int().Range(10, 20);
        var below = await schema.ValidateAsync(9);
        var above = await schema.ValidateAsync(21);

        Assert.False(below.IsSuccess);
        Assert.Contains(below.Errors, e => e.Code == "min_value");
        Assert.False(above.IsSuccess);
        Assert.Contains(above.Errors, e => e.Code == "max_value");
    }

    [Fact]
    public void Range_MinGreaterThanMax_Throws()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => Z.Int().Range(20, 10));
        Assert.Equal("min", ex.ParamName);
    }

    public record LimitContext(int MaxLimit);

    [Fact]
    public async Task ContextRefine_UsesContextData()
    {
        var schema = Z.Int()
            .Using<LimitContext>()
            .Refine((val, ctx) => val <= ctx.MaxLimit, "Exceeds dynamic limit");

        var context = new LimitContext(50);

        Assert.True((await schema.ValidateAsync(50, context)).IsSuccess);
        Assert.False((await schema.ValidateAsync(51, context)).IsSuccess);
    }
}
