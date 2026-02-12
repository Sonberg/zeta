using Zeta.Core;

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
        var schema = Z.String()
            .Using<TestContext>()
            .Refine((val, ctx) => val == ctx.MagicWord, "Wrong magic word");

        var context = new TestContext("Abracadabra");

        var valid = await schema.ValidateAsync("Abracadabra", context);
        Assert.True(valid.IsSuccess);

        var invalid = await schema.ValidateAsync("HocusPocus", context);
        Assert.False(invalid.IsSuccess);
        Assert.Contains(invalid.Errors, e => e.Message == "Wrong magic word");
    }

    [Fact]
    public async Task Uuid_Valid_ReturnsSuccess()
    {
        var schema = Z.String().Uuid();
        var result = await schema.ValidateAsync("550e8400-e29b-41d4-a716-446655440000");
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Uuid_Invalid_ReturnsFailure()
    {
        var schema = Z.String().Uuid();
        var result = await schema.ValidateAsync("not-a-uuid");

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "uuid");
    }

    [Fact]
    public async Task Url_Valid_ReturnsSuccess()
    {
        var schema = Z.String().Url();

        Assert.True((await schema.ValidateAsync("https://example.com")).IsSuccess);
        Assert.True((await schema.ValidateAsync("http://example.com/path?query=1")).IsSuccess);
    }

    [Fact]
    public async Task Url_Invalid_ReturnsFailure()
    {
        var schema = Z.String().Url();

        Assert.False((await schema.ValidateAsync("not-a-url")).IsSuccess);
        Assert.False((await schema.ValidateAsync("ftp://example.com")).IsSuccess); // Only http/https allowed
        Assert.False((await schema.ValidateAsync("example.com")).IsSuccess); // Missing scheme
    }

    [Fact]
    public async Task Uri_Valid_ReturnsSuccess()
    {
        var schema = Z.String().Uri();

        Assert.True((await schema.ValidateAsync("https://example.com")).IsSuccess);
        Assert.True((await schema.ValidateAsync("ftp://example.com")).IsSuccess);
        Assert.True((await schema.ValidateAsync("file:///path/to/file")).IsSuccess);
    }

    [Fact]
    public async Task Uri_Invalid_ReturnsFailure()
    {
        var schema = Z.String().Uri();
        var result = await schema.ValidateAsync("not a valid uri");

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "uri");
    }

    [Fact]
    public async Task Alphanumeric_Valid_ReturnsSuccess()
    {
        var schema = Z.String().Alphanumeric();

        Assert.True((await schema.ValidateAsync("abc123")).IsSuccess);
        Assert.True((await schema.ValidateAsync("ABC")).IsSuccess);
        Assert.True((await schema.ValidateAsync("123")).IsSuccess);
    }

    [Fact]
    public async Task Alphanumeric_Invalid_ReturnsFailure()
    {
        var schema = Z.String().Alphanumeric();

        Assert.False((await schema.ValidateAsync("abc-123")).IsSuccess);
        Assert.False((await schema.ValidateAsync("hello world")).IsSuccess);
        Assert.False((await schema.ValidateAsync("test@email")).IsSuccess);
    }

    [Fact]
    public async Task StartsWith_Valid_ReturnsSuccess()
    {
        var schema = Z.String().StartsWith("Hello");
        var result = await schema.ValidateAsync("Hello, World!");
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task StartsWith_Invalid_ReturnsFailure()
    {
        var schema = Z.String().StartsWith("Hello");
        var result = await schema.ValidateAsync("Hi, World!");

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "starts_with");
    }

    [Fact]
    public async Task EndsWith_Valid_ReturnsSuccess()
    {
        var schema = Z.String().EndsWith(".txt");
        var result = await schema.ValidateAsync("document.txt");
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task EndsWith_Invalid_ReturnsFailure()
    {
        var schema = Z.String().EndsWith(".txt");
        var result = await schema.ValidateAsync("document.pdf");

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "ends_with");
    }

    [Fact]
    public async Task Contains_Valid_ReturnsSuccess()
    {
        var schema = Z.String().Contains("world");
        var result = await schema.ValidateAsync("Hello, world!");
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Contains_Invalid_ReturnsFailure()
    {
        var schema = Z.String().Contains("world");
        var result = await schema.ValidateAsync("Hello, everyone!");

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "contains");
    }

    [Fact]
    public async Task Length_Valid_ReturnsSuccess()
    {
        var schema = Z.String().Length(5);
        var result = await schema.ValidateAsync("hello");
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Length_Invalid_ReturnsFailure()
    {
        var schema = Z.String().Length(5);

        Assert.False((await schema.ValidateAsync("hi")).IsSuccess);
        Assert.False((await schema.ValidateAsync("hello world")).IsSuccess);
    }

    [Fact]
    public async Task MaxLength_Valid_ReturnsSuccess()
    {
        var schema = Z.String().MaxLength(5);
        var result = await schema.ValidateAsync("hello");
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task MaxLength_Invalid_ReturnsFailure()
    {
        var schema = Z.String().MaxLength(5);
        var result = await schema.ValidateAsync("hello world");

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "max_length");
    }

    [Fact]
    public async Task Regex_Valid_ReturnsSuccess()
    {
        var schema = Z.String().Regex(@"^\d{3}-\d{4}$");
        var result = await schema.ValidateAsync("123-4567");
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Regex_Invalid_ReturnsFailure()
    {
        var schema = Z.String().Regex(@"^\d{3}-\d{4}$");
        var result = await schema.ValidateAsync("abc-defg");

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "regex");
    }

    [Fact]
    public async Task NotEmpty_Valid_ReturnsSuccess()
    {
        var schema = Z.String().NotEmpty();
        var result = await schema.ValidateAsync("hello");
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task NotEmpty_WhitespaceOnly_ReturnsFailure()
    {
        var schema = Z.String().NotEmpty();
        var result = await schema.ValidateAsync("   ");

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "required");
    }

    [Fact]
    public async Task NotEmpty_EmptyString_ReturnsFailure()
    {
        var schema = Z.String().NotEmpty();
        var result = await schema.ValidateAsync("");

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "required");
    }

    [Fact]
    public async Task RefineAsync_Valid_ReturnsSuccess()
    {
        var schema = Z.String().RefineAsync(
            val => new ValueTask<bool>(val.Length > 3),
            "Must be longer than 3");

        var result = await schema.ValidateAsync("hello");
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task RefineAsync_Invalid_ReturnsFailure()
    {
        var schema = Z.String().RefineAsync(
            val => new ValueTask<bool>(val.Length > 3),
            "Must be longer than 3");

        var result = await schema.ValidateAsync("hi");

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Message == "Must be longer than 3");
    }

    [Fact]
    public async Task RefineAsync_WithCancellationToken_Works()
    {
        var schema = Z.String().RefineAsync(
            (val, ct) => new ValueTask<bool>(val.Length > 3),
            "Must be longer than 3");

        var result = await schema.ValidateAsync("hello");
        Assert.True(result.IsSuccess);

        var result2 = await schema.ValidateAsync("hi");
        Assert.False(result2.IsSuccess);
    }

    [Fact]
    public async Task MinLength_CustomMessage_ReturnsCustomMessage()
    {
        var schema = Z.String().MinLength(5, "At least 5 chars please");
        var result = await schema.ValidateAsync("hi");

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Message == "At least 5 chars please");
    }

    public record TestContext(string MagicWord);
}
