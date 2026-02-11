using Zeta.Core;
using Zeta.Rules;

namespace Zeta.Schemas;

/// <summary>
/// A context-aware schema for validating Guid values.
/// </summary>
public class GuidContextSchema<TContext> : ContextSchema<Guid, TContext, GuidContextSchema<TContext>>
{
    internal GuidContextSchema(ContextRuleEngine<Guid, TContext> rules) : base(rules)
    {
    }

    public GuidContextSchema<TContext> NotEmpty(string? message = null)
    {
        Use(new RefinementRule<Guid, TContext>((val, ctx) =>
            val != Guid.Empty
                ? null
                : new ValidationError(ctx.Path, "not_empty", message ?? "GUID cannot be empty")));
        return this;
    }

    public GuidContextSchema<TContext> Version(int version, string? message = null)
    {
        Use(new RefinementRule<Guid, TContext>((val, ctx) =>
        {
            var bytes = val.ToByteArray();
            var guidVersion = (bytes[7] >> 4) & 0x0F;
            return guidVersion == version
                ? null
                : new ValidationError(ctx.Path, "version", message ?? $"GUID must be version {version}");
        }));
        return this;
    }
}