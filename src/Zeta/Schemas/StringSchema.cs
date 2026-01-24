using Zeta.Core;
using Zeta.Rules;

namespace Zeta.Schemas;

/// <summary>
/// A schema for validating string values with a specific context.
/// </summary>
public class StringSchema<TContext> : ISchema<string, TContext>
{
    private readonly List<IRule<string, TContext>> _rules = new();

    public async ValueTask<Result> ValidateAsync(string value, ValidationContext<TContext> context)
    {
        List<ValidationError>? errors = null;

        foreach (var rule in _rules)
        {
            var error = await rule.ValidateAsync(value, context);
            if (error == null) continue;
            errors ??= [];
            errors.Add(error);
        }

        return errors == null
            ? Result.Success()
            : Result.Failure(errors);
    }

    public StringSchema<TContext> Use(IRule<string, TContext> rule)
    {
        _rules.Add(rule);
        return this;
    }

    public StringSchema<TContext> MinLength(int min, string? message = null)
    {
        return Use(new DelegateRule<string, TContext>((val, ctx) =>
        {
            if (val.Length >= min) return ValueTaskHelper.NullError();
            return ValueTaskHelper.Error(new ValidationError(
                ctx.Execution.Path, "min_length", message ?? $"Must be at least {min} characters long"));
        }));
    }

    public StringSchema<TContext> MaxLength(int max, string? message = null)
    {
        return Use(new DelegateRule<string, TContext>((val, ctx) =>
        {
            if (val.Length <= max) return ValueTaskHelper.NullError();
            return ValueTaskHelper.Error(new ValidationError(
                ctx.Execution.Path, "max_length", message ?? $"Must be at most {max} characters long"));
        }));
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
        return Use(new DelegateRule<string, TContext>((val, ctx) =>
        {
            if (Guid.TryParse(val, out _)) return ValueTaskHelper.NullError();
            return ValueTaskHelper.Error(new ValidationError(
                ctx.Execution.Path, "uuid", message ?? "Invalid UUID format"));
        }));
    }

    /// <summary>
    /// Validates that the string is a valid absolute URL (http or https).
    /// </summary>
    public StringSchema<TContext> Url(string? message = null)
    {
        return Use(new DelegateRule<string, TContext>((val, ctx) =>
        {
            if (System.Uri.TryCreate(val, UriKind.Absolute, out var uri) &&
                (uri.Scheme == System.Uri.UriSchemeHttp || uri.Scheme == System.Uri.UriSchemeHttps))
                return ValueTaskHelper.NullError();
            return ValueTaskHelper.Error(new ValidationError(
                ctx.Execution.Path, "url", message ?? "Invalid URL format"));
        }));
    }

    /// <summary>
    /// Validates that the string is a valid URI with any scheme.
    /// </summary>
    public StringSchema<TContext> Uri(UriKind kind = UriKind.Absolute, string? message = null)
    {
        return Use(new DelegateRule<string, TContext>((val, ctx) =>
        {
            if (System.Uri.TryCreate(val, kind, out _)) return ValueTaskHelper.NullError();
            return ValueTaskHelper.Error(new ValidationError(
                ctx.Execution.Path, "uri", message ?? "Invalid URI format"));
        }));
    }

    /// <summary>
    /// Validates that the string contains only alphanumeric characters.
    /// </summary>
    public StringSchema<TContext> Alphanumeric(string? message = null)
    {
        return Use(new DelegateRule<string, TContext>((val, ctx) =>
        {
            if (val.All(char.IsLetterOrDigit)) return ValueTaskHelper.NullError();
            return ValueTaskHelper.Error(new ValidationError(
                ctx.Execution.Path, "alphanumeric", message ?? "Must contain only letters and numbers"));
        }));
    }

    /// <summary>
    /// Validates that the string starts with the specified prefix.
    /// </summary>
    public StringSchema<TContext> StartsWith(string prefix, StringComparison comparison = StringComparison.Ordinal, string? message = null)
    {
        return Use(new DelegateRule<string, TContext>((val, ctx) =>
        {
            if (val.StartsWith(prefix, comparison)) return ValueTaskHelper.NullError();
            return ValueTaskHelper.Error(new ValidationError(
                ctx.Execution.Path, "starts_with", message ?? $"Must start with '{prefix}'"));
        }));
    }

    /// <summary>
    /// Validates that the string ends with the specified suffix.
    /// </summary>
    public StringSchema<TContext> EndsWith(string suffix, StringComparison comparison = StringComparison.Ordinal, string? message = null)
    {
        return Use(new DelegateRule<string, TContext>((val, ctx) =>
        {
            if (val.EndsWith(suffix, comparison)) return ValueTaskHelper.NullError();
            return ValueTaskHelper.Error(new ValidationError(
                ctx.Execution.Path, "ends_with", message ?? $"Must end with '{suffix}'"));
        }));
    }

    /// <summary>
    /// Validates that the string contains the specified substring.
    /// </summary>
    public StringSchema<TContext> Contains(string substring, StringComparison comparison = StringComparison.Ordinal, string? message = null)
    {
        return Use(new DelegateRule<string, TContext>((val, ctx) =>
        {
            if (val.IndexOf(substring, comparison) >= 0) return ValueTaskHelper.NullError();
            return ValueTaskHelper.Error(new ValidationError(
                ctx.Execution.Path, "contains", message ?? $"Must contain '{substring}'"));
        }));
    }

    /// <summary>
    /// Validates that the string has an exact length.
    /// </summary>
    public StringSchema<TContext> Length(int exact, string? message = null)
    {
        return Use(new DelegateRule<string, TContext>((val, ctx) =>
        {
            if (val.Length == exact) return ValueTaskHelper.NullError();
            return ValueTaskHelper.Error(new ValidationError(
                ctx.Execution.Path, "length", message ?? $"Must be exactly {exact} characters long"));
        }));
    }

    public StringSchema<TContext> Regex(string pattern, string? message = null, string code = "regex")
    {
        var compiledRegex = new System.Text.RegularExpressions.Regex(
            pattern,
            System.Text.RegularExpressions.RegexOptions.Compiled,
            TimeSpan.FromSeconds(1));

        return Use(new DelegateRule<string, TContext>((val, ctx) =>
        {
            if (compiledRegex.IsMatch(val))
                return ValueTaskHelper.NullError();

            return ValueTaskHelper.Error(new ValidationError(
                ctx.Execution.Path, code, message ?? $"Must match pattern {pattern}"));
        }));
    }

    public StringSchema<TContext> NotEmpty(string? message = null)
    {
        return Use(new DelegateRule<string, TContext>((val, ctx) =>
        {
            if (!string.IsNullOrWhiteSpace(val)) return ValueTaskHelper.NullError();
            return ValueTaskHelper.Error(new ValidationError(
                ctx.Execution.Path, "required", message ?? "Value cannot be empty"));
        }));
    }

    /// <summary>
    /// Refines validation using context.
    /// </summary>
    public StringSchema<TContext> Refine(Func<string, TContext, bool> predicate, string message, string code = "custom_error")
    {
        return Use(new DelegateRule<string, TContext>((val, ctx) =>
        {
            if (predicate(val, ctx.Data)) return ValueTaskHelper.NullError();
            return ValueTaskHelper.Error(new ValidationError(ctx.Execution.Path, code, message));
        }));
    }

    // Kept for backward compatibility logic or simple checks
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

internal sealed class DelegateAsyncRule<T, TContext> : IAsyncRule<T, TContext>
{
    private readonly Func<T, ValidationContext<TContext>, ValueTask<ValidationError?>> _validate;

    public DelegateAsyncRule(Func<T, ValidationContext<TContext>, ValueTask<ValidationError?>> validate)
    {
        _validate = validate;
    }

    public ValueTask<ValidationError?> ValidateAsync(T value, ValidationContext<TContext> context)
    {
        return _validate(value, context);
    }
}

internal sealed class DelegateSyncRule<T, TContext> : ISyncRule<T, TContext>
{
    private readonly Func<T, ValidationContext<TContext>, ValidationError?> _validate;

    public DelegateSyncRule(Func<T, ValidationContext<TContext>, ValidationError?> validate)
    {
        _validate = validate;
    }

    public ValidationError? Validate(T value, ValidationContext<TContext> context)
    {
        return _validate(value, context);
    }
}