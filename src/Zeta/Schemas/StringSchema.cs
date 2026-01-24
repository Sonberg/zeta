using Zeta.Core;
using Zeta.Rules;

namespace Zeta.Schemas;

/// <summary>
/// A schema for validating string values with a specific context.
/// </summary>
public class StringSchema<TContext> : BaseSchema<string, TContext>
{
    public StringSchema<TContext> MinLength(int min, string? message = null)
    {
        Use(new DelegateSyncRule<string, TContext>((val, ctx) =>
            val.Length >= min
                ? null
                : new ValidationError(ctx.Execution.Path, "min_length", message ?? $"Must be at least {min} characters long")));
        return this;
    }

    public StringSchema<TContext> MaxLength(int max, string? message = null)
    {
        Use(new DelegateSyncRule<string, TContext>((val, ctx) =>
            val.Length <= max
                ? null
                : new ValidationError(ctx.Execution.Path, "max_length", message ?? $"Must be at most {max} characters long")));
        return this;
    }

    public StringSchema<TContext> Email(string? message = null)
    {
        return Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", message ?? "Invalid email format", "email");
    }

    /// <summary>
    /// Validates that the string is a valid UUID/GUID.
    /// </summary>
    public StringSchema<TContext> Uuid(string? message = null)
    {
        Use(new DelegateSyncRule<string, TContext>((val, ctx) =>
            Guid.TryParse(val, out _)
                ? null
                : new ValidationError(ctx.Execution.Path, "uuid", message ?? "Invalid UUID format")));
        return this;
    }

    /// <summary>
    /// Validates that the string is a valid absolute URL (http or https).
    /// </summary>
    public StringSchema<TContext> Url(string? message = null)
    {
        Use(new DelegateSyncRule<string, TContext>((val, ctx) =>
            System.Uri.TryCreate(val, UriKind.Absolute, out var uri) &&
            (uri.Scheme == System.Uri.UriSchemeHttp || uri.Scheme == System.Uri.UriSchemeHttps)
                ? null
                : new ValidationError(ctx.Execution.Path, "url", message ?? "Invalid URL format")));
        return this;
    }

    /// <summary>
    /// Validates that the string is a valid URI with any scheme.
    /// </summary>
    public StringSchema<TContext> Uri(UriKind kind = UriKind.Absolute, string? message = null)
    {
        Use(new DelegateSyncRule<string, TContext>((val, ctx) =>
            System.Uri.TryCreate(val, kind, out _)
                ? null
                : new ValidationError(ctx.Execution.Path, "uri", message ?? "Invalid URI format")));
        return this;
    }

    /// <summary>
    /// Validates that the string contains only alphanumeric characters.
    /// </summary>
    public StringSchema<TContext> Alphanumeric(string? message = null)
    {
        Use(new DelegateSyncRule<string, TContext>((val, ctx) =>
            val.All(char.IsLetterOrDigit)
                ? null
                : new ValidationError(ctx.Execution.Path, "alphanumeric", message ?? "Must contain only letters and numbers")));
        return this;
    }

    /// <summary>
    /// Validates that the string starts with the specified prefix.
    /// </summary>
    public StringSchema<TContext> StartsWith(string prefix, StringComparison comparison = StringComparison.Ordinal, string? message = null)
    {
        Use(new DelegateSyncRule<string, TContext>((val, ctx) =>
            val.StartsWith(prefix, comparison)
                ? null
                : new ValidationError(ctx.Execution.Path, "starts_with", message ?? $"Must start with '{prefix}'")));
        return this;
    }

    /// <summary>
    /// Validates that the string ends with the specified suffix.
    /// </summary>
    public StringSchema<TContext> EndsWith(string suffix, StringComparison comparison = StringComparison.Ordinal, string? message = null)
    {
        Use(new DelegateSyncRule<string, TContext>((val, ctx) =>
            val.EndsWith(suffix, comparison)
                ? null
                : new ValidationError(ctx.Execution.Path, "ends_with", message ?? $"Must end with '{suffix}'")));
        return this;
    }

    /// <summary>
    /// Validates that the string contains the specified substring.
    /// </summary>
    public StringSchema<TContext> Contains(string substring, StringComparison comparison = StringComparison.Ordinal, string? message = null)
    {
        Use(new DelegateSyncRule<string, TContext>((val, ctx) =>
            val.IndexOf(substring, comparison) >= 0
                ? null
                : new ValidationError(ctx.Execution.Path, "contains", message ?? $"Must contain '{substring}'")));
        return this;
    }

    /// <summary>
    /// Validates that the string has an exact length.
    /// </summary>
    public StringSchema<TContext> Length(int exact, string? message = null)
    {
        Use(new DelegateSyncRule<string, TContext>((val, ctx) =>
            val.Length == exact
                ? null
                : new ValidationError(ctx.Execution.Path, "length", message ?? $"Must be exactly {exact} characters long")));
        return this;
    }

    public StringSchema<TContext> Regex(string pattern, string? message = null, string code = "regex")
    {
        var compiledRegex = new System.Text.RegularExpressions.Regex(
            pattern,
            System.Text.RegularExpressions.RegexOptions.Compiled,
            TimeSpan.FromSeconds(1));

        Use(new DelegateSyncRule<string, TContext>((val, ctx) =>
            compiledRegex.IsMatch(val)
                ? null
                : new ValidationError(ctx.Execution.Path, code, message ?? $"Must match pattern {pattern}")));
        return this;
    }

    public StringSchema<TContext> NotEmpty(string? message = null)
    {
        Use(new DelegateSyncRule<string, TContext>((val, ctx) =>
            !string.IsNullOrWhiteSpace(val)
                ? null
                : new ValidationError(ctx.Execution.Path, "required", message ?? "Value cannot be empty")));
        return this;
    }

    /// <summary>
    /// Refines validation using context.
    /// </summary>
    public StringSchema<TContext> Refine(Func<string, TContext, bool> predicate, string message, string code = "custom_error")
    {
        Use(new DelegateSyncRule<string, TContext>((val, ctx) =>
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

/// <summary>
/// A schema for validating string values with default context.
/// </summary>
public sealed class StringSchema : StringSchema<object?>, ISchema<string>
{
    public async ValueTask<Result<string>> ValidateAsync(string value, ValidationExecutionContext? execution = null)
    {
        execution ??= ValidationExecutionContext.Empty;
        var context = new ValidationContext<object?>(null, execution);
        var result = await ValidateAsync(value, context);

        return result.IsSuccess
            ? Result<string>.Success(value)
            : Result<string>.Failure(result.Errors);
    }
}
