using Zeta.Core;
using Zeta.Rules;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating boolean values.
/// </summary>
public sealed class BoolSchema : ISchema<bool>
{
    private readonly RuleEngine<bool> _rules = new();

    public async ValueTask<Result<bool>> ValidateAsync(bool value, ValidationExecutionContext? execution = null)
    {
        execution ??= ValidationExecutionContext.Empty;
        var errors = await _rules.ExecuteAsync(value, execution);

        return errors == null
            ? Result<bool>.Success(value)
            : Result<bool>.Failure(errors);
    }

    public BoolSchema IsTrue(string? message = null)
    {
        _rules.Add(new RefinementRule<bool>((val, exec) =>
            val
                ? null
                : new ValidationError(exec.Path, "is_true", message ?? "Must be true")));
        return this;
    }

    public BoolSchema IsFalse(string? message = null)
    {
        _rules.Add(new RefinementRule<bool>((val, exec) =>
            !val
                ? null
                : new ValidationError(exec.Path, "is_false", message ?? "Must be false")));
        return this;
    }

    public BoolSchema Refine(Func<bool, bool> predicate, string message, string code = "custom_error")
    {
        _rules.Add(new RefinementRule<bool>((val, exec) =>
            predicate(val)
                ? null
                : new ValidationError(exec.Path, code, message)));
        return this;
    }
}

/// <summary>
/// A context-aware schema for validating boolean values.
/// </summary>
public class BoolSchema<TContext> : BaseSchema<bool, TContext>
{
    public BoolSchema<TContext> IsTrue(string? message = null)
    {
        Use(new RefinementRule<bool, TContext>((val, ctx) =>
            val
                ? null
                : new ValidationError(ctx.Execution.Path, "is_true", message ?? "Must be true")));
        return this;
    }

    public BoolSchema<TContext> IsFalse(string? message = null)
    {
        Use(new RefinementRule<bool, TContext>((val, ctx) =>
            !val
                ? null
                : new ValidationError(ctx.Execution.Path, "is_false", message ?? "Must be false")));
        return this;
    }

    public BoolSchema<TContext> Refine(Func<bool, TContext, bool> predicate, string message, string code = "custom_error")
    {
        Use(new RefinementRule<bool, TContext>((val, ctx) =>
            predicate(val, ctx.Data)
                ? null
                : new ValidationError(ctx.Execution.Path, code, message)));
        return this;
    }

    public BoolSchema<TContext> Refine(Func<bool, bool> predicate, string message, string code = "custom_error")
    {
        return Refine((val, _) => predicate(val), message, code);
    }
}
