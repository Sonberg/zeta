using Zeta.Core;

namespace Zeta.Rules;

/// <summary>
/// A stateful context-free validation rule that uses static delegates with state to avoid closure allocations.
/// Supports both sync and async delegates via constructor overloads.
/// </summary>
public readonly struct StatefulRefinementRule<T, TState> : IValidationRule<T>
{
    private readonly Func<T, ValidationContext, TState, ValueTask<ValidationError?>> _validate;
    private readonly TState _state;

    /// <summary>
    /// Creates a stateful rule from a synchronous static delegate.
    /// </summary>
    public StatefulRefinementRule(Func<T, ValidationContext, TState, ValidationError?> validate, TState state)
    {
        _validate = (val, exec, s) => ValueTaskHelper.FromResult(validate(val, exec, s));
        _state = state;
    }

    /// <summary>
    /// Creates a stateful rule from an asynchronous static delegate.
    /// </summary>
    public StatefulRefinementRule(Func<T, ValidationContext, TState, ValueTask<ValidationError?>> validate, TState state)
    {
        _validate = validate;
        _state = state;
    }

    public ValueTask<ValidationError?> ValidateAsync(T value, ValidationContext context)
    {
        return _validate(value, context, _state);
    }
}

/// <summary>
/// A stateful context-aware validation rule that uses static delegates with state to avoid closure allocations.
/// Supports both sync and async delegates via constructor overloads.
/// </summary>
public readonly struct StatefulRefinementRule<T, TContext, TState> : IValidationRule<T, TContext>
{
    private readonly Func<T, ValidationContext<TContext>, TState, ValueTask<ValidationError?>> _validate;
    private readonly TState _state;

    /// <summary>
    /// Creates a stateful rule from a synchronous static delegate.
    /// </summary>
    public StatefulRefinementRule(Func<T, ValidationContext<TContext>, TState, ValidationError?> validate, TState state)
    {
        _validate = (val, ctx, s) => ValueTaskHelper.FromResult(validate(val, ctx, s));
        _state = state;
    }

    /// <summary>
    /// Creates a stateful rule from an asynchronous static delegate.
    /// </summary>
    public StatefulRefinementRule(Func<T, ValidationContext<TContext>, TState, ValueTask<ValidationError?>> validate, TState state)
    {
        _validate = validate;
        _state = state;
    }

    public ValueTask<ValidationError?> ValidateAsync(T value, ValidationContext<TContext> context)
    {
        return _validate(value, context, _state);
    }
}
