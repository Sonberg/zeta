namespace Zeta.Rules;

/// <summary>
/// A synchronous validation rule that wraps a delegate function.
/// </summary>
public readonly struct DelegateSyncRule<T, TContext> : ISyncRule<T, TContext>
{
    private readonly Func<T, ValidationContext<TContext>, ValidationError?> _validate;

    public DelegateSyncRule(Func<T, ValidationContext<TContext>, ValidationError?> validate)
    {
        _validate = validate;
    }

    public ValidationError? Validate(T value, ValidationContext<TContext> context)
    {
        return _validate(value, context);
    }
}
