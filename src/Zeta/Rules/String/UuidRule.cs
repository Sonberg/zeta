using Zeta.Core;

namespace Zeta.Rules.String;

/// <summary>
/// Validates that a string is a valid UUID.
/// </summary>
public readonly struct UuidRule : IValidationRule<string>
{
    private readonly string? _message;

    public UuidRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(string value, ValidationContext context)
    {
        var error = Guid.TryParse(value, out _)
            ? null
            : new ValidationError(context.PathSegments, "uuid", _message ?? "Invalid UUID format");

        return ValueTaskHelper.FromResult(error);
    }
}

/// <summary>
/// Context-aware version: Validates that a string is a valid UUID.
/// </summary>
public readonly struct UuidRule<TContext> : IValidationRule<string, TContext>
{
    private readonly string? _message;

    public UuidRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(string value, ValidationContext<TContext> context)
    {
        var error = Guid.TryParse(value, out _)
            ? null
            : new ValidationError(context.PathSegments, "uuid", _message ?? "Invalid UUID format");

        return ValueTaskHelper.FromResult(error);
    }
}
