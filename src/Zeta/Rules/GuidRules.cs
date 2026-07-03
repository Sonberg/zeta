using Zeta.Core;

namespace Zeta.Rules;

/// <summary>
/// Validates that a Guid value is not empty.
/// </summary>
public readonly struct GuidNotEmptyRule : IValidationRule<Guid>
{
    private readonly string? _message;

    public GuidNotEmptyRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(Guid value, ValidationContext context)
    {
        var error = value != Guid.Empty
            ? null
            : new ValidationError(context.PathSegments, "not_empty", _message ?? "GUID cannot be empty");
        return ValueTask.FromResult(error);
    }
}

/// <summary>
/// Validates that a Guid value is of a specific RFC 4122 version.
/// </summary>
public readonly struct GuidVersionRule : IValidationRule<Guid>
{
    private readonly int _version;
    private readonly string? _message;

    public GuidVersionRule(int version, string? message = null)
    {
        _version = version;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(Guid value, ValidationContext context)
    {
        var bytes = value.ToByteArray();
        var guidVersion = (bytes[7] >> 4) & 0x0F;
        var error = guidVersion == _version
            ? null
            : new ValidationError(context.PathSegments, "version", _message ?? $"GUID must be version {_version}");
        return ValueTask.FromResult(error);
    }
}
