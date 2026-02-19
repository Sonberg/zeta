using System.Collections.Generic;
using Zeta.Core;

namespace Zeta.Rules.Dictionary;

/// <summary>
/// Validates that a dictionary has at least a minimum number of entries.
/// </summary>
public readonly struct DictionaryMinLengthRule<TKey, TValue> : IValidationRule<IDictionary<TKey, TValue>>
    where TKey : notnull
{
    private readonly int _min;
    private readonly string? _message;

    public DictionaryMinLengthRule(int min, string? message = null)
    {
        _min = min;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(IDictionary<TKey, TValue> value, ValidationContext context)
    {
        var error = value.Count >= _min
            ? null
            : new ValidationError(context.Path, "min_length", _message ?? $"Must have at least {_min} entries");

        return ValueTaskHelper.FromResult(error);
    }
}

/// <summary>
/// Context-aware: Validates that a dictionary has at least a minimum number of entries.
/// </summary>
public readonly struct DictionaryMinLengthRule<TKey, TValue, TContext> : IValidationRule<IDictionary<TKey, TValue>, TContext>
    where TKey : notnull
{
    private readonly int _min;
    private readonly string? _message;

    public DictionaryMinLengthRule(int min, string? message = null)
    {
        _min = min;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(IDictionary<TKey, TValue> value, ValidationContext<TContext> context)
    {
        var error = value.Count >= _min
            ? null
            : new ValidationError(context.Path, "min_length", _message ?? $"Must have at least {_min} entries");

        return ValueTaskHelper.FromResult(error);
    }
}

/// <summary>
/// Validates that a dictionary does not exceed a maximum number of entries.
/// </summary>
public readonly struct DictionaryMaxLengthRule<TKey, TValue> : IValidationRule<IDictionary<TKey, TValue>>
    where TKey : notnull
{
    private readonly int _max;
    private readonly string? _message;

    public DictionaryMaxLengthRule(int max, string? message = null)
    {
        _max = max;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(IDictionary<TKey, TValue> value, ValidationContext context)
    {
        var error = value.Count <= _max
            ? null
            : new ValidationError(context.Path, "max_length", _message ?? $"Must have at most {_max} entries");

        return ValueTaskHelper.FromResult(error);
    }
}

/// <summary>
/// Context-aware: Validates that a dictionary does not exceed a maximum number of entries.
/// </summary>
public readonly struct DictionaryMaxLengthRule<TKey, TValue, TContext> : IValidationRule<IDictionary<TKey, TValue>, TContext>
    where TKey : notnull
{
    private readonly int _max;
    private readonly string? _message;

    public DictionaryMaxLengthRule(int max, string? message = null)
    {
        _max = max;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(IDictionary<TKey, TValue> value, ValidationContext<TContext> context)
    {
        var error = value.Count <= _max
            ? null
            : new ValidationError(context.Path, "max_length", _message ?? $"Must have at most {_max} entries");

        return ValueTaskHelper.FromResult(error);
    }
}

/// <summary>
/// Validates that a dictionary is not empty.
/// </summary>
public readonly struct DictionaryNotEmptyRule<TKey, TValue> : IValidationRule<IDictionary<TKey, TValue>>
    where TKey : notnull
{
    private readonly string? _message;

    public DictionaryNotEmptyRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(IDictionary<TKey, TValue> value, ValidationContext context)
    {
        var error = value.Count > 0
            ? null
            : new ValidationError(context.Path, "min_length", _message ?? "Must not be empty");

        return ValueTaskHelper.FromResult(error);
    }
}

/// <summary>
/// Context-aware: Validates that a dictionary is not empty.
/// </summary>
public readonly struct DictionaryNotEmptyRule<TKey, TValue, TContext> : IValidationRule<IDictionary<TKey, TValue>, TContext>
    where TKey : notnull
{
    private readonly string? _message;

    public DictionaryNotEmptyRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(IDictionary<TKey, TValue> value, ValidationContext<TContext> context)
    {
        var error = value.Count > 0
            ? null
            : new ValidationError(context.Path, "min_length", _message ?? "Must not be empty");

        return ValueTaskHelper.FromResult(error);
    }
}
