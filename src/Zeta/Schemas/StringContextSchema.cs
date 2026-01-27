using System.Text.RegularExpressions;
using Zeta.Core;
using Zeta.Rules;
using Zeta.Validation;

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
        Use(new RefinementRule<string, TContext>((val, ctx) =>
            StringValidators.MinLength(val, min, ctx.Path, message)));
        return this;
    }

    public StringContextSchema<TContext> MaxLength(int max, string? message = null)
    {
        Use(new RefinementRule<string, TContext>((val, ctx) =>
            StringValidators.MaxLength(val, max, ctx.Path, message)));
        return this;
    }

    public StringContextSchema<TContext> Length(int exact, string? message = null)
    {
        Use(new RefinementRule<string, TContext>((val, ctx) =>
            StringValidators.Length(val, exact, ctx.Path, message)));
        return this;
    }

    public StringContextSchema<TContext> NotEmpty(string? message = null)
    {
        Use(new RefinementRule<string, TContext>((val, ctx) =>
            StringValidators.NotEmpty(val, ctx.Path, message)));
        return this;
    }

    public StringContextSchema<TContext> Email(string? message = null)
    {
        Use(new RefinementRule<string, TContext>((val, ctx) =>
            StringValidators.Email(val, ctx.Path, message)));
        return this;
    }

    public StringContextSchema<TContext> Uuid(string? message = null)
    {
        Use(new RefinementRule<string, TContext>((val, ctx) =>
            StringValidators.Uuid(val, ctx.Path, message)));
        return this;
    }

    public StringContextSchema<TContext> Url(string? message = null)
    {
        Use(new RefinementRule<string, TContext>((val, ctx) =>
            StringValidators.Url(val, ctx.Path, message)));
        return this;
    }

    public StringContextSchema<TContext> Uri(UriKind kind = UriKind.Absolute, string? message = null)
    {
        Use(new RefinementRule<string, TContext>((val, ctx) =>
            StringValidators.ValidUri(val, kind, ctx.Path, message)));
        return this;
    }

    public StringContextSchema<TContext> Alphanumeric(string? message = null)
    {
        Use(new RefinementRule<string, TContext>((val, ctx) =>
            StringValidators.Alphanumeric(val, ctx.Path, message)));
        return this;
    }

    public StringContextSchema<TContext> StartsWith(string prefix, StringComparison comparison = StringComparison.Ordinal, string? message = null)
    {
        Use(new RefinementRule<string, TContext>((val, ctx) =>
            StringValidators.StartsWith(val, prefix, comparison, ctx.Path, message)));
        return this;
    }

    public StringContextSchema<TContext> EndsWith(string suffix, StringComparison comparison = StringComparison.Ordinal, string? message = null)
    {
        Use(new RefinementRule<string, TContext>((val, ctx) =>
            StringValidators.EndsWith(val, suffix, comparison, ctx.Path, message)));
        return this;
    }

    public StringContextSchema<TContext> Contains(string substring, StringComparison comparison = StringComparison.Ordinal, string? message = null)
    {
        Use(new RefinementRule<string, TContext>((val, ctx) =>
            StringValidators.Contains(val, substring, comparison, ctx.Path, message)));
        return this;
    }

    public StringContextSchema<TContext> Regex(string pattern, string? message = null, string code = "regex")
    {
        var compiledRegex = new Regex(
            pattern,
            RegexOptions.Compiled,
            TimeSpan.FromSeconds(1));

        Use(new RefinementRule<string, TContext>((val, ctx) =>
            StringValidators.MatchesRegex(val, compiledRegex, ctx.Path, message, code)));
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
