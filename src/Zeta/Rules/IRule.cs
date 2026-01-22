namespace Zeta;

/// <summary>
/// Defines a single validation rule for type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of value to validate.</typeparam>
public interface IRule<in T>
{
    /// <summary>
    /// Validates the value against this rule.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="context">The current validation context.</param>
    /// <returns>A validation error if the rule fails, or null if it succeeds.</returns>
    ValueTask<ValidationError?> ValidateAsync(T value, ValidationContext context);
}
