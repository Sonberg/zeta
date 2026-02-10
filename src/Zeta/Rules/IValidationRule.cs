using Zeta.Core;

namespace Zeta.Rules;

/// <summary>
/// A context-free validation rule. Async-first design.
/// These rules only need ValidationExecutionContext for path tracking.
/// </summary>
public interface IValidationRule<in T>
{
    /// <summary>
    /// Validates the given value asynchronously and returns a validation error if validation fails.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="context">The validation context.</param>
    /// <returns>A validation error if validation fails, otherwise null.</returns>
    ValueTask<ValidationError?> ValidateAsync(T value, ValidationContext context);
}

/// <summary>
/// A context-aware validation rule. Async-first design.
/// </summary>
public interface IValidationRule<in T, TContext>
{
    /// <summary>
    /// Validates the given value asynchronously with context and returns a validation error if validation fails.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="context">The validation context with typed data.</param>
    /// <returns>A validation error if validation fails, otherwise null.</returns>
    ValueTask<ValidationError?> ValidateAsync(T value, ValidationContext<TContext> context);
}
