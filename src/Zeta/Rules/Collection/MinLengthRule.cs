using System.Collections.Generic;
using Zeta.Core;
using Zeta.Validation;

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
        return new ValueTask<ValidationError?>(
            CollectionValidators.MinLength(value, _min, context.Path, _message));
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
        return new ValueTask<ValidationError?>(
            CollectionValidators.MinLength(value, _min, context.Path, _message));
    }
}
