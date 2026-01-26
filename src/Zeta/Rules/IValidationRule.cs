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
