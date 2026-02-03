using System.Text.RegularExpressions;
using Zeta.Core;
using Zeta.Rules;
using Zeta.Rules.String;

namespace Zeta.Schemas;

/// <summary>
/// A context-aware schema for validating string values.
/// </summary>
public class StringContextSchema<TContext> : ContextSchema<string, TContext>
{
    public StringContextSchema() { }

    public StringContextSchema(ContextRuleEngine<string, TContext> rules) : base(rules) { }

    public StringContextSchema<TContext> MinLength(int min, string? message = null)
    {
        Use(new MinLengthRule<TContext>(min, message));
        return this;
    }

    public StringContextSchema<TContext> MaxLength(int max, string? message = null)
    {
        Use(new MaxLengthRule<TContext>(max, message));
        return this;
    }

    public StringContextSchema<TContext> Length(int exact, string? message = null)
    {
        Use(new LengthRule<TContext>(exact, message));
        return this;
    }

    public StringContextSchema<TContext> NotEmpty(string? message = null)
    {
        Use(new NotEmptyRule<TContext>(message));
        return this;
    }

    public StringContextSchema<TContext> Email(string? message = null)
    {
        Use(new EmailRule<TContext>(message));
        return this;
    }

    public StringContextSchema<TContext> Uuid(string? message = null)
    {
        Use(new UuidRule<TContext>(message));
        return this;
    }

    public StringContextSchema<TContext> Url(string? message = null)
    {
        Use(new UrlRule<TContext>(message));
        return this;
    }

    public StringContextSchema<TContext> Uri(UriKind kind = UriKind.Absolute, string? message = null)
    {
        Use(new UriRule<TContext>(kind, message));
        return this;
    }

    public StringContextSchema<TContext> Alphanumeric(string? message = null)
    {
        Use(new AlphanumericRule<TContext>(message));
        return this;
    }

    public StringContextSchema<TContext> StartsWith(string prefix, StringComparison comparison = StringComparison.Ordinal, string? message = null)
    {
        Use(new StartsWithRule<TContext>(prefix, comparison, message));
        return this;
    }

    public StringContextSchema<TContext> EndsWith(string suffix, StringComparison comparison = StringComparison.Ordinal, string? message = null)
    {
        Use(new EndsWithRule<TContext>(suffix, comparison, message));
        return this;
    }

    public StringContextSchema<TContext> Contains(string substring, StringComparison comparison = StringComparison.Ordinal, string? message = null)
    {
        Use(new ContainsRule<TContext>(substring, comparison, message));
        return this;
    }

    public StringContextSchema<TContext> Regex(string pattern, string? message = null, string code = "regex")
    {
        var compiledRegex = new Regex(
            pattern,
            RegexOptions.Compiled,
            TimeSpan.FromSeconds(1));

        Use(new RegexRule<TContext>(compiledRegex, message, code));
        return this;
    }

    public StringContextSchema<TContext> Refine(Func<string, TContext, bool> predicate, string message, string code = "custom_error")
    {
        Use(new RefinementRule<string, TContext>((val, ctx) =>
            predicate(val, ctx.Data)
                ? null
                : new ValidationError(ctx.Path, code, message)));
        return this;
    }

    public StringContextSchema<TContext> Refine(Func<string, bool> predicate, string message, string code = "custom_error")
    {
        return Refine((val, _) => predicate(val), message, code);
    }

    public StringContextSchema<TContext> RefineAsync(
        Func<string, TContext, CancellationToken, ValueTask<bool>> predicate,
        string message,
        string code = "custom_error")
    {
        Use(new RefinementRule<string, TContext>(async (val, ctx) =>
            await predicate(val, ctx.Data, ctx.CancellationToken)
                ? null
                : new ValidationError(ctx.Path, code, message)));
        return this;
    }

    public StringContextSchema<TContext> RefineAsync(
        Func<string, CancellationToken, ValueTask<bool>> predicate,
        string message,
        string code = "custom_error")
    {
        return RefineAsync((val, _, ct) => predicate(val, ct), message, code);
    }
}
