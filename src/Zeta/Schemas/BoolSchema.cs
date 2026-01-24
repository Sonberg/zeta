using Zeta.Core;

namespace Zeta.Schemas;

/// <summary>
/// A schema for validating boolean values with a specific context.
/// </summary>
public class BoolSchema<TContext> : ISchema<bool, TContext>
{
    private readonly List<IRule<bool, TContext>> _rules = [];

    public async ValueTask<Result> ValidateAsync(bool value, ValidationContext<TContext> context)
    {
        List<ValidationError>? errors = null;
        foreach (var rule in _rules)
        {
            var error = await rule.ValidateAsync(value, context);
            if (error != null)
            {
                errors ??= new List<ValidationError>();
                errors.Add(error);
            }
        }

        return errors == null
            ? Result.Success()
            : Result.Failure(errors);
    }

    public BoolSchema<TContext> Use(IRule<bool, TContext> rule)
    {
        _rules.Add(rule);
        return this;
    }

    /// <summary>
    /// Validates that the value is true.
    /// </summary>
    public BoolSchema<TContext> IsTrue(string? message = null)
    {
        return Use(new DelegateRule<bool, TContext>((val, ctx) =>
        {
            if (val) return ValueTaskHelper.NullError();
            return ValueTaskHelper.Error(new ValidationError(
                ctx.Execution.Path, "is_true", message ?? "Must be true"));
        }));
    }

    /// <summary>
    /// Validates that the value is false.
    /// </summary>
    public BoolSchema<TContext> IsFalse(string? message = null)
    {
        return Use(new DelegateRule<bool, TContext>((val, ctx) =>
        {
            if (!val) return ValueTaskHelper.NullError();
            return ValueTaskHelper.Error(new ValidationError(
                ctx.Execution.Path, "is_false", message ?? "Must be false"));
        }));
    }

    public BoolSchema<TContext> Refine(Func<bool, TContext, bool> predicate, string message, string code = "custom_error")
    {
        return Use(new DelegateRule<bool, TContext>((val, ctx) =>
        {
            if (predicate(val, ctx.Data)) return ValueTaskHelper.NullError();
            return ValueTaskHelper.Error(new ValidationError(ctx.Execution.Path, code, message));
        }));
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

        return result.IsSuccess ? Result<bool>.Success(value) : Result<bool>.Failure(result.Errors);
    }
}