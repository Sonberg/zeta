using System.Text.RegularExpressions;
using Zeta.Core;
using Zeta.Rules;
using Zeta.Rules.String;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating string values.
/// </summary>
public sealed class StringContextlessSchema : ContextlessSchema<string, StringContextlessSchema>
{
    internal StringContextlessSchema()
    {
    }

    protected override StringContextlessSchema CreateInstance() => new();

    public StringContextlessSchema MinLength(int min, string? message = null)
    {
        Use(new MinLengthRule(min, message));
        return this;
    }

    public StringContextlessSchema MaxLength(int max, string? message = null)
    {
        Use(new MaxLengthRule(max, message));
        return this;
    }

    public StringContextlessSchema Length(int exact, string? message = null)
    {
        Use(new LengthRule(exact, message));
        return this;
    }

    public StringContextlessSchema NotEmpty(string? message = null)
    {
        Use(new NotEmptyRule(message));
        return this;
    }

    public StringContextlessSchema Email(string? message = null)
    {
        Use(new EmailRule(message));
        return this;
    }

    public StringContextlessSchema Uuid(string? message = null)
    {
        Use(new UuidRule(message));
        return this;
    }

    public StringContextlessSchema Url(string? message = null)
    {
        Use(new UrlRule(message));
        return this;
    }

    public StringContextlessSchema Uri(UriKind kind = UriKind.Absolute, string? message = null)
    {
        Use(new UriRule(kind, message));
        return this;
    }

    public StringContextlessSchema Alphanumeric(string? message = null)
    {
        Use(new AlphanumericRule(message));
        return this;
    }

    public StringContextlessSchema StartsWith(string prefix, StringComparison comparison = StringComparison.Ordinal, string? message = null)
    {
        Use(new StartsWithRule(prefix, comparison, message));
        return this;
    }

    public StringContextlessSchema EndsWith(string suffix, StringComparison comparison = StringComparison.Ordinal, string? message = null)
    {
        Use(new EndsWithRule(suffix, comparison, message));
        return this;
    }

    public StringContextlessSchema Contains(string substring, StringComparison comparison = StringComparison.Ordinal, string? message = null)
    {
        Use(new ContainsRule(substring, comparison, message));
        return this;
    }

    public StringContextlessSchema Regex(string pattern, string? message = null, string code = "regex")
    {
        var compiledRegex = new Regex(
            pattern,
            RegexOptions.Compiled,
            TimeSpan.FromSeconds(1));

        Use(new RegexRule(compiledRegex, message, code));
        return this;
    }

    /// <summary>
    /// Creates a context-aware string schema with all rules from this schema.
    /// </summary>
    public StringContextSchema<TContext> Using<TContext>()
    {
        var schema = new StringContextSchema<TContext>(Rules.ToContext<TContext>());
        if (AllowNull) schema.Nullable();
        schema.TransferContextlessConditionals(GetConditionals());
        return schema;
    }

    /// <summary>
    /// Creates a context-aware string schema with a factory delegate for creating context data.
    /// </summary>
    public StringContextSchema<TContext> Using<TContext>(
        Func<string, IServiceProvider, CancellationToken, ValueTask<TContext>> factory)
    {
        var schema = Using<TContext>();
        schema.SetContextFactory(factory);
        return schema;
    }
}