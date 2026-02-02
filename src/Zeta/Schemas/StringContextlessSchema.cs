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
        Use(new StatefulRefinementRule<string, (int, string?)>(
            static (val, exec, state) => StringValidators.MinLength(val, state.Item1, exec.Path, state.Item2),
            (min, message)));
        return this;
    }

    public StringContextlessSchema MaxLength(int max, string? message = null)
    {
        Use(new StatefulRefinementRule<string, (int, string?)>(
            static (val, exec, state) => StringValidators.MaxLength(val, state.Item1, exec.Path, state.Item2),
            (max, message)));
        return this;
    }

    public StringContextlessSchema Length(int exact, string? message = null)
    {
        Use(new StatefulRefinementRule<string, (int, string?)>(
            static (val, exec, state) => StringValidators.Length(val, state.Item1, exec.Path, state.Item2),
            (exact, message)));
        return this;
    }

    public StringContextlessSchema NotEmpty(string? message = null)
    {
        Use(new StatefulRefinementRule<string, string?>(
            static (val, exec, state) => StringValidators.NotEmpty(val, exec.Path, state),
            message));
        return this;
    }

    public StringContextlessSchema Email(string? message = null)
    {
        Use(new StatefulRefinementRule<string, string?>(
            static (val, exec, state) => StringValidators.Email(val, exec.Path, state),
            message));
        return this;
    }

    public StringContextlessSchema Uuid(string? message = null)
    {
        Use(new StatefulRefinementRule<string, string?>(
            static (val, exec, state) => StringValidators.Uuid(val, exec.Path, state),
            message));
        return this;
    }

    public StringContextlessSchema Url(string? message = null)
    {
        Use(new StatefulRefinementRule<string, string?>(
            static (val, exec, state) => StringValidators.Url(val, exec.Path, state),
            message));
        return this;
    }

    public StringContextlessSchema Uri(UriKind kind = UriKind.Absolute, string? message = null)
    {
        Use(new StatefulRefinementRule<string, (UriKind, string?)>(
            static (val, exec, state) => StringValidators.ValidUri(val, state.Item1, exec.Path, state.Item2),
            (kind, message)));
        return this;
    }

    public StringContextlessSchema Alphanumeric(string? message = null)
    {
        Use(new StatefulRefinementRule<string, string?>(
            static (val, exec, state) => StringValidators.Alphanumeric(val, exec.Path, state),
            message));
        return this;
    }

    public StringContextlessSchema StartsWith(string prefix, StringComparison comparison = StringComparison.Ordinal, string? message = null)
    {
        Use(new StatefulRefinementRule<string, (string, StringComparison, string?)>(
            static (val, exec, state) => StringValidators.StartsWith(val, state.Item1, state.Item2, exec.Path, state.Item3),
            (prefix, comparison, message)));
        return this;
    }

    public StringContextlessSchema EndsWith(string suffix, StringComparison comparison = StringComparison.Ordinal, string? message = null)
    {
        Use(new StatefulRefinementRule<string, (string, StringComparison, string?)>(
            static (val, exec, state) => StringValidators.EndsWith(val, state.Item1, state.Item2, exec.Path, state.Item3),
            (suffix, comparison, message)));
        return this;
    }

    public StringContextlessSchema Contains(string substring, StringComparison comparison = StringComparison.Ordinal, string? message = null)
    {
        Use(new StatefulRefinementRule<string, (string, StringComparison, string?)>(
            static (val, exec, state) => StringValidators.Contains(val, state.Item1, state.Item2, exec.Path, state.Item3),
            (substring, comparison, message)));
        return this;
    }

    public StringContextlessSchema Regex(string pattern, string? message = null, string code = "regex")
    {
        var compiledRegex = new Regex(
            pattern,
            RegexOptions.Compiled,
            TimeSpan.FromSeconds(1));

        Use(new StatefulRefinementRule<string, (System.Text.RegularExpressions.Regex, string?, string)>(
            static (val, exec, state) => StringValidators.MatchesRegex(val, state.Item1, exec.Path, state.Item2, state.Item3),
            (compiledRegex, message, code)));
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
