using Zeta.Core;
using Zeta.Rules;
using Zeta.Schemas;

namespace Zeta;

/// <summary>
/// Guid validators. Work on both contextless and context-aware Guid schemas.
/// </summary>
public static class GuidSchemaExtensions
{
    /// <summary>Requires the value to not be <see cref="Guid.Empty"/>.</summary>
    public static TSelf NotEmpty<TSelf>(this IValueSchema<Guid, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<Guid, TSelf>
        => schema.AppendRule(new GuidNotEmptyRule(message));

    /// <summary>Requires the value to be an RFC 4122 GUID of the given <paramref name="version"/>.</summary>
    public static TSelf Version<TSelf>(this IValueSchema<Guid, TSelf> schema, int version, string? message = null)
        where TSelf : IValueSchema<Guid, TSelf>
        => schema.AppendRule(new GuidVersionRule(version, message));
}
