using Zeta.Core;
using Zeta.Rules;
using Zeta.Rules.Numeric;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating integer values.
/// </summary>
public sealed class IntContextlessSchema : ContextlessSchema<int, IntContextlessSchema>
{
    internal IntContextlessSchema()
    {
    }

    protected override IntContextlessSchema CreateInstance() => new();

    public IntContextlessSchema Min(int min, string? message = null)
    {
        Use(new MinIntRule(min, message));
        return this;
    }

    public IntContextlessSchema Max(int max, string? message = null)
    {
        Use(new MaxIntRule(max, message));
        return this;
    }

    /// <summary>
    /// Creates a context-aware int schema with all rules from this schema.
    /// </summary>
    public IntContextSchema<TContext> Using<TContext>()
    {
        var schema = new IntContextSchema<TContext>(Rules.ToContext<TContext>());
        if (AllowNull) schema.Nullable();
        schema.TransferContextlessConditionals(GetConditionals());
        return schema;
    }

    /// <summary>
    /// Creates a context-aware int schema with a factory delegate for creating context data.
    /// </summary>
    public IntContextSchema<TContext> Using<TContext>(
        Func<int, IServiceProvider, CancellationToken, ValueTask<TContext>> factory)
    {
        var schema = Using<TContext>();
        schema.SetContextFactory(factory);
        return schema;
    }
}