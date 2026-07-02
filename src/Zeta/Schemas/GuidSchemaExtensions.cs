using Zeta.Core;
using Zeta.Rules;
using Zeta.Schemas;

namespace Zeta;

/// <summary>
/// Guid validators. Work on both contextless and context-aware Guid schemas.
/// </summary>
public static class GuidSchemaExtensions
{
    public static TSelf NotEmpty<TSelf>(this IValueSchema<Guid, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<Guid, TSelf>
        => schema.AppendRule(new RefinementRule<Guid>((val, exec) =>
            val != Guid.Empty
                ? null
                : new ValidationError(exec.PathSegments, "not_empty", message ?? "GUID cannot be empty")));

    public static TSelf Version<TSelf>(this IValueSchema<Guid, TSelf> schema, int version, string? message = null)
        where TSelf : IValueSchema<Guid, TSelf>
        => schema.AppendRule(new RefinementRule<Guid>((val, exec) =>
        {
            var bytes = val.ToByteArray();
            var guidVersion = (bytes[7] >> 4) & 0x0F;
            return guidVersion == version
                ? null
                : new ValidationError(exec.PathSegments, "version", message ?? $"GUID must be version {version}");
        }));
}
