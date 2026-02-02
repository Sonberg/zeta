using System.Collections.Generic;
using Zeta.Core;
using Zeta.Validation;

namespace Zeta.Rules.Collection;

/// <summary>
/// Validates that a collection does not exceed a maximum number of elements.
/// </summary>
public readonly struct MaxLengthRule<T> : IValidationRule<ICollection<T>>
{
    private readonly int _max;
    private readonly string? _message;

    public MaxLengthRule(int max, string? message = null)
    {
        _max = max;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(ICollection<T> value, ValidationContext context)
    {
        return new ValueTask<ValidationError?>(
            CollectionValidators.MaxLength(value, _max, context.Path, _message));
    }
}

/// <summary>
/// Context-aware version: Validates that a collection does not exceed a maximum number of elements.
/// </summary>
public readonly struct MaxLengthRule<T, TContext> : IValidationRule<ICollection<T>, TContext>
{
    private readonly int _max;
    private readonly string? _message;

    public MaxLengthRule(int max, string? message = null)
    {
        _max = max;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(ICollection<T> value, ValidationContext<TContext> context)
    {
        return new ValueTask<ValidationError?>(
            CollectionValidators.MaxLength(value, _max, context.Path, _message));
    }
}
