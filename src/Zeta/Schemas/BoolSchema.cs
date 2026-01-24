using Zeta.Core;
using Zeta.Rules;

namespace Zeta.Schemas;

/// <summary>
/// A schema for validating boolean values with a specific context.
/// </summary>
public class BoolSchema<TContext> : BaseSchema<bool, TContext>
{
    /// <summary>
    /// Validates that the value is true.
    /// </summary>
    public BoolSchema<TContext> IsTrue(string? message = null)
    {
        Use(new DelegateSyncRule<bool, TContext>((val, ctx) =>
            val
                ? null
                : new ValidationError(ctx.Execution.Path, "is_true", message ?? "Must be true")));
        return this;
    }

    /// <summary>
    /// Validates that the value is false.
    /// </summary>
    public BoolSchema<TContext> IsFalse(string? message = null)
    {
        Use(new DelegateSyncRule<bool, TContext>((val, ctx) =>
            !val
                ? null
                : new ValidationError(ctx.Execution.Path, "is_false", message ?? "Must be false")));
        return this;
    }

    public BoolSchema<TContext> Refine(Func<bool, TContext, bool> predicate, string message, string code = "custom_error")
    {
        Use(new DelegateSyncRule<bool, TContext>((val, ctx) =>
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

/// <summary>
/// A schema for validating boolean values with default context.
/// </summary>
public sealed class BoolSchema : BoolSchema<object?>, ISchema<bool>
{
    public async ValueTask<Result<bool>> ValidateAsync(bool value, ValidationExecutionContext? execution = null)
    {
        execution ??= ValidationExecutionContext.Empty;
        var context = new ValidationContext<object?>(null, execution);
        var result = await ValidateAsync(value, context);

        return result.IsSuccess
            ? Result<bool>.Success(value)
            : Result<bool>.Failure(result.Errors);
    }
}
