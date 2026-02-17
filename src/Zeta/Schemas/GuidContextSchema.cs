using Zeta.Core;
using Zeta.Rules;

namespace Zeta.Schemas;

/// <summary>
/// A context-aware schema for validating Guid values.
/// </summary>
public class GuidContextSchema<TContext> : ContextSchema<Guid, TContext, GuidContextSchema<TContext>>
{
    internal GuidContextSchema() { }

    internal GuidContextSchema(ContextRuleEngine<Guid, TContext> rules) : base(rules)
    {
    }

    private GuidContextSchema(
        ContextRuleEngine<Guid, TContext> rules,
        bool allowNull,
        IReadOnlyList<ISchemaConditional<Guid, TContext>>? conditionals,
        Func<Guid, IServiceProvider, CancellationToken, ValueTask<TContext>>? contextFactory)
        : base(rules, allowNull, conditionals, contextFactory)
    {
    }

    protected override GuidContextSchema<TContext> CreateInstance() => new();

    protected override GuidContextSchema<TContext> CreateInstance(
        ContextRuleEngine<Guid, TContext> rules,
        bool allowNull,
        IReadOnlyList<ISchemaConditional<Guid, TContext>>? conditionals,
        Func<Guid, IServiceProvider, CancellationToken, ValueTask<TContext>>? contextFactory)
        => new(rules, allowNull, conditionals, contextFactory);

    public GuidContextSchema<TContext> NotEmpty(string? message = null)
        => Append(new RefinementRule<Guid, TContext>((val, ctx) =>
            val != Guid.Empty
                ? null
                : new ValidationError(ctx.Path, "not_empty", message ?? "GUID cannot be empty")));

    public GuidContextSchema<TContext> Version(int version, string? message = null)
        => Append(new RefinementRule<Guid, TContext>((val, ctx) =>
        {
            var bytes = val.ToByteArray();
            var guidVersion = (bytes[7] >> 4) & 0x0F;
            return guidVersion == version
                ? null
                : new ValidationError(ctx.Path, "version", message ?? $"GUID must be version {version}");
        }));
}
