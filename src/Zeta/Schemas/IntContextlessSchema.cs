using Zeta.Core;
using Zeta.Rules;
using Zeta.Rules.Numeric;
using Zeta.Validation;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating integer values.
/// </summary>
public sealed class IntContextlessSchema : ContextlessSchema<int>
{
    public IntContextlessSchema() { }

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

    public IntContextlessSchema Refine(Func<int, bool> predicate, string message, string code = "custom_error")
    {
        Use(new RefinementRule<int>((val, exec) =>
            predicate(val)
                ? null
                : new ValidationError(exec.Path, code, message)));
        return this;
    }

    public IntContextlessSchema RefineAsync(
        Func<int, CancellationToken, ValueTask<bool>> predicate,
        string message,
        string code = "custom_error")
    {
        Use(new RefinementRule<int>(async (val, exec) =>
            await predicate(val, exec.CancellationToken)
                ? null
                : new ValidationError(exec.Path, code, message)));
        return this;
    }

    /// <summary>
    /// Creates a context-aware int schema with all rules from this schema.
    /// </summary>
    public IntContextSchema<TContext> WithContext<TContext>()
        => new IntContextSchema<TContext>(Rules.ToContext<TContext>());
}
