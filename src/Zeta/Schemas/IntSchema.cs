using Zeta.Core;

namespace Zeta.Schemas;

/// <summary>
/// A schema for validating integer values with a specific context.
/// </summary>
public class IntSchema<TContext> : ISchema<int, TContext>
{
    private readonly List<IRule<int, TContext>> _rules = new();

    public async ValueTask<Result> ValidateAsync(int value, ValidationContext<TContext> context)
    {
        List<ValidationError>? errors = null;
        foreach (var rule in _rules)
        {
            var error = await rule.ValidateAsync(value, context);
            if (error == null) continue;
            errors ??= [];
            errors.Add(error);
        }

        return errors == null
            ? Result.Success()
            : Result.Failure(errors);
    }

    public IntSchema<TContext> Use(IRule<int, TContext> rule)
    {
        _rules.Add(rule);
        return this;
    }

    public IntSchema<TContext> Min(int min, string? message = null)
    {
        return Use(new DelegateRule<int, TContext>((val, ctx) =>
        {
            if (val >= min) return ValueTaskHelper.NullError();
            return ValueTaskHelper.Error(new ValidationError(
                ctx.Execution.Path, "min_value", message ?? $"Must be at least {min}"));
        }));
    }

    public IntSchema<TContext> Max(int max, string? message = null)
    {
        return Use(new DelegateRule<int, TContext>((val, ctx) =>
        {
            if (val <= max) return ValueTaskHelper.NullError();
            return ValueTaskHelper.Error(new ValidationError(
                ctx.Execution.Path, "max_value", message ?? $"Must be at most {max}"));
        }));
    }

    public IntSchema<TContext> Refine(Func<int, TContext, bool> predicate, string message, string code = "custom_error")
    {
        return Use(new DelegateRule<int, TContext>((val, ctx) =>
        {
            if (predicate(val, ctx.Data)) return ValueTaskHelper.NullError();
            return ValueTaskHelper.Error(new ValidationError(ctx.Execution.Path, code, message));
        }));
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