using Zeta.Core;

namespace Zeta.Rules;

/// <summary>
/// A context-free validation rule that wraps a delegate function.
/// Supports both sync and async delegates via constructor overloads.
/// </summary>
public readonly struct RefinementRule<T> : IValidationRule<T>
{
    private readonly Func<T, ValidationRun, ValueTask<ValidationError?>> _validate;

    /// <summary>
    /// Creates a rule from a synchronous delegate.
    /// </summary>
    public RefinementRule(Func<T, ValidationRun, ValidationError?> validate)
    {
        _validate = (val, exec) => ValueTask.FromResult(validate(val, exec));
    }

    /// <summary>
    /// Creates a rule from an asynchronous delegate.
    /// </summary>
    public RefinementRule(Func<T, ValidationRun, ValueTask<ValidationError?>> validate)
    {
        _validate = validate;
    }

    public ValueTask<ValidationError?> ValidateAsync(T value, ValidationRun context)
    {
        return _validate(value, context);
    }
}

/// <summary>
/// A context-aware validation rule that wraps a delegate function.
/// Supports both sync and async delegates via constructor overloads.
/// </summary>
public readonly struct RefinementRule<T, TContext> : IValidationRule<T, TContext>
{
    private readonly Func<T, ValidationRun<TContext>, ValueTask<ValidationError?>> _validate;

    /// <summary>
    /// Creates a rule from a synchronous delegate.
    /// </summary>
    public RefinementRule(Func<T, ValidationRun<TContext>, ValidationError?> validate)
    {
        _validate = (val, ctx) => ValueTask.FromResult(validate(val, ctx));
    }

    /// <summary>
    /// Creates a rule from an asynchronous delegate.
    /// </summary>
    public RefinementRule(Func<T, ValidationRun<TContext>, ValueTask<ValidationError?>> validate)
    {
        _validate = validate;
    }

    public ValueTask<ValidationError?> ValidateAsync(T value, ValidationRun<TContext> context)
    {
        return _validate(value, context);
    }
}
