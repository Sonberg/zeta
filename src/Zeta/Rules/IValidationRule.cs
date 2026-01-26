using Zeta.Core;

namespace Zeta.Rules;

/// <summary>
/// A context-free synchronous validation rule.
/// These rules only need ValidationExecutionContext for path tracking.
/// </summary>
public interface IValidationRule<in T>
{
    ValidationError? Validate(T value, ValidationExecutionContext execution);
}

/// <summary>
/// A context-free asynchronous validation rule.
/// </summary>
public interface IAsyncValidationRule<in T>
{
    ValueTask<ValidationError?> ValidateAsync(T value, ValidationExecutionContext execution);
}

/// <summary>
/// A context-aware synchronous validation rule.
/// </summary>
public interface IValidationRule<in T, TContext>
{
    ValidationError? Validate(T value, ValidationContext<TContext> context);
}

/// <summary>
/// A context-aware asynchronous validation rule.
/// </summary>
public interface IAsyncValidationRule<in T, TContext>
{
    ValueTask<ValidationError?> ValidateAsync(T value, ValidationContext<TContext> context);
}
