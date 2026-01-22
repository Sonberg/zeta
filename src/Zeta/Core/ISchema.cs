namespace Zeta;

/// <summary>
/// Defines a schema that can validate a value of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of value to validate.</typeparam>
public interface ISchema<T>
{
    /// <summary>
    /// Validates the given value asynchronously.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="context">The validation context (optional).</param>
    /// <returns>A result containing the value or validation errors.</returns>
    Task<Result<T>> ValidateAsync(T value, ValidationContext? context = null);
}
