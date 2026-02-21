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

    private GuidContextlessSchema(
        ContextlessRuleEngine<Guid> rules,
        bool allowNull,
        IReadOnlyList<(Func<Guid, bool>, ISchema<Guid>)>? conditionals)
        : base(rules, allowNull, conditionals)
    {
    }

    protected override GuidContextlessSchema CreateInstance() => new();

    protected override GuidContextlessSchema CreateInstance(
        ContextlessRuleEngine<Guid> rules,
        bool allowNull,
        IReadOnlyList<(Func<Guid, bool>, ISchema<Guid>)>? conditionals)
        => new(rules, allowNull, conditionals);

    public GuidContextlessSchema NotEmpty(string? message = null)
        => Append(new RefinementRule<Guid>((val, exec) =>
            val != Guid.Empty
                ? null
                : new ValidationError(exec.PathSegments, "not_empty", message ?? "GUID cannot be empty")));

    public GuidContextlessSchema Version(int version, string? message = null)
        => Append(new RefinementRule<Guid>((val, exec) =>
        {
            var bytes = val.ToByteArray();
            var guidVersion = (bytes[7] >> 4) & 0x0F;
            return guidVersion == version
                ? null
                : new ValidationError(exec.PathSegments, "version", message ?? $"GUID must be version {version}");
        }));

    /// <summary>
    /// Creates a context-aware Guid schema with all rules from this schema.
    /// </summary>
    public GuidContextSchema<TContext> Using<TContext>()
    {
        var schema = new GuidContextSchema<TContext>(Rules.ToContext<TContext>());
        schema = AllowNull ? schema.Nullable() : schema;
        schema = schema.TransferContextlessConditionals(GetConditionals());
        return schema;
    }

    /// <summary>
    /// Creates a context-aware Guid schema with a factory delegate for creating context data.
    /// </summary>
    public GuidContextSchema<TContext> Using<TContext>(
        Func<Guid, IServiceProvider, CancellationToken, ValueTask<TContext>> factory)
    {
        return Using<TContext>().WithContextFactory(factory);
    }
}
