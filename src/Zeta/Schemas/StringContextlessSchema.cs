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

    private StringContextlessSchema(
        ContextlessRuleEngine<string> rules,
        bool allowNull,
        IReadOnlyList<(Func<string, bool>, ISchema<string>)>? conditionals)
        : base(rules, allowNull, conditionals)
    {
    }

    protected override StringContextlessSchema CreateInstance() => new();

    protected override StringContextlessSchema CreateInstance(
        ContextlessRuleEngine<string> rules,
        bool allowNull,
        IReadOnlyList<(Func<string, bool>, ISchema<string>)>? conditionals)
        => new(rules, allowNull, conditionals);

    public StringContextlessSchema MinLength(int min, string? message = null)
        => Append(new MinLengthRule(min, message));

    public StringContextlessSchema MaxLength(int max, string? message = null)
        => Append(new MaxLengthRule(max, message));

    public StringContextlessSchema Length(int exact, string? message = null)
        => Append(new LengthRule(exact, message));

    public StringContextlessSchema NotEmpty(string? message = null)
        => Append(new NotEmptyRule(message));

    public StringContextlessSchema Email(string? message = null)
        => Append(new EmailRule(message));

    public StringContextlessSchema Uuid(string? message = null)
        => Append(new UuidRule(message));

    public StringContextlessSchema Url(string? message = null)
        => Append(new UrlRule(message));

    public StringContextlessSchema Uri(UriKind kind = UriKind.Absolute, string? message = null)
        => Append(new UriRule(kind, message));

    public StringContextlessSchema Alphanumeric(string? message = null)
        => Append(new AlphanumericRule(message));

    public StringContextlessSchema StartsWith(string prefix, StringComparison comparison = StringComparison.Ordinal, string? message = null)
        => Append(new StartsWithRule(prefix, comparison, message));

    public StringContextlessSchema EndsWith(string suffix, StringComparison comparison = StringComparison.Ordinal, string? message = null)
        => Append(new EndsWithRule(suffix, comparison, message));

    public StringContextlessSchema Contains(string substring, StringComparison comparison = StringComparison.Ordinal, string? message = null)
        => Append(new ContainsRule(substring, comparison, message));

    public StringContextlessSchema Regex(string pattern, string? message = null, string code = "regex")
    {
        var compiledRegex = new Regex(
            pattern,
            RegexOptions.Compiled,
            TimeSpan.FromSeconds(1));

        return Append(new RegexRule(compiledRegex, message, code));
    }

    /// <summary>
    /// Creates a context-aware string schema with all rules from this schema.
    /// </summary>
    public StringContextSchema<TContext> Using<TContext>()
    {
        var schema = new StringContextSchema<TContext>(Rules.ToContext<TContext>());
        schema = AllowNull ? schema.Nullable() : schema;
        schema = schema.TransferContextlessConditionals(GetConditionals());
        return schema;
    }

    /// <summary>
    /// Creates a context-aware string schema with a factory delegate for creating context data.
    /// </summary>
    public StringContextSchema<TContext> Using<TContext>(
        Func<string, IServiceProvider, CancellationToken, ValueTask<TContext>> factory)
    {
        return Using<TContext>().WithContextFactory(factory);
    }
}
