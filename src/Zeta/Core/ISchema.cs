using Zeta.Core;

namespace Zeta;

/// <summary>
/// Defines a schema that can validate a value of type <typeparamref name="T"/> with context <typeparamref name="TContext"/>.
/// </summary>
public interface ISchema<T, TContext>
{
    /// <summary>
    /// Validates the given value asynchronously.
    /// </summary>
    ValueTask<Result<T>> ValidateAsync(T value, ValidationContext<TContext> context);
}

/// <summary>
/// Alias for schemas with no special context.
/// </summary>
public interface ISchema<T> : ISchema<T, object?>
{
     ValueTask<Result<T>> ValidateAsync(T value, ValidationExecutionContext? execution = null);
}
