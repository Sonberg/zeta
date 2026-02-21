using Zeta.Core;

namespace Zeta.Rules.String;

/// <summary>
/// Validates that a string starts with a specified prefix.
/// </summary>
public readonly struct StartsWithRule : IValidationRule<string>
{
    private readonly string _prefix;
    private readonly StringComparison _comparison;
    private readonly string? _message;

    public StartsWithRule(string prefix, StringComparison comparison = StringComparison.Ordinal, string? message = null)
    {
        _prefix = prefix;
        _comparison = comparison;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(string value, ValidationContext context)
    {
        var error = value.StartsWith(_prefix, _comparison)
            ? null
            : new ValidationError(context.PathSegments, "starts_with", _message ?? $"Must start with '{_prefix}'");

        return ValueTaskHelper.FromResult(error);
    }
}

/// <summary>
/// Context-aware version: Validates that a string starts with a specified prefix.
/// </summary>
public readonly struct StartsWithRule<TContext> : IValidationRule<string, TContext>
{
    private readonly string _prefix;
    private readonly StringComparison _comparison;
    private readonly string? _message;

    public StartsWithRule(string prefix, StringComparison comparison = StringComparison.Ordinal, string? message = null)
    {
        _prefix = prefix;
        _comparison = comparison;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(string value, ValidationContext<TContext> context)
    {
        var error = value.StartsWith(_prefix, _comparison)
            ? null
            : new ValidationError(context.PathSegments, "starts_with", _message ?? $"Must start with '{_prefix}'");

        return ValueTaskHelper.FromResult(error);
    }
}
