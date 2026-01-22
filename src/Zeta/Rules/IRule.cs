namespace Zeta;

/// <summary>
/// Defines a single validation rule for type <typeparamref name="T"/> with context <typeparamref name="TContext"/>.
/// </summary>
public interface IRule<in T, TContext>
{
    /// <summary>
    /// Validates the value against this rule.
    /// </summary>
    ValueTask<ValidationError?> ValidateAsync(T value, ValidationContext<TContext> context);
}

/// <summary>
/// Alias for rules with no special context (Unit/Void).
/// </summary>
public interface IRule<in T> : IRule<T, object?>
{
}
