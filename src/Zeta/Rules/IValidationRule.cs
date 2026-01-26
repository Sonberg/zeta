using Zeta.Core;

namespace Zeta.Rules;

/// <summary>
/// A context-free validation rule. Async-first design.
/// These rules only need ValidationExecutionContext for path tracking.
/// </summary>
public interface IValidationRule<in T>
{
    ValueTask<ValidationError?> ValidateAsync(T value, ValidationExecutionContext execution);
}

/// <summary>
/// A context-aware validation rule. Async-first design.
/// </summary>
public interface IValidationRule<in T, TContext>
{
    ValueTask<ValidationError?> ValidateAsync(T value, ValidationContext<TContext> context);
}
