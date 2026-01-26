using Zeta.Core;

namespace Zeta.Rules;

/// <summary>
/// A context-free synchronous validation rule that wraps a delegate function.
/// </summary>
public readonly struct DelegateValidationRule<T> : IValidationRule<T>
{
    private readonly Func<T, ValidationExecutionContext, ValidationError?> _validate;

    public DelegateValidationRule(Func<T, ValidationExecutionContext, ValidationError?> validate)
    {
        _validate = validate;
    }

    public ValidationError? Validate(T value, ValidationExecutionContext execution)
    {
        return _validate(value, execution);
    }
}

/// <summary>
/// A context-free asynchronous validation rule that wraps a delegate function.
/// </summary>
public readonly struct DelegateAsyncValidationRule<T> : IAsyncValidationRule<T>
{
    private readonly Func<T, ValidationExecutionContext, ValueTask<ValidationError?>> _validate;

    public DelegateAsyncValidationRule(Func<T, ValidationExecutionContext, ValueTask<ValidationError?>> validate)
    {
        _validate = validate;
    }

    public ValueTask<ValidationError?> ValidateAsync(T value, ValidationExecutionContext execution)
    {
        return _validate(value, execution);
    }
}
