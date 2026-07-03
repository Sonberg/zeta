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

    public ValueTask<ValidationError?> ValidateAsync(ICollection<T> value, ValidationRun context)
    {
        var error = value.Count == _exact
            ? null
            : new ValidationError(context.PathSegments, "length", _message ?? $"Must have exactly {_exact} items");

        return ValueTask.FromResult(error);
    }
}
