using Zeta.Core;
using Zeta.Rules;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating Guid values.
/// </summary>
public sealed class GuidContextlessSchema : ContextlessSchema<Guid>
{
    public GuidContextlessSchema() { }

    public GuidContextlessSchema NotEmpty(string? message = null)
    {
        Use(new RefinementRule<Guid>((val, exec) =>
            val != Guid.Empty
                ? null
                : new ValidationError(exec.Path, "not_empty", message ?? "GUID cannot be empty")));
        return this;
    }

    public GuidContextlessSchema Version(int version, string? message = null)
    {
        Use(new RefinementRule<Guid>((val, exec) =>
        {
            var bytes = val.ToByteArray();
            var guidVersion = (bytes[7] >> 4) & 0x0F;
            return guidVersion == version
                ? null
                : new ValidationError(exec.Path, "version", message ?? $"GUID must be version {version}");
        }));
        return this;
    }

    public GuidContextlessSchema Refine(Func<Guid, bool> predicate, string message, string code = "custom_error")
    {
        Use(new RefinementRule<Guid>((val, exec) =>
            predicate(val)
                ? null
                : new ValidationError(exec.Path, code, message)));
        return this;
    }

    public GuidContextlessSchema RefineAsync(
        Func<Guid, CancellationToken, ValueTask<bool>> predicate,
        string message,
        string code = "custom_error")
    {
        Use(new RefinementRule<Guid>(async (val, exec) =>
            await predicate(val, exec.CancellationToken)
                ? null
                : new ValidationError(exec.Path, code, message)));
        return this;
    }

    public GuidContextlessSchema If(
        Func<Guid, bool> condition,
        Func<GuidContextlessSchema, GuidContextlessSchema> configure)
    {
        var inner = configure(new GuidContextlessSchema());
        foreach (var rule in inner.Rules.GetRules())
            Use(new ConditionalRule<Guid>(condition, rule));
        return this;
    }

    /// <summary>
    /// Creates a context-aware Guid schema with all rules from this schema.
    /// </summary>
    public GuidContextSchema<TContext> WithContext<TContext>()
        => new GuidContextSchema<TContext>(Rules.ToContext<TContext>());
}
