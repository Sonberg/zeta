using Zeta.Core;
using Zeta.Schemas;

namespace Zeta.Tests;

public class StringSchemaTests
{
    [Fact]
    public async Task MinLength_Valid_ReturnsSuccess()
    {
        var schema = Z.String().MinLength(3);
        var result = await schema.ValidateAsync("abc");
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task MinLength_Invalid_ReturnsFailure()
    {
        var schema = Z.String().MinLength(3);
        var result = await schema.ValidateAsync("ab");
        
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "min_length");
    }

    [Fact]
    public async Task Email_Valid_ReturnsSuccess()
    {
        var schema = Z.String().Email();
        var result = await schema.ValidateAsync("test@example.com");
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Email_Invalid_ReturnsFailure()
    {
        var schema = Z.String().Email();
        var result = await schema.ValidateAsync("invalid-email");
        
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "email");
    }

    [Fact]
    public async Task ContextRefine_UsesContextData()
    {
        // Setup schema that requires a "MagicWord" from context
        var schema = Z.String<TestContext>()
            .Refine((val, ctx) => val == ctx.MagicWord, "Wrong magic word");

        var context = new ValidationContext<TestContext>(
            new TestContext("Abracadabra"), 
            ValidationExecutionContext.Empty);

        var valid = await schema.ValidateAsync("Abracadabra", context);
        Assert.True(valid.IsSuccess);

        var invalid = await schema.ValidateAsync("HocusPocus", context);
        Assert.False(invalid.IsSuccess);
        Assert.Contains(invalid.Errors, e => e.Message == "Wrong magic word");
    }

    public record TestContext(string MagicWord);
}
