using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Zeta.AspNetCore.Tests;

public class ZetaValidatorContextResultTests
{
    private sealed record TestContext(int MaxValue);

    [Fact]
    public async Task ValidateAsync_ContextAware_ReturnsResultWithContext()
    {
        var services = new ServiceCollection();
        services.AddScoped<IZetaValidator, ZetaValidator>();
        await using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var validator = scope.ServiceProvider.GetRequiredService<IZetaValidator>();

        var schema = Z.Int()
            .Using<TestContext>((_, _, _) => ValueTask.FromResult(new TestContext(10)))
            .Refine((value, ctx) => value <= ctx.MaxValue, "too_big");

        Result<int, TestContext> result = await validator.ValidateAsync(5, schema);

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value);
        Assert.Equal(10, result.Context.MaxValue);
    }

    [Fact]
    public async Task ValidateAsync_ContextAware_CanStillAssignToResultOfT()
    {
        var services = new ServiceCollection();
        services.AddScoped<IZetaValidator, ZetaValidator>();
        await using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var validator = scope.ServiceProvider.GetRequiredService<IZetaValidator>();

        var schema = Z.Int()
            .Using<TestContext>((_, _, _) => ValueTask.FromResult(new TestContext(10)))
            .Refine((value, ctx) => value <= ctx.MaxValue, "too_big");

        Result<int> result = await validator.ValidateAsync(5, schema);

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value);
    }
}
