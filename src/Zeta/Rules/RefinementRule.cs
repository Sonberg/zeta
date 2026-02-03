using Zeta.Core;

namespace Zeta.Rules;

/// <summary>
/// A context-free validation rule that wraps a delegate function.
/// Supports both sync and async delegates via constructor overloads.
/// </summary>
public readonly struct RefinementRule<T> : IValidationRule<T>
{
    private readonly Func<T, ValidationContext, ValueTask<ValidationError?>> _validate;

    /// <summary>
    /// Creates a rule from a synchronous delegate.
    /// </summary>
    public RefinementRule(Func<T, ValidationContext, ValidationError?> validate)
    {
        _validate = (val, exec) => ValueTaskHelper.FromResult(validate(val, exec));
    }

    /// <summary>
    /// Creates a rule from an asynchronous delegate.
    /// </summary>
    public RefinementRule(Func<T, ValidationContext, ValueTask<ValidationError?>> validate)
    {
        _validate = validate;
    }

    public ValueTask<ValidationError?> ValidateAsync(T value, ValidationContext context)
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
    private readonly Func<T, ValidationContext<TContext>, ValueTask<ValidationError?>> _validate;

    /// <summary>
    /// Creates a rule from a synchronous delegate.
    /// </summary>
    public RefinementRule(Func<T, ValidationContext<TContext>, ValidationError?> validate)
    {
        _validate = (val, ctx) => ValueTaskHelper.FromResult(validate(val, ctx));
    }

    /// <summary>
    /// Creates a rule from an asynchronous delegate.
    /// </summary>
    public RefinementRule(Func<T, ValidationContext<TContext>, ValueTask<ValidationError?>> validate)
    {
        _validate = validate;
    }

    public ValueTask<ValidationError?> ValidateAsync(T value, ValidationContext<TContext> context)
    {
        return _validate(value, context);
    }
}
