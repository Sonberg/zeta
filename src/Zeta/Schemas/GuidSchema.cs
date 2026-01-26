using Zeta.Core;
using Zeta.Rules;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating Guid values.
/// </summary>
public sealed class GuidSchema : ISchema<Guid>
{
    private readonly RuleEngine<Guid> _rules = new();

    public async ValueTask<Result<Guid>> ValidateAsync(Guid value, ValidationExecutionContext? execution = null)
    {
        execution ??= ValidationExecutionContext.Empty;
        var errors = await _rules.ExecuteAsync(value, execution);

        return errors == null
            ? Result<Guid>.Success(value)
            : Result<Guid>.Failure(errors);
    }

    public GuidSchema NotEmpty(string? message = null)
    {
        _rules.Add(new DelegateValidationRule<Guid>((val, exec) =>
            val != Guid.Empty
                ? null
                : new ValidationError(exec.Path, "not_empty", message ?? "GUID cannot be empty")));
        return this;
    }

    public GuidSchema Version(int version, string? message = null)
    {
        _rules.Add(new DelegateValidationRule<Guid>((val, exec) =>
        {
            var bytes = val.ToByteArray();
            var guidVersion = (bytes[7] >> 4) & 0x0F;
            return guidVersion == version
                ? null
                : new ValidationError(exec.Path, "version", message ?? $"GUID must be version {version}");
        }));
        return this;
    }

    public GuidSchema Refine(Func<Guid, bool> predicate, string message, string code = "custom_error")
    {
        _rules.Add(new DelegateValidationRule<Guid>((val, exec) =>
            predicate(val)
                ? null
                : new ValidationError(exec.Path, code, message)));
        return this;
    }
}

/// <summary>
/// A context-aware schema for validating Guid values.
/// </summary>
public class GuidSchema<TContext> : BaseSchema<Guid, TContext>
{
    public GuidSchema<TContext> NotEmpty(string? message = null)
    {
        Use(new DelegateSyncRule<Guid, TContext>((val, ctx) =>
            val != Guid.Empty
                ? null
                : new ValidationError(ctx.Execution.Path, "not_empty", message ?? "GUID cannot be empty")));
        return this;
    }

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
