namespace Zeta.Rules;

/// <summary>
/// An asynchronous validation rule that wraps a delegate function.
/// </summary>
public readonly struct DelegateAsyncRule<T, TContext> : IAsyncRule<T, TContext>
{
    private readonly Func<T, ValidationContext<TContext>, ValueTask<ValidationError?>> _validate;

    public DelegateAsyncRule(Func<T, ValidationContext<TContext>, ValueTask<ValidationError?>> validate)
    {
        _validate = validate;
    }

    public ValueTask<ValidationError?> ValidateAsync(T value, ValidationContext<TContext> context)
    {
        return _validate(value, context);
    }
}