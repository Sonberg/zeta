using FluentValidation.Results;

namespace Zeta.FastEndpoints;

/// <summary>
/// Extensions for shaping Zeta <see cref="ValidationError"/>s into FastEndpoints validation failures.
/// </summary>
public static class ValidationErrorExtensions
{
    /// <summary>
    /// Projects Zeta errors into <see cref="ValidationFailure"/>s, preserving path, message and error code.
    /// </summary>
    public static IEnumerable<ValidationFailure> ToValidationFailures(this IEnumerable<ValidationError> errors)
        => errors.Select(e => new ValidationFailure(e.PathString, e.Message) { ErrorCode = e.Code });
}
