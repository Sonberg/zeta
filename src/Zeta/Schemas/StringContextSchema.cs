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
        Use(new StatefulRefinementRule<string, TContext, (int, string?)>(
            static (val, ctx, state) => StringValidators.MinLength(val, state.Item1, ctx.Path, state.Item2),
            (min, message)));
        return this;
    }

    public StringContextSchema<TContext> MaxLength(int max, string? message = null)
    {
        Use(new StatefulRefinementRule<string, TContext, (int, string?)>(
            static (val, ctx, state) => StringValidators.MaxLength(val, state.Item1, ctx.Path, state.Item2),
            (max, message)));
        return this;
    }

    public StringContextSchema<TContext> Length(int exact, string? message = null)
    {
        Use(new StatefulRefinementRule<string, TContext, (int, string?)>(
            static (val, ctx, state) => StringValidators.Length(val, state.Item1, ctx.Path, state.Item2),
            (exact, message)));
        return this;
    }

    public StringContextSchema<TContext> NotEmpty(string? message = null)
    {
        Use(new StatefulRefinementRule<string, TContext, string?>(
            static (val, ctx, state) => StringValidators.NotEmpty(val, ctx.Path, state),
            message));
        return this;
    }

    public StringContextSchema<TContext> Email(string? message = null)
    {
        Use(new StatefulRefinementRule<string, TContext, string?>(
            static (val, ctx, state) => StringValidators.Email(val, ctx.Path, state),
            message));
        return this;
    }

    public StringContextSchema<TContext> Uuid(string? message = null)
    {
        Use(new StatefulRefinementRule<string, TContext, string?>(
            static (val, ctx, state) => StringValidators.Uuid(val, ctx.Path, state),
            message));
        return this;
    }

    public StringContextSchema<TContext> Url(string? message = null)
    {
        Use(new StatefulRefinementRule<string, TContext, string?>(
            static (val, ctx, state) => StringValidators.Url(val, ctx.Path, state),
            message));
        return this;
    }

    public StringContextSchema<TContext> Uri(UriKind kind = UriKind.Absolute, string? message = null)
    {
        Use(new StatefulRefinementRule<string, TContext, (UriKind, string?)>(
            static (val, ctx, state) => StringValidators.ValidUri(val, state.Item1, ctx.Path, state.Item2),
            (kind, message)));
        return this;
    }

    public StringContextSchema<TContext> Alphanumeric(string? message = null)
    {
        Use(new StatefulRefinementRule<string, TContext, string?>(
            static (val, ctx, state) => StringValidators.Alphanumeric(val, ctx.Path, state),
            message));
        return this;
    }

    public StringContextSchema<TContext> StartsWith(string prefix, StringComparison comparison = StringComparison.Ordinal, string? message = null)
    {
        Use(new StatefulRefinementRule<string, TContext, (string, StringComparison, string?)>(
            static (val, ctx, state) => StringValidators.StartsWith(val, state.Item1, state.Item2, ctx.Path, state.Item3),
            (prefix, comparison, message)));
        return this;
    }

    public StringContextSchema<TContext> EndsWith(string suffix, StringComparison comparison = StringComparison.Ordinal, string? message = null)
    {
        Use(new StatefulRefinementRule<string, TContext, (string, StringComparison, string?)>(
            static (val, ctx, state) => StringValidators.EndsWith(val, state.Item1, state.Item2, ctx.Path, state.Item3),
            (suffix, comparison, message)));
        return this;
    }

    public StringContextSchema<TContext> Contains(string substring, StringComparison comparison = StringComparison.Ordinal, string? message = null)
    {
        Use(new StatefulRefinementRule<string, TContext, (string, StringComparison, string?)>(
            static (val, ctx, state) => StringValidators.Contains(val, state.Item1, state.Item2, ctx.Path, state.Item3),
            (substring, comparison, message)));
        return this;
    }

    public StringContextSchema<TContext> Regex(string pattern, string? message = null, string code = "regex")
    {
        var compiledRegex = new Regex(
            pattern,
            RegexOptions.Compiled,
            TimeSpan.FromSeconds(1));

        Use(new StatefulRefinementRule<string, TContext, (System.Text.RegularExpressions.Regex, string?, string)>(
            static (val, ctx, state) => StringValidators.MatchesRegex(val, state.Item1, ctx.Path, state.Item2, state.Item3),
            (compiledRegex, message, code)));
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
