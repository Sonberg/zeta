namespace Zeta.Rules;

/// <summary>
/// A context-aware synchronous validation rule that wraps a delegate function.
/// </summary>
public readonly struct DelegateContextRule<T, TContext> : IContextRule<T, TContext>
{
    private readonly Func<T, ValidationContext<TContext>, ValidationError?> _validate;

    public DelegateContextRule(Func<T, ValidationContext<TContext>, ValidationError?> validate)
    {
        _validate = validate;
    }

    public ValidationError? Validate(T value, ValidationContext<TContext> context)
    {
        return _validate(value, context);
    }
}

/// <summary>
/// A context-aware asynchronous validation rule that wraps a delegate function.
/// </summary>
public readonly struct DelegateAsyncContextRule<T, TContext> : IAsyncContextRule<T, TContext>
{
    private readonly Func<T, ValidationContext<TContext>, ValueTask<ValidationError?>> _validate;

    public DelegateAsyncContextRule(Func<T, ValidationContext<TContext>, ValueTask<ValidationError?>> validate)
    {
        _validate = validate;
    }

    public ValueTask<ValidationError?> ValidateAsync(T value, ValidationContext<TContext> context)
    {
        return _validate(value, context);
    }
}
