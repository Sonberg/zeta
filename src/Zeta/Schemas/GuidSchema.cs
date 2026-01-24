using Zeta.Core;

namespace Zeta.Schemas;

/// <summary>
/// A schema for validating Guid values with a specific context.
/// </summary>
public class GuidSchema<TContext> : ISchema<Guid, TContext>
{
    private readonly List<IRule<Guid, TContext>> _rules = [];

    public async ValueTask<Result> ValidateAsync(Guid value, ValidationContext<TContext> context)
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

    public GuidSchema<TContext> Use(IRule<Guid, TContext> rule)
    {
        _rules.Add(rule);
        return this;
    }

    /// <summary>
    /// Validates that the Guid is not empty (not Guid.Empty).
    /// </summary>
    public GuidSchema<TContext> NotEmpty(string? message = null)
    {
        return Use(new DelegateRule<Guid, TContext>((val, ctx) =>
        {
            if (val != Guid.Empty) return ValueTaskHelper.NullError();
            return ValueTaskHelper.Error(new ValidationError(
                ctx.Execution.Path, "not_empty", message ?? "GUID cannot be empty"));
        }));
    }

    /// <summary>
    /// Validates that the Guid matches the expected version (1-5).
    /// </summary>
    public GuidSchema<TContext> Version(int version, string? message = null)
    {
        return Use(new DelegateRule<Guid, TContext>((val, ctx) =>
        {
            var bytes = val.ToByteArray();
            var guidVersion = (bytes[7] >> 4) & 0x0F;
            if (guidVersion == version) return ValueTaskHelper.NullError();
            return ValueTaskHelper.Error(new ValidationError(
                ctx.Execution.Path, "version", message ?? $"GUID must be version {version}"));
        }));
    }

    public GuidSchema<TContext> Refine(Func<Guid, TContext, bool> predicate, string message, string code = "custom_error")
    {
        return Use(new DelegateRule<Guid, TContext>((val, ctx) =>
        {
            if (predicate(val, ctx.Data)) return ValueTaskHelper.NullError();
            return ValueTaskHelper.Error(new ValidationError(ctx.Execution.Path, code, message));
        }));
    }

    public GuidSchema<TContext> Refine(Func<Guid, bool> predicate, string message, string code = "custom_error")
    {
        return Refine((val, _) => predicate(val), message, code);
    }
}

/// <summary>
/// A schema for validating Guid values with default context.
/// </summary>
public sealed class GuidSchema : GuidSchema<object?>, ISchema<Guid>
{
    public async ValueTask<Result<Guid>> ValidateAsync(Guid value, ValidationExecutionContext? execution = null)
    {
        execution ??= ValidationExecutionContext.Empty;
        var context = new ValidationContext<object?>(null, execution);
        var result = await ValidateAsync(value, context);

        return result.IsSuccess
            ? Result<Guid>.Success(value)
            : Result<Guid>.Failure(result.Errors);
    }
}