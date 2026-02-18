using System.Text.RegularExpressions;
using Zeta.Core;
using Zeta.Rules;
using Zeta.Rules.String;

namespace Zeta.Schemas;

/// <summary>
/// A context-aware schema for validating string values.
/// </summary>
public class StringContextSchema<TContext> : ContextSchema<string, TContext, StringContextSchema<TContext>>
{
    internal StringContextSchema() { }

    internal StringContextSchema(ContextRuleEngine<string, TContext> rules) : base(rules)
    {
    }

    private StringContextSchema(
        ContextRuleEngine<string, TContext> rules,
        bool allowNull,
        IReadOnlyList<ISchemaConditional<string, TContext>>? conditionals,
        Func<string, IServiceProvider, CancellationToken, ValueTask<TContext>>? contextFactory)
        : base(rules, allowNull, conditionals, contextFactory)
    {
    }

    protected override StringContextSchema<TContext> CreateInstance() => new();

    private protected override StringContextSchema<TContext> CreateInstance(
        ContextRuleEngine<string, TContext> rules,
        bool allowNull,
        IReadOnlyList<ISchemaConditional<string, TContext>>? conditionals,
        Func<string, IServiceProvider, CancellationToken, ValueTask<TContext>>? contextFactory)
        => new(rules, allowNull, conditionals, contextFactory);

    public StringContextSchema<TContext> MinLength(int min, string? message = null)
        => Append(new MinLengthRule<TContext>(min, message));

    public StringContextSchema<TContext> MaxLength(int max, string? message = null)
        => Append(new MaxLengthRule<TContext>(max, message));

    public StringContextSchema<TContext> Length(int exact, string? message = null)
        => Append(new LengthRule<TContext>(exact, message));

    public StringContextSchema<TContext> NotEmpty(string? message = null)
        => Append(new NotEmptyRule<TContext>(message));

    public StringContextSchema<TContext> Email(string? message = null)
        => Append(new EmailRule<TContext>(message));

    public StringContextSchema<TContext> Uuid(string? message = null)
        => Append(new UuidRule<TContext>(message));

    public StringContextSchema<TContext> Url(string? message = null)
        => Append(new UrlRule<TContext>(message));

    public StringContextSchema<TContext> Uri(UriKind kind = UriKind.Absolute, string? message = null)
        => Append(new UriRule<TContext>(kind, message));

    public StringContextSchema<TContext> Alphanumeric(string? message = null)
        => Append(new AlphanumericRule<TContext>(message));

    public StringContextSchema<TContext> StartsWith(string prefix, StringComparison comparison = StringComparison.Ordinal, string? message = null)
        => Append(new StartsWithRule<TContext>(prefix, comparison, message));

    public StringContextSchema<TContext> EndsWith(string suffix, StringComparison comparison = StringComparison.Ordinal, string? message = null)
        => Append(new EndsWithRule<TContext>(suffix, comparison, message));

    public StringContextSchema<TContext> Contains(string substring, StringComparison comparison = StringComparison.Ordinal, string? message = null)
        => Append(new ContainsRule<TContext>(substring, comparison, message));

    public StringContextSchema<TContext> Regex(string pattern, string? message = null, string code = "regex")
    {
        var compiledRegex = new Regex(
            pattern,
            RegexOptions.Compiled,
            TimeSpan.FromSeconds(1));

        return Append(new RegexRule<TContext>(compiledRegex, message, code));
    }
}
