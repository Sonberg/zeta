using Zeta.Schemas;

namespace Zeta.Tests;

public class IntSchemaTests
{
    [Fact]
    public async Task Min_Valid_ReturnsSuccess()
    {
        var schema = Zeta.Int().Min(10);
        var result = await schema.ValidateAsync(10);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Min_Invalid_ReturnsFailure()
    {
        var schema = Zeta.Int().Min(10);
        var result = await schema.ValidateAsync(9);
        
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "min_value");
    }

    [Fact]
    public async Task Max_Valid_ReturnsSuccess()
    {
        var schema = Zeta.Int().Max(10);
        var result = await schema.ValidateAsync(10);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Max_Invalid_ReturnsFailure()
    {
        var schema = Zeta.Int().Max(10);
        var result = await schema.ValidateAsync(11);
        
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "max_value");
    }

    public record LimitContext(int MaxLimit);

    [Fact]
    public async Task ContextRefine_UsesContextData()
    {
        var schema = Zeta.Int<LimitContext>()
            .Refine((val, ctx) => val <= ctx.MaxLimit, "Exceeds dynamic limit");

        var context = new ValidationContext<LimitContext>(
            new LimitContext(50), 
            ValidationExecutionContext.Empty);

        Assert.True((await schema.ValidateAsync(50, context)).IsSuccess);
        Assert.False((await schema.ValidateAsync(51, context)).IsSuccess);
    }
}
