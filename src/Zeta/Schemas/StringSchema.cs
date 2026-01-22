using System.Text.RegularExpressions;

namespace Zeta.Schemas;

/// <summary>
/// A schema for validating string values with a specific context.
/// </summary>
public class StringSchema<TContext> : ISchema<string, TContext>
{
    private readonly List<IRule<string, TContext>> _rules = new();

    public async Task<Result<string>> ValidateAsync(string value, ValidationContext<TContext> context)
    {
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

    public StringSchema<TContext> Use(IRule<string, TContext> rule)
    {
        _rules.Add(rule);
        return this;
    }

    public StringSchema<TContext> MinLength(int min, string? message = null)
    {
        return Use(new DelegateRule<string, TContext>((val, ctx) =>
        {
            if (val.Length >= min) return ValueTask.FromResult<ValidationError?>(null);
            return ValueTask.FromResult<ValidationError?>(new ValidationError(
                ctx.Execution.Path, "min_length", message ?? $"Must be at least {min} characters long"));
        }));
    }

    public StringSchema<TContext> MaxLength(int max, string? message = null)
    {
        return Use(new DelegateRule<string, TContext>((val, ctx) =>
        {
            if (val.Length <= max) return ValueTask.FromResult<ValidationError?>(null);
            return ValueTask.FromResult<ValidationError?>(new ValidationError(
                ctx.Execution.Path, "max_length", message ?? $"Must be at most {max} characters long"));
        }));
    }

    public StringSchema<TContext> Email(string? message = null)
    {
        return Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", message ?? "Invalid email format", "email");
    }

    public StringSchema<TContext> Regex(string pattern, string? message = null, string code = "regex")
    {
        return Use(new DelegateRule<string, TContext>((val, ctx) =>
        {
             if (System.Text.RegularExpressions.Regex.IsMatch(val, pattern)) 
                 return ValueTask.FromResult<ValidationError?>(null);

             return ValueTask.FromResult<ValidationError?>(new ValidationError(
                ctx.Execution.Path, code, message ?? $"Must match pattern {pattern}"));
        }));
    }

    public StringSchema<TContext> NotEmpty(string? message = null)
    {
        return Use(new DelegateRule<string, TContext>((val, ctx) =>
        {
            if (!string.IsNullOrWhiteSpace(val)) return ValueTask.FromResult<ValidationError?>(null);
            return ValueTask.FromResult<ValidationError?>(new ValidationError(
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
            if (predicate(val, ctx.Data)) return ValueTask.FromResult<ValidationError?>(null);
            return ValueTask.FromResult<ValidationError?>(new ValidationError(ctx.Execution.Path, code, message));
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
    public Task<Result<string>> ValidateAsync(string value, ValidationExecutionContext? execution = null)
    {
        execution ??= ValidationExecutionContext.Empty;
        var context = new ValidationContext<object?>(null, execution);
        return ValidateAsync(value, context);
    }
}

internal sealed class DelegateRule<T, TContext> : IRule<T, TContext>
{
    private readonly Func<T, ValidationContext<TContext>, ValueTask<ValidationError?>> _validate;

    public DelegateRule(Func<T, ValidationContext<TContext>, ValueTask<ValidationError?>> validate)
    {
        _validate = validate;
    }

    public ValueTask<ValidationError?> ValidateAsync(T value, ValidationContext<TContext> context)
    {
        return _validate(value, context);
    }
}
