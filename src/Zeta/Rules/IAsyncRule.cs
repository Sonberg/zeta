namespace Zeta.Rules;

public interface IAsyncRule<in T, TContext>
{
    ValueTask<ValidationError?> ValidateAsync(T value, ValidationContext<TContext> context);
}


/// <summary>
/// Alias for rules with no special context (Unit/Void).
/// </summary>
public interface IAsyncRule<in T> : IAsyncRule<T, object?>
{
}