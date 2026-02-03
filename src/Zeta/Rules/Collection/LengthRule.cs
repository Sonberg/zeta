using System.Collections.Generic;
using Zeta.Core;

namespace Zeta.Rules.Collection;

/// <summary>
/// Validates that a collection has an exact number of elements.
/// </summary>
public readonly struct LengthRule<T> : IValidationRule<ICollection<T>>
{
    private readonly int _exact;
    private readonly string? _message;

    public LengthRule(int exact, string? message = null)
    {
        _exact = exact;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(ICollection<T> value, ValidationContext context)
    {
        var error = value.Count == _exact
            ? null
            : new ValidationError(context.Path, "length", _message ?? $"Must have exactly {_exact} items");

        return new ValueTask<ValidationError?>(error);
    }
}

/// <summary>
/// Context-aware version: Validates that a collection has an exact number of elements.
/// </summary>
public readonly struct LengthRule<T, TContext> : IValidationRule<ICollection<T>, TContext>
{
    private readonly int _exact;
    private readonly string? _message;

    public LengthRule(int exact, string? message = null)
    {
        _exact = exact;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(ICollection<T> value, ValidationContext<TContext> context)
    {
        var error = value.Count == _exact
            ? null
            : new ValidationError(context.Path, "length", _message ?? $"Must have exactly {_exact} items");

        return new ValueTask<ValidationError?>(error);
    }
}
