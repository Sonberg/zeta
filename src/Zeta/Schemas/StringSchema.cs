using System.Text.RegularExpressions;
using Zeta.Core;
using Zeta.Rules;
using Zeta.Validation;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating string values.
/// </summary>
public sealed class StringSchema : ContextlessSchema<string>
{
    public StringSchema MinLength(int min, string? message = null)
    {
        Use(new RefinementRule<string>((val, exec) =>
            StringValidators.MinLength(val, min, exec.Path, message)));
        return this;
    }

    public StringSchema MaxLength(int max, string? message = null)
    {
        Use(new RefinementRule<string>((val, exec) =>
            StringValidators.MaxLength(val, max, exec.Path, message)));
        return this;
    }

    public StringSchema Length(int exact, string? message = null)
    {
        Use(new RefinementRule<string>((val, exec) =>
            StringValidators.Length(val, exact, exec.Path, message)));
        return this;
    }

    public StringSchema NotEmpty(string? message = null)
    {
        Use(new RefinementRule<string>((val, exec) =>
            StringValidators.NotEmpty(val, exec.Path, message)));
        return this;
    }

    public StringSchema Email(string? message = null)
    {
        Use(new RefinementRule<string>((val, exec) =>
            StringValidators.Email(val, exec.Path, message)));
        return this;
    }

    public StringSchema Uuid(string? message = null)
    {
        Use(new RefinementRule<string>((val, exec) =>
            StringValidators.Uuid(val, exec.Path, message)));
        return this;
    }

    public StringSchema Url(string? message = null)
    {
        Use(new RefinementRule<string>((val, exec) =>
            StringValidators.Url(val, exec.Path, message)));
        return this;
    }

    public StringSchema Uri(UriKind kind = UriKind.Absolute, string? message = null)
    {
        Use(new RefinementRule<string>((val, exec) =>
            StringValidators.ValidUri(val, kind, exec.Path, message)));
        return this;
    }

    public StringSchema Alphanumeric(string? message = null)
    {
        Use(new RefinementRule<string>((val, exec) =>
            StringValidators.Alphanumeric(val, exec.Path, message)));
        return this;
    }

    public StringSchema StartsWith(string prefix, StringComparison comparison = StringComparison.Ordinal, string? message = null)
    {
        Use(new RefinementRule<string>((val, exec) =>
            StringValidators.StartsWith(val, prefix, comparison, exec.Path, message)));
        return this;
    }

    public StringSchema EndsWith(string suffix, StringComparison comparison = StringComparison.Ordinal, string? message = null)
    {
        Use(new RefinementRule<string>((val, exec) =>
            StringValidators.EndsWith(val, suffix, comparison, exec.Path, message)));
        return this;
    }

    public StringSchema Contains(string substring, StringComparison comparison = StringComparison.Ordinal, string? message = null)
    {
        Use(new RefinementRule<string>((val, exec) =>
            StringValidators.Contains(val, substring, comparison, exec.Path, message)));
        return this;
    }

    public StringSchema Regex(string pattern, string? message = null, string code = "regex")
    {
        var compiledRegex = new Regex(
            pattern,
            RegexOptions.Compiled,
            TimeSpan.FromSeconds(1));

        Use(new RefinementRule<string>((val, exec) =>
            StringValidators.MatchesRegex(val, compiledRegex, exec.Path, message, code)));
        return this;
    }

    public StringSchema Refine(Func<string, bool> predicate, string message, string code = "custom_error")
    {
        Use(new RefinementRule<string>((val, exec) =>
            predicate(val)
                ? null
                : new ValidationError(exec.Path, code, message)));
        return this;
    }
}

/// <summary>
/// A context-aware schema for validating string values.
/// </summary>
public class StringSchema<TContext> : ContextSchema<string, TContext>
{
    public StringSchema<TContext> MinLength(int min, string? message = null)
    {
        Use(new RefinementRule<string, TContext>((val, ctx) =>
            StringValidators.MinLength(val, min, ctx.Execution.Path, message)));
        return this;
    }

    public StringSchema<TContext> MaxLength(int max, string? message = null)
    {
        Use(new RefinementRule<string, TContext>((val, ctx) =>
            StringValidators.MaxLength(val, max, ctx.Execution.Path, message)));
        return this;
    }

    public StringSchema<TContext> Length(int exact, string? message = null)
    {
        Use(new RefinementRule<string, TContext>((val, ctx) =>
            StringValidators.Length(val, exact, ctx.Execution.Path, message)));
        return this;
    }

    public StringSchema<TContext> NotEmpty(string? message = null)
    {
        Use(new RefinementRule<string, TContext>((val, ctx) =>
            StringValidators.NotEmpty(val, ctx.Execution.Path, message)));
        return this;
    }

    public StringSchema<TContext> Email(string? message = null)
    {
        Use(new RefinementRule<string, TContext>((val, ctx) =>
            StringValidators.Email(val, ctx.Execution.Path, message)));
        return this;
    }

    public StringSchema<TContext> Uuid(string? message = null)
    {
        Use(new RefinementRule<string, TContext>((val, ctx) =>
            StringValidators.Uuid(val, ctx.Execution.Path, message)));
        return this;
    }

    public StringSchema<TContext> Url(string? message = null)
    {
        Use(new RefinementRule<string, TContext>((val, ctx) =>
            StringValidators.Url(val, ctx.Execution.Path, message)));
        return this;
    }

    public StringSchema<TContext> Uri(UriKind kind = UriKind.Absolute, string? message = null)
    {
        Use(new RefinementRule<string, TContext>((val, ctx) =>
            StringValidators.ValidUri(val, kind, ctx.Execution.Path, message)));
        return this;
    }

    public StringSchema<TContext> Alphanumeric(string? message = null)
    {
        Use(new RefinementRule<string, TContext>((val, ctx) =>
            StringValidators.Alphanumeric(val, ctx.Execution.Path, message)));
        return this;
    }

    public StringSchema<TContext> StartsWith(string prefix, StringComparison comparison = StringComparison.Ordinal, string? message = null)
    {
        Use(new RefinementRule<string, TContext>((val, ctx) =>
            StringValidators.StartsWith(val, prefix, comparison, ctx.Execution.Path, message)));
        return this;
    }

    public StringSchema<TContext> EndsWith(string suffix, StringComparison comparison = StringComparison.Ordinal, string? message = null)
    {
        Use(new RefinementRule<string, TContext>((val, ctx) =>
            StringValidators.EndsWith(val, suffix, comparison, ctx.Execution.Path, message)));
        return this;
    }

    public StringSchema<TContext> Contains(string substring, StringComparison comparison = StringComparison.Ordinal, string? message = null)
    {
        Use(new RefinementRule<string, TContext>((val, ctx) =>
            StringValidators.Contains(val, substring, comparison, ctx.Execution.Path, message)));
        return this;
    }

    public StringSchema<TContext> Regex(string pattern, string? message = null, string code = "regex")
    {
        var compiledRegex = new Regex(
            pattern,
            RegexOptions.Compiled,
            TimeSpan.FromSeconds(1));

        Use(new RefinementRule<string, TContext>((val, ctx) =>
            StringValidators.MatchesRegex(val, compiledRegex, ctx.Execution.Path, message, code)));
        return this;
    }

    public StringSchema<TContext> Refine(Func<string, TContext, bool> predicate, string message, string code = "custom_error")
    {
        Use(new RefinementRule<string, TContext>((val, ctx) =>
            predicate(val, ctx.Data)
                ? null
                : new ValidationError(ctx.Execution.Path, code, message)));
        return this;
    }

    public StringSchema<TContext> Refine(Func<string, bool> predicate, string message, string code = "custom_error")
    {
        return Refine((val, _) => predicate(val), message, code);
    }
}