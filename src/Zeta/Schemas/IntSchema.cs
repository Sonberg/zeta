using Zeta.Core;
using Zeta.Rules;
using Zeta.Validation;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating integer values.
/// </summary>
public sealed class IntSchema : ContextlessSchema<int>
{
    public IntSchema Min(int min, string? message = null)
    {
        Use(new RefinementRule<int>((val, exec) =>
            NumericValidators.Min(val, min, exec.Path, message)));
        return this;
    }

    public IntSchema Max(int max, string? message = null)
    {
        Use(new RefinementRule<int>((val, exec) =>
            NumericValidators.Max(val, max, exec.Path, message)));
        return this;
    }

    public IntSchema Refine(Func<int, bool> predicate, string message, string code = "custom_error")
    {
        Use(new RefinementRule<int>((val, exec) =>
            predicate(val)
                ? null
                : new ValidationError(exec.Path, code, message)));
        return this;
    }
}

/// <summary>
/// A context-aware schema for validating integer values.
/// </summary>
public class IntSchema<TContext> : ContextSchema<int, TContext>
{
    public IntSchema<TContext> Min(int min, string? message = null)
    {
        Use(new RefinementRule<int, TContext>((val, ctx) =>
            NumericValidators.Min(val, min, ctx.Execution.Path, message)));
        return this;
    }

    public IntSchema<TContext> Max(int max, string? message = null)
    {
        Use(new RefinementRule<int, TContext>((val, ctx) =>
            NumericValidators.Max(val, max, ctx.Execution.Path, message)));
        return this;
    }

    public IntSchema<TContext> Refine(Func<int, TContext, bool> predicate, string message, string code = "custom_error")
    {
        Use(new RefinementRule<int, TContext>((val, ctx) =>
            predicate(val, ctx.Data)
                ? null
                : new ValidationError(ctx.Execution.Path, code, message)));
        return this;
    }

    public IntSchema<TContext> Refine(Func<int, bool> predicate, string message, string code = "custom_error")
    {
        return Refine((val, _) => predicate(val), message, code);
    }
}
