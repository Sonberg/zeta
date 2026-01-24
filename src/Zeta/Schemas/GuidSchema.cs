using Zeta.Core;
using Zeta.Rules;

namespace Zeta.Schemas;

/// <summary>
/// A schema for validating Guid values with a specific context.
/// </summary>
public class GuidSchema<TContext> : BaseSchema<Guid, TContext>
{
    /// <summary>
    /// Validates that the Guid is not empty (not Guid.Empty).
    /// </summary>
    public GuidSchema<TContext> NotEmpty(string? message = null)
    {
        Use(new DelegateSyncRule<Guid, TContext>((val, ctx) =>
            val != Guid.Empty
                ? null
                : new ValidationError(ctx.Execution.Path, "not_empty", message ?? "GUID cannot be empty")));
        return this;
    }

    /// <summary>
    /// Validates that the Guid matches the expected version (1-5).
    /// </summary>
    public GuidSchema<TContext> Version(int version, string? message = null)
    {
        Use(new DelegateSyncRule<Guid, TContext>((val, ctx) =>
        {
            var bytes = val.ToByteArray();
            var guidVersion = (bytes[7] >> 4) & 0x0F;
            return guidVersion == version
                ? null
                : new ValidationError(ctx.Execution.Path, "version", message ?? $"GUID must be version {version}");
        }));
        return this;
    }

    public GuidSchema<TContext> Refine(Func<Guid, TContext, bool> predicate, string message, string code = "custom_error")
    {
        Use(new DelegateSyncRule<Guid, TContext>((val, ctx) =>
            predicate(val, ctx.Data)
                ? null
                : new ValidationError(ctx.Execution.Path, code, message)));
        return this;
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
