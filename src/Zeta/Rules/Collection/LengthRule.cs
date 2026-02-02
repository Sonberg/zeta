using System.Collections.Generic;
using Zeta.Core;
using Zeta.Validation;

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
        return new ValueTask<ValidationError?>(
            CollectionValidators.Length(value, _exact, context.Path, _message));
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
        return new ValueTask<ValidationError?>(
            CollectionValidators.Length(value, _exact, context.Path, _message));
    }
}
