using System.Text.RegularExpressions;
using Zeta.Core;
using Zeta.Rules;
using Zeta.Validation;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating string values.
/// </summary>
public sealed class StringContextlessSchema : ContextlessSchema<string>
{
    public StringContextlessSchema() { }

    public StringContextlessSchema MinLength(int min, string? message = null)
    {
        Use(new RefinementRule<string>((val, exec) =>
            StringValidators.MinLength(val, min, exec.Path, message)));
        return this;
    }

    public StringContextlessSchema MaxLength(int max, string? message = null)
    {
        Use(new RefinementRule<string>((val, exec) =>
            StringValidators.MaxLength(val, max, exec.Path, message)));
        return this;
    }

    public StringContextlessSchema Length(int exact, string? message = null)
    {
        Use(new RefinementRule<string>((val, exec) =>
            StringValidators.Length(val, exact, exec.Path, message)));
        return this;
    }

    public StringContextlessSchema NotEmpty(string? message = null)
    {
        Use(new RefinementRule<string>((val, exec) =>
            StringValidators.NotEmpty(val, exec.Path, message)));
        return this;
    }

    public StringContextlessSchema Email(string? message = null)
    {
        Use(new RefinementRule<string>((val, exec) =>
            StringValidators.Email(val, exec.Path, message)));
        return this;
    }

    public StringContextlessSchema Uuid(string? message = null)
    {
        Use(new RefinementRule<string>((val, exec) =>
            StringValidators.Uuid(val, exec.Path, message)));
        return this;
    }

    public StringContextlessSchema Url(string? message = null)
    {
        Use(new RefinementRule<string>((val, exec) =>
            StringValidators.Url(val, exec.Path, message)));
        return this;
    }

    public StringContextlessSchema Uri(UriKind kind = UriKind.Absolute, string? message = null)
    {
        Use(new RefinementRule<string>((val, exec) =>
            StringValidators.ValidUri(val, kind, exec.Path, message)));
        return this;
    }

    public StringContextlessSchema Alphanumeric(string? message = null)
    {
        Use(new RefinementRule<string>((val, exec) =>
            StringValidators.Alphanumeric(val, exec.Path, message)));
        return this;
    }

    public StringContextlessSchema StartsWith(string prefix, StringComparison comparison = StringComparison.Ordinal, string? message = null)
    {
        Use(new RefinementRule<string>((val, exec) =>
            StringValidators.StartsWith(val, prefix, comparison, exec.Path, message)));
        return this;
    }

    public StringContextlessSchema EndsWith(string suffix, StringComparison comparison = StringComparison.Ordinal, string? message = null)
    {
        Use(new RefinementRule<string>((val, exec) =>
            StringValidators.EndsWith(val, suffix, comparison, exec.Path, message)));
        return this;
    }

    public StringContextlessSchema Contains(string substring, StringComparison comparison = StringComparison.Ordinal, string? message = null)
    {
        Use(new RefinementRule<string>((val, exec) =>
            StringValidators.Contains(val, substring, comparison, exec.Path, message)));
        return this;
    }

    public StringContextlessSchema Regex(string pattern, string? message = null, string code = "regex")
    {
        var compiledRegex = new Regex(
            pattern,
            RegexOptions.Compiled,
            TimeSpan.FromSeconds(1));

        Use(new RefinementRule<string>((val, exec) =>
            StringValidators.MatchesRegex(val, compiledRegex, exec.Path, message, code)));
        return this;
    }

    public StringContextlessSchema Refine(Func<string, bool> predicate, string message, string code = "custom_error")
    {
        Use(new RefinementRule<string>((val, exec) =>
            predicate(val)
                ? null
                : new ValidationError(exec.Path, code, message)));
        return this;
    }

    public StringContextlessSchema RefineAsync(
        Func<string, CancellationToken, ValueTask<bool>> predicate,
        string message,
        string code = "custom_error")
    {
        Use(new RefinementRule<string>(async (val, exec) =>
            await predicate(val, exec.CancellationToken)
                ? null
                : new ValidationError(exec.Path, code, message)));
        return this;
    }

    /// <summary>
    /// Creates a context-aware string schema with all rules from this schema.
    /// </summary>
    public StringContextSchema<TContext> WithContext<TContext>()
        => new StringContextSchema<TContext>(Rules.ToContext<TContext>());
}
