using Zeta.Core;
using Zeta.Rules.String;

namespace Zeta.Tests;

public class StringRuleTests
{
    private static ValidationContext Context => new();

    [Theory]
    [InlineData("abc", 3, true)]
    [InlineData("abcd", 3, true)]
    [InlineData("ab", 3, false)]
    [InlineData("", 1, false)]
    public async Task MinLengthRule_ValidatesCorrectly(string value, int min, bool shouldPass)
    {
        var rule = new MinLengthRule(min);
        var error = await rule.ValidateAsync(value, Context);

        if (shouldPass)
            Assert.Null(error);
        else
        {
            Assert.NotNull(error);
            Assert.Equal("min_length", error.Code);
            Assert.Contains($"at least {min}", error.Message);
        }
    }

    [Theory]
    [InlineData("abc", 3, true)]
    [InlineData("ab", 3, true)]
    [InlineData("abcd", 3, false)]
    public async Task MaxLengthRule_ValidatesCorrectly(string value, int max, bool shouldPass)
    {
        var rule = new MaxLengthRule(max);
        var error = await rule.ValidateAsync(value, Context);

        if (shouldPass)
            Assert.Null(error);
        else
        {
            Assert.NotNull(error);
            Assert.Equal("max_length", error.Code);
            Assert.Contains($"at most {max}", error.Message);
        }
    }

    [Theory]
    [InlineData("abc", 3, true)]
    [InlineData("ab", 3, false)]
    [InlineData("abcd", 3, false)]
    public async Task LengthRule_ValidatesCorrectly(string value, int exact, bool shouldPass)
    {
        var rule = new LengthRule(exact);
        var error = await rule.ValidateAsync(value, Context);

        if (shouldPass)
            Assert.Null(error);
        else
        {
            Assert.NotNull(error);
            Assert.Equal("length", error.Code);
            Assert.Contains($"exactly {exact}", error.Message);
        }
    }

    [Theory]
    [InlineData("hello", true)]
    [InlineData(" ", false)]
    [InlineData("", false)]
    [InlineData("  \t\n", false)]
    public async Task NotEmptyRule_ValidatesCorrectly(string value, bool shouldPass)
    {
        var rule = new NotEmptyRule();
        var error = await rule.ValidateAsync(value, Context);

        if (shouldPass)
            Assert.Null(error);
        else
        {
            Assert.NotNull(error);
            Assert.Equal("required", error.Code);
        }
    }

    [Theory]
    [InlineData("test@example.com", true)]
    [InlineData("user+tag@domain.co.uk", true)]
    [InlineData("test@example", false)]
    [InlineData("@example.com", false)]
    [InlineData("test@", false)]
    [InlineData("invalid-email", false)]
    [InlineData("test @example.com", false)]
    public async Task EmailRule_ValidatesCorrectly(string value, bool shouldPass)
    {
        var rule = new EmailRule();
        var error = await rule.ValidateAsync(value, Context);

        if (shouldPass)
            Assert.Null(error);
        else
        {
            Assert.NotNull(error);
            Assert.Equal("email", error.Code);
        }
    }

    [Theory]
    [InlineData("550e8400-e29b-41d4-a716-446655440000", true)]
    [InlineData("00000000-0000-0000-0000-000000000000", true)]
    [InlineData("not-a-uuid", false)]
    [InlineData("550e8400-e29b-41d4-a716", false)]
    [InlineData("", false)]
    public async Task UuidRule_ValidatesCorrectly(string value, bool shouldPass)
    {
        var rule = new UuidRule();
        var error = await rule.ValidateAsync(value, Context);

        if (shouldPass)
            Assert.Null(error);
        else
        {
            Assert.NotNull(error);
            Assert.Equal("uuid", error.Code);
        }
    }

    [Theory]
    [InlineData("https://example.com", true)]
    [InlineData("http://example.com", true)]
    [InlineData("https://example.com/path?query=1", true)]
    [InlineData("ftp://example.com", false)]
    [InlineData("example.com", false)]
    [InlineData("not-a-url", false)]
    public async Task UrlRule_ValidatesCorrectly(string value, bool shouldPass)
    {
        var rule = new UrlRule();
        var error = await rule.ValidateAsync(value, Context);

        if (shouldPass)
            Assert.Null(error);
        else
        {
            Assert.NotNull(error);
            Assert.Equal("url", error.Code);
        }
    }

    [Theory]
    [InlineData("https://example.com", true)]
    [InlineData("ftp://example.com", true)]
    [InlineData("file:///path/to/file", true)]
    public async Task UriRule_ValidatesCorrectly(string value, bool shouldPass)
    {
        var rule = new UriRule();
        var error = await rule.ValidateAsync(value, Context);

        if (shouldPass)
            Assert.Null(error);
        else
        {
            Assert.NotNull(error);
            Assert.Equal("uri", error.Code);
        }
    }

    [Fact]
    public async Task UriRule_WithRelativeKind_AcceptsRelativeUri()
    {
        var rule = new UriRule(UriKind.Relative);
        var error = await rule.ValidateAsync("/path/to/resource", Context);

        Assert.Null(error);
    }

    [Theory]
    [InlineData("abc123", true)]
    [InlineData("ABC", true)]
    [InlineData("123", true)]
    [InlineData("abc-123", false)]
    [InlineData("hello world", false)]
    [InlineData("test@email", false)]
    public async Task AlphanumericRule_ValidatesCorrectly(string value, bool shouldPass)
    {
        var rule = new AlphanumericRule();
        var error = await rule.ValidateAsync(value, Context);

        if (shouldPass)
            Assert.Null(error);
        else
        {
            Assert.NotNull(error);
            Assert.Equal("alphanumeric", error.Code);
        }
    }

    [Theory]
    [InlineData("Hello, World!", "Hello", true)]
    [InlineData("Hello, World!", "World", false)]
    [InlineData("TEST", "test", false)] // Case-sensitive by default
    public async Task StartsWithRule_ValidatesCorrectly(string value, string prefix, bool shouldPass)
    {
        var rule = new StartsWithRule(prefix);
        var error = await rule.ValidateAsync(value, Context);

        if (shouldPass)
            Assert.Null(error);
        else
        {
            Assert.NotNull(error);
            Assert.Equal("starts_with", error.Code);
        }
    }

    [Fact]
    public async Task StartsWithRule_WithCaseInsensitive_WorksCorrectly()
    {
        var rule = new StartsWithRule("test", StringComparison.OrdinalIgnoreCase);
        var error = await rule.ValidateAsync("TEST", Context);
        Assert.Null(error);
    }

    [Theory]
    [InlineData("document.txt", ".txt", true)]
    [InlineData("document.pdf", ".txt", false)]
    [InlineData("TEST.TXT", ".txt", false)] // Case-sensitive by default
    public async Task EndsWithRule_ValidatesCorrectly(string value, string suffix, bool shouldPass)
    {
        var rule = new EndsWithRule(suffix);
        var error = await rule.ValidateAsync(value, Context);

        if (shouldPass)
            Assert.Null(error);
        else
        {
            Assert.NotNull(error);
            Assert.Equal("ends_with", error.Code);
        }
    }

    [Theory]
    [InlineData("Hello, world!", "world", true)]
    [InlineData("Hello, everyone!", "world", false)]
    public async Task ContainsRule_ValidatesCorrectly(string value, string substring, bool shouldPass)
    {
        var rule = new ContainsRule(substring);
        var error = await rule.ValidateAsync(value, Context);

        if (shouldPass)
            Assert.Null(error);
        else
        {
            Assert.NotNull(error);
            Assert.Equal("contains", error.Code);
        }
    }

    [Theory]
    [InlineData("abc123", @"^[a-z0-9]+$", true)]
    [InlineData("ABC123", @"^[a-z0-9]+$", false)]
    [InlineData("test@example.com", @"^[^@\s]+@[^@\s]+\.[^@\s]+$", true)]
    public async Task RegexRule_ValidatesCorrectly(string value, string pattern, bool shouldPass)
    {
        var rule = new RegexRule(new System.Text.RegularExpressions.Regex(pattern));
        var error = await rule.ValidateAsync(value, Context);

        if (shouldPass)
            Assert.Null(error);
        else
        {
            Assert.NotNull(error);
            Assert.Equal("regex", error.Code);
        }
    }

    [Fact]
    public async Task MinLengthRule_WithCustomMessage_UsesCustomMessage()
    {
        var rule = new MinLengthRule(5, "Custom error message");
        var error = await rule.ValidateAsync("abc", Context);

        Assert.NotNull(error);
        Assert.Equal("Custom error message", error.Message);
    }

    [Fact]
    public async Task EmailRule_WithCustomMessage_UsesCustomMessage()
    {
        var rule = new EmailRule("Invalid email address");
        var error = await rule.ValidateAsync("not-an-email", Context);

        Assert.NotNull(error);
        Assert.Equal("Invalid email address", error.Message);
    }
}
