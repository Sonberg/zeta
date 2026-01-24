namespace Zeta.Rules;

public interface ISyncRule<in T, TContext>
{
    ValidationError? Validate(T value, ValidationContext<TContext> context);
}

/// <summary>
/// Alias for rules with no special context (Unit/Void).
/// </summary>
public interface ISyncRule<in T> : ISyncRule<T, object?>
{
}
