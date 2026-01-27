using Zeta.Core;
using Zeta.Rules;
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
        Use(new RefinementRule<int>((val, exec) =>
            NumericValidators.Min(val, min, exec.Path, message)));
        return this;
    }

    public IntContextlessSchema Max(int max, string? message = null)
    {
        Use(new RefinementRule<int>((val, exec) =>
            NumericValidators.Max(val, max, exec.Path, message)));
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

    /// <summary>
    /// Creates a context-aware int schema with all rules from this schema.
    /// </summary>
    public IntContextSchema<TContext> WithContext<TContext>()
        => new IntContextSchema<TContext>(Rules.ToContext<TContext>());
}
