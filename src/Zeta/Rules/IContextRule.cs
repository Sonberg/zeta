namespace Zeta.Rules;

/// <summary>
/// A context-aware synchronous validation rule.
/// </summary>
public interface IContextRule<in T, TContext>
{
    ValidationError? Validate(T value, ValidationContext<TContext> context);
}

/// <summary>
/// A context-aware asynchronous validation rule.
/// </summary>
public interface IAsyncContextRule<in T, TContext>
{
    ValueTask<ValidationError?> ValidateAsync(T value, ValidationContext<TContext> context);
}
