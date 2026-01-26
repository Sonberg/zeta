namespace Zeta.Rules;

/// <summary>
/// A context-aware synchronous validation rule.
/// </summary>
public interface ISyncRule<in T, TContext>
{
    ValidationError? Validate(T value, ValidationContext<TContext> context);
}
