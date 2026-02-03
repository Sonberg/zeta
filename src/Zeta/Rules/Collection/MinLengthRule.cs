using System.Collections.Generic;
using Zeta.Core;

namespace Zeta.Rules.Collection;

/// <summary>
/// Validates that a collection has at least a minimum number of elements.
/// </summary>
public readonly struct MinLengthRule<T> : IValidationRule<ICollection<T>>
{
    private readonly int _min;
    private readonly string? _message;

    public MinLengthRule(int min, string? message = null)
    {
        _min = min;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(ICollection<T> value, ValidationContext context)
    {
        var error = value.Count >= _min
            ? null
            : new ValidationError(context.Path, "min_length", _message ?? $"Must have at least {_min} items");

        return ValueTaskHelper.FromResult(error);
    }
}

/// <summary>
/// Context-aware version: Validates that a collection has at least a minimum number of elements.
/// </summary>
public readonly struct MinLengthRule<T, TContext> : IValidationRule<ICollection<T>, TContext>
{
    private readonly int _min;
    private readonly string? _message;

    public MinLengthRule(int min, string? message = null)
    {
        _min = min;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(ICollection<T> value, ValidationContext<TContext> context)
    {
        var error = value.Count >= _min
            ? null
            : new ValidationError(context.Path, "min_length", _message ?? $"Must have at least {_min} items");

        return ValueTaskHelper.FromResult(error);
    }
}
