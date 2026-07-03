using Zeta.Core;

namespace Zeta.Tests;

public class WithErrorTests
{
    [Fact]
    public async Task WithError_OverridesCodeMessageAndPath()
    {
        var schema = Z.String()
            .MinLength(5)
            .WithError(x => x.Code("invalid_name").Path("Name").Message("Must be at least 5 characters long"));

        var result = await schema.ValidateAsync("ab");

        Assert.False(result.IsSuccess);
        var error = Assert.Single(result.Errors);
        Assert.Equal("invalid_name", error.Code);
        Assert.Equal("Must be at least 5 characters long", error.Message);
        Assert.Equal("$.name", error.PathString); // default formatter camel-cases property segments
    }

    [Fact]
    public async Task WithError_OnlyAffectsMostRecentRule()
    {
        var schema = Z.String()
            .MinLength(5, "too short")
            .MaxLength(10)
            .WithError(x => x.Code("too_long"));

        var min = await schema.ValidateAsync("ab");
        Assert.Equal("min_length", Assert.Single(min.Errors).Code);

        var max = await schema.ValidateAsync("aaaaaaaaaaa");
        Assert.Equal("too_long", Assert.Single(max.Errors).Code);
    }

    [Fact]
    public async Task WithError_PartialOverrideKeepsOriginalFields()
    {
        var schema = Z.String().Email().WithError(x => x.Message("bad email"));

        var result = await schema.ValidateAsync("nope");

        var error = Assert.Single(result.Errors);
        Assert.Equal("email", error.Code);          // untouched
        Assert.Equal("bad email", error.Message);   // overridden
    }

    [Fact]
    public void WithError_WithNoPrecedingRule_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => Z.String().WithError(x => x.Code("nope")));
    }

    [Fact]
    public async Task WithError_ContextAware_OverridesCode()
    {
        var schema = Z.String().MinLength(5).Using<object>()
            .WithError(x => x.Code("invalid_name"));

        var result = await schema.ValidateAsync("ab", new object());

        Assert.Equal("invalid_name", Assert.Single(result.Errors).Code);
    }
}
