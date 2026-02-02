using System.Collections.Generic;
using Zeta.Core;
using Zeta.Validation;

namespace Zeta.Rules.Collection;

/// <summary>
/// Validates that a collection is not empty.
/// </summary>
public readonly struct NotEmptyRule<T> : IValidationRule<ICollection<T>>
{
    private readonly string? _message;

    public NotEmptyRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(ICollection<T> value, ValidationContext context)
    {
        return new ValueTask<ValidationError?>(
            CollectionValidators.NotEmpty(value, context.Path, _message));
    }
}

/// <summary>
/// Context-aware version: Validates that a collection is not empty.
/// </summary>
public readonly struct NotEmptyRule<T, TContext> : IValidationRule<ICollection<T>, TContext>
{
    private readonly string? _message;

    public NotEmptyRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(ICollection<T> value, ValidationContext<TContext> context)
    {
        return new ValueTask<ValidationError?>(
            CollectionValidators.NotEmpty(value, context.Path, _message));
    }
}
