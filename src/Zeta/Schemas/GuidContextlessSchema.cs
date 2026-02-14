using Zeta.Core;
using Zeta.Rules;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating Guid values.
/// </summary>
public sealed class GuidContextlessSchema : ContextlessSchema<Guid, GuidContextlessSchema>
{
    internal GuidContextlessSchema()
    {
    }

    protected override GuidContextlessSchema CreateInstance() => new();

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

    /// <summary>
    /// Creates a context-aware Guid schema with all rules from this schema.
    /// </summary>
    public GuidContextSchema<TContext> Using<TContext>()
    {
        var schema = new GuidContextSchema<TContext>(Rules.ToContext<TContext>());
        if (AllowNull) schema.Nullable();
        schema.TransferContextlessConditionals(GetConditionals());
        return schema;
    }

    /// <summary>
    /// Creates a context-aware Guid schema with a factory delegate for creating context data.
    /// </summary>
    public GuidContextSchema<TContext> Using<TContext>(
        Func<Guid, IServiceProvider, CancellationToken, ValueTask<TContext>> factory)
    {
        var schema = Using<TContext>();
        schema.SetContextFactory(factory);
        return schema;
    }
}