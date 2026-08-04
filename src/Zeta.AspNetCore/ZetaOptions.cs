using Microsoft.AspNetCore.Http;

namespace Zeta.AspNetCore;

/// <summary>
/// Options for Zeta's ASP.NET Core integration, configured via <c>AddZeta(options => ...)</c>.
/// </summary>
public sealed class ZetaOptions
{
    /// <summary>
    /// Maps validation errors to the failure <see cref="IResult"/> returned by the endpoint filters
    /// registered through <c>WithValidation(...)</c>. Defaults to RFC 7807 <c>Results.ValidationProblem</c>.
    /// Set this to emit a stable custom error envelope during a migration.
    /// </summary>
    public Func<IReadOnlyList<ValidationError>, IResult> ValidationResultFactory { get; set; }
        = errors => Results.ValidationProblem(errors.ToPathDictionary());
}
