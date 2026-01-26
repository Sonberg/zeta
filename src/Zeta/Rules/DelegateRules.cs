namespace Zeta.Rules;

/// <summary>
/// A context-aware synchronous validation rule that wraps a delegate function.
/// </summary>
public readonly struct DelegateValidationRule<T, TContext> : IValidationRule<T, TContext>
{
    private readonly Func<T, ValidationContext<TContext>, ValidationError?> _validate;

    public DelegateValidationRule(Func<T, ValidationContext<TContext>, ValidationError?> validate)
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
public readonly struct DelegateAsyncValidationRule<T, TContext> : IAsyncValidationRule<T, TContext>
{
    private readonly Func<T, ValidationContext<TContext>, ValueTask<ValidationError?>> _validate;

    public DelegateAsyncValidationRule(Func<T, ValidationContext<TContext>, ValueTask<ValidationError?>> validate)
    {
        _validate = validate;
    }

    public ValueTask<ValidationError?> ValidateAsync(T value, ValidationContext<TContext> context)
    {
        return _validate(value, context);
    }
}
