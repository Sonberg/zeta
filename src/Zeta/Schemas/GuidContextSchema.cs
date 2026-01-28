using Zeta.Core;
using Zeta.Rules;

namespace Zeta.Schemas;

/// <summary>
/// A context-aware schema for validating Guid values.
/// </summary>
public class GuidContextSchema<TContext> : ContextSchema<Guid, TContext>
{
    public GuidContextSchema() { }

    public GuidContextSchema(ContextRuleEngine<Guid, TContext> rules) : base(rules) { }

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

    public GuidContextSchema<TContext> Refine(Func<Guid, TContext, bool> predicate, string message, string code = "custom_error")
    {
        Use(new RefinementRule<Guid, TContext>((val, ctx) =>
            predicate(val, ctx.Data)
                ? null
                : new ValidationError(ctx.Path, code, message)));
        return this;
    }

    public GuidContextSchema<TContext> Refine(Func<Guid, bool> predicate, string message, string code = "custom_error")
    {
        return Refine((val, _) => predicate(val), message, code);
    }

    public GuidContextSchema<TContext> RefineAsync(
        Func<Guid, TContext, CancellationToken, ValueTask<bool>> predicate,
        string message,
        string code = "custom_error")
    {
        Use(new RefinementRule<Guid, TContext>(async (val, ctx) =>
            await predicate(val, ctx.Data, ctx.CancellationToken)
                ? null
                : new ValidationError(ctx.Path, code, message)));
        return this;
    }

    public GuidContextSchema<TContext> RefineAsync(
        Func<Guid, CancellationToken, ValueTask<bool>> predicate,
        string message,
        string code = "custom_error")
    {
        return RefineAsync((val, _, ct) => predicate(val, ct), message, code);
    }
}
