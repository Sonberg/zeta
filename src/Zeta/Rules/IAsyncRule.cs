namespace Zeta.Rules;

/// <summary>
/// A context-aware asynchronous validation rule.
/// </summary>
public interface IAsyncRule<in T, TContext>
{
    ValueTask<ValidationError?> ValidateAsync(T value, ValidationContext<TContext> context);
}
