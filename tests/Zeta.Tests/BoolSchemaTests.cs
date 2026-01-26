using Zeta.Core;

namespace Zeta.Tests;

public class BoolSchemaTests
{
    [Fact]
    public async Task IsTrue_Valid_ReturnsSuccess()
    {
        var schema = Z.Bool().IsTrue();
        var result = await schema.ValidateAsync(true);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task IsTrue_Invalid_ReturnsFailure()
    {
        var schema = Z.Bool().IsTrue();
        var result = await schema.ValidateAsync(false);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "is_true");
    }

    [Fact]
    public async Task IsFalse_Valid_ReturnsSuccess()
    {
        var schema = Z.Bool().IsFalse();
        var result = await schema.ValidateAsync(false);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task IsFalse_Invalid_ReturnsFailure()
    {
        var schema = Z.Bool().IsFalse();
        var result = await schema.ValidateAsync(true);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "is_false");
    }

    [Fact]
    public async Task Refine_Valid_ReturnsSuccess()
    {
        var schema = Z.Bool().Refine(b => b, "Must accept terms");
        var result = await schema.ValidateAsync(true);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Refine_Invalid_ReturnsFailure()
    {
        var schema = Z.Bool().Refine(b => b, "Must accept terms");
        var result = await schema.ValidateAsync(false);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Message == "Must accept terms");
    }

    [Fact]
    public async Task NoRules_AlwaysValid()
    {
        var schema = Z.Bool();

        Assert.True((await schema.ValidateAsync(true)).IsSuccess);
        Assert.True((await schema.ValidateAsync(false)).IsSuccess);
    }

    [Fact]
    public async Task ContextRefine_UsesContextData()
    {
        var schema = Z.Bool()
            .WithContext<FeatureFlags>()
            .Refine((val, ctx) => !ctx.RequireTrue || val, "Feature flag requires true value");

        var contextRequiresTrue = new FeatureFlags(true);
        var contextNoRequirement = new FeatureFlags(false);

        // When RequireTrue is true, only true passes
        Assert.True((await schema.ValidateAsync(true, contextRequiresTrue)).IsSuccess);
        Assert.False((await schema.ValidateAsync(false, contextRequiresTrue)).IsSuccess);

        // When RequireTrue is false, both pass
        Assert.True((await schema.ValidateAsync(true, contextNoRequirement)).IsSuccess);
        Assert.True((await schema.ValidateAsync(false, contextNoRequirement)).IsSuccess);
    }

    [Fact]
    public async Task Nullable_AllowsNull()
    {
        var schema = Z.Bool().IsTrue().Nullable();
        var result = await schema.ValidateAsync(null);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Nullable_ValidatesNonNull()
    {
        var schema = Z.Bool().IsTrue().Nullable();
        var result = await schema.ValidateAsync(false);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "is_true");
    }

    public record FeatureFlags(bool RequireTrue);
}
