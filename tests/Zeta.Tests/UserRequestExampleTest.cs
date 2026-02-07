using Xunit;
using Zeta.Schemas;

namespace Zeta.Tests;

/// <summary>
/// Tests the exact example from the user's original request.
/// </summary>
public class UserRequestExampleTest
{
    public record FooBar(int Foo, int? Bar);

    [Fact]
    public async Task OriginalExample_InlineBuilder_WithNullBar_Succeeds()
    {
        var schema = Z.Object<FooBar>()
            .Field(x => x.Foo, x => x.Min(10).Max(100))
            .Field(x => x.Bar, x => x.Min(1));

        var result = await schema.ValidateAsync(new FooBar(50, null));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task OriginalExample_InlineBuilder_WithValidBar_Succeeds()
    {
        var schema = Z.Object<FooBar>()
            .Field(x => x.Foo, x => x.Min(10).Max(100))
            .Field(x => x.Bar, x => x.Min(1));

        var result = await schema.ValidateAsync(new FooBar(50, 5));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task OriginalExample_InlineBuilder_WithInvalidBar_Fails()
    {
        var schema = Z.Object<FooBar>()
            .Field(x => x.Foo, x => x.Min(10).Max(100))
            .Field(x => x.Bar, x => x.Min(1));

        var result = await schema.ValidateAsync(new FooBar(50, 0));
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Path == "bar" && e.Code == "min_value");
    }

    [Fact]
    public async Task OriginalExample_PrebuiltSchema_WithNullBar_Succeeds()
    {
        var schema = Z.Object<FooBar>()
            .Field(x => x.Foo, x => x.Min(10).Max(100))
            .Field(x => x.Bar, Z.Int().Min(1));

        var result = await schema.ValidateAsync(new FooBar(50, null));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task OriginalExample_PrebuiltSchema_WithValidBar_Succeeds()
    {
        var schema = Z.Object<FooBar>()
            .Field(x => x.Foo, x => x.Min(10).Max(100))
            .Field(x => x.Bar, Z.Int().Min(1));

        var result = await schema.ValidateAsync(new FooBar(50, 5));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task OriginalExample_PrebuiltSchema_WithInvalidBar_Fails()
    {
        var schema = Z.Object<FooBar>()
            .Field(x => x.Foo, x => x.Min(10).Max(100))
            .Field(x => x.Bar, Z.Int().Min(1));

        var result = await schema.ValidateAsync(new FooBar(50, 0));
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Path == "bar" && e.Code == "min_value");
    }

    [Fact]
    public async Task OriginalExample_BothFieldsInvalid_ReturnsAllErrors()
    {
        var schema = Z.Object<FooBar>()
            .Field(x => x.Foo, x => x.Min(10).Max(100))
            .Field(x => x.Bar, x => x.Min(1));

        var result = await schema.ValidateAsync(new FooBar(5, 0));
        Assert.False(result.IsSuccess);
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains(result.Errors, e => e.Path == "foo" && e.Code == "min_value");
        Assert.Contains(result.Errors, e => e.Path == "bar" && e.Code == "min_value");
    }
}
