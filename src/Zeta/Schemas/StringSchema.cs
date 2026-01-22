using System.Text.RegularExpressions;

namespace Zeta.Schemas;

/// <summary>
/// A schema for validating string values.
/// </summary>
public sealed class StringSchema : ISchema<string>
{
    private readonly List<IRule<string>> _rules = new();

    /// <inheritdoc />
    public async Task<Result<string>> ValidateAsync(string value, ValidationContext? context = null)
    {
        context ??= ValidationContext.Empty;
        var errors = new List<ValidationError>();

        foreach (var rule in _rules)
        {
            var error = await rule.ValidateAsync(value, context);
            if (error != null)
            {
                errors.Add(error);
            }
        }

        return errors.Count == 0
            ? Result<string>.Success(value)
            : Result<string>.Failure(errors);
    }

    /// <summary>
    /// Adds a custom rule to the schema.
    /// </summary>
    public StringSchema Use(IRule<string> rule)
    {
        _rules.Add(rule);
        return this;
    }

    /// <summary>
    /// Adds a rule that requires the string to have a minimum length.
    /// </summary>
    public StringSchema MinLength(int min, string? message = null)
    {
        return Use(new DelegateRule<string>((val, ctx) =>
        {
            if (val.Length >= min) return ValueTask.FromResult<ValidationError?>(null);
            
            return ValueTask.FromResult<ValidationError?>(new ValidationError(
                ctx.Path, 
                "min_length", 
                message ?? $"Must be at least {min} characters long"));
        }));
    }

    /// <summary>
    /// Adds a rule that requires the string to have a maximum length.
    /// </summary>
    public StringSchema MaxLength(int max, string? message = null)
    {
         return Use(new DelegateRule<string>((val, ctx) =>
        {
            if (val.Length <= max) return ValueTask.FromResult<ValidationError?>(null);
            
            return ValueTask.FromResult<ValidationError?>(new ValidationError(
                ctx.Path, 
                "max_length", 
                message ?? $"Must be at most {max} characters long"));
        }));
    }

    /// <summary>
    /// Adds a rule that requires the string to be a valid email address.
    /// </summary>
    public StringSchema Email(string? message = null)
    {
        // Simple regex for email validation
        return Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", message ?? "Invalid email format", "email");
    }

    /// <summary>
    /// Adds a rule that requires the string to match a regular expression.
    /// </summary>
    public StringSchema Regex(string pattern, string? message = null, string code = "regex")
    {
        return Use(new DelegateRule<string>((val, ctx) =>
        {
             if (System.Text.RegularExpressions.Regex.IsMatch(val, pattern)) 
                 return ValueTask.FromResult<ValidationError?>(null);

             return ValueTask.FromResult<ValidationError?>(new ValidationError(
                ctx.Path, 
                code, 
                message ?? $"Must match pattern {pattern}"));
        }));
    }

    /// <summary>
    /// Adds a rule that requires the string to be not empty or whitespace.
    /// </summary>
    public StringSchema NotEmpty(string? message = null)
    {
        return Use(new DelegateRule<string>((val, ctx) =>
        {
            if (!string.IsNullOrWhiteSpace(val)) return ValueTask.FromResult<ValidationError?>(null);

            return ValueTask.FromResult<ValidationError?>(new ValidationError(
                ctx.Path,
                "required",
                message ?? "Value cannot be empty"));
        }));
    }

     /// <summary>
    /// Refines the validation with a custom synchronous predicate.
    /// </summary>
    public StringSchema Refine(Func<string, bool> predicate, string message, string code = "custom_error")
    {
        return Use(new DelegateRule<string>((val, ctx) =>
        {
            if (predicate(val)) return ValueTask.FromResult<ValidationError?>(null);

            return ValueTask.FromResult<ValidationError?>(new ValidationError(ctx.Path, code, message));
        }));
    }

    /// <summary>
    /// Refines the validation with a custom asynchronous predicate.
    /// </summary>
    public StringSchema RefineAsync(Func<string, ValidationContext, Task<bool>> predicate, string message, string code = "custom_error")
    {
        return Use(new DelegateRule<string>(async (val, ctx) =>
        {
            if (await predicate(val, ctx)) return null;

            return new ValidationError(ctx.Path, code, message);
        }));
    }
}

// Simple delegate rule helper
internal sealed class DelegateRule<T> : IRule<T>
{
    private readonly Func<T, ValidationContext, ValueTask<ValidationError?>> _validate;

    public DelegateRule(Func<T, ValidationContext, ValueTask<ValidationError?>> validate)
    {
        _validate = validate;
    }

    public ValueTask<ValidationError?> ValidateAsync(T value, ValidationContext context)
    {
        return _validate(value, context);
    }
}
