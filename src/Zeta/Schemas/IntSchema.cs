using Zeta.Core;
using Zeta.Rules;

namespace Zeta.Schemas;

/// <summary>
/// A schema for validating integer values with a specific context.
/// </summary>
public class IntSchema<TContext> : BaseSchema<int, TContext>
{
    public IntSchema<TContext> Min(int min, string? message = null)
    {
        Use(new DelegateSyncRule<int, TContext>((val, ctx) =>
            val >= min
                ? null
                : new ValidationError(ctx.Execution.Path, "min_value", message ?? $"Must be at least {min}")));
        return this;
    }

    public IntSchema<TContext> Max(int max, string? message = null)
    {
        Use(new DelegateSyncRule<int, TContext>((val, ctx) =>
            val <= max
                ? null
                : new ValidationError(ctx.Execution.Path, "max_value", message ?? $"Must be at most {max}")));
        return this;
    }

    public IntSchema<TContext> Refine(Func<int, TContext, bool> predicate, string message, string code = "custom_error")
    {
        Use(new DelegateSyncRule<int, TContext>((val, ctx) =>
            predicate(val, ctx.Data)
                ? null
                : new ValidationError(ctx.Execution.Path, code, message)));
        return this;
    }
}

/// <summary>
/// A schema for validating integer values with default context.
/// </summary>
public sealed class IntSchema : IntSchema<object?>, ISchema<int>
{
    public async ValueTask<Result<int>> ValidateAsync(int value, ValidationExecutionContext? execution = null)
    {
        execution ??= ValidationExecutionContext.Empty;
        var context = new ValidationContext<object?>(null, execution);
        var result = await ValidateAsync(value, context);

        return result.IsSuccess
            ? Result<int>.Success(value)
            : Result<int>.Failure(result.Errors);
    }
}
