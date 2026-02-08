using Zeta.Rules;

namespace Zeta.Core;

/// <summary>
/// Base class for context-aware schemas.
/// </summary>
public abstract class ContextSchema<T, TContext, TSchema> : ISchema<T, TContext> where TSchema : ContextSchema<T, TContext, TSchema>
{
    protected ContextRuleEngine<T, TContext> Rules { get; }

    private bool AllowNull { get; set; }

    protected ContextSchema() : this(new ContextRuleEngine<T, TContext>())
    {
    }

    protected ContextSchema(ContextRuleEngine<T, TContext> rules)
    {
        Rules = rules;
    }

    public virtual async ValueTask<Result> ValidateAsync(T? value, ValidationContext<TContext> context)
    {
        if (value is null)
        {
            return AllowNull
                ? Result.Success()
                : Result<T>.Failure(new ValidationError(context.Path, "null_value", "Value cannot be null"));
        }

        var errors = await Rules.ExecuteAsync(value, context);

        return errors == null
            ? Result.Success()
            : Result.Failure(errors);
    }

    protected void Use(IValidationRule<T, TContext> rule)
    {
        Rules.Add(rule);
    }
    
    internal ContextRuleEngine<T, TContext> GetRules()
    {
        return Rules;
    }

    public TSchema Nullable()
    {
        AllowNull = true;
        return this as TSchema ?? throw new InvalidOperationException();
    }

    public TSchema Refine(Func<T, TContext, bool> predicate, string message, string code = "custom_error")
    {
        Use(new RefinementRule<T, TContext>((val, ctx) =>
            predicate(val, ctx.Data)
                ? null
                : new ValidationError(ctx.Path, code, message)));
        return this as TSchema ?? throw new InvalidOperationException();
    }

    public TSchema RefineAsync(Func<T, TContext, ValueTask<bool>> predicate, string message, string code = "custom_error")
    {
        Use(new RefinementRule<T, TContext>(async (val, ctx) =>
            await predicate(val, ctx.Data)
                ? null
                : new ValidationError(ctx.Path, code, message)));
        return this as TSchema ?? throw new InvalidOperationException();
    }

    public TSchema Refine(Func<T, bool> predicate, string message, string code = "custom_error")
    {
        return Refine((val, _) => predicate(val), message, code);
    }

    public TSchema RefineAsync(Func<T, ValueTask<bool>> predicate, string message, string code = "custom_error")
    {
        return RefineAsync(async (val, _) => await predicate(val), message, code);
    }
}