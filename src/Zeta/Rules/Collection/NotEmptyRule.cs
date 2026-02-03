using System.Collections.Generic;
using Zeta.Core;

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
        var error = value.Count > 0
            ? null
            : new ValidationError(context.Path, "min_length", _message ?? "Must not be empty");

        return new ValueTask<ValidationError?>(error);
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
        var error = value.Count > 0
            ? null
            : new ValidationError(context.Path, "min_length", _message ?? "Must not be empty");

        return new ValueTask<ValidationError?>(error);
    }
}
