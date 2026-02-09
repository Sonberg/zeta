using Zeta.Rules;

namespace Zeta.Core;

public abstract class ContextlessSchema<T, TSchema> : ISchema<T> where TSchema : ContextlessSchema<T, TSchema>
{
    protected ContextlessRuleEngine<T> Rules { get; }
    
    public bool AllowNull { get; private set; }
    
    protected ContextlessSchema() : this(new ContextlessRuleEngine<T>())
    {
    }

    protected ContextlessSchema(ContextlessRuleEngine<T> rules)
    {
        Rules = rules;
    }

    public ValueTask<Result<T>> ValidateAsync(T? value)
    {
        return ValidateAsync(value, ValidationContext.Empty);
    }
    
    public virtual async ValueTask<Result<T>> ValidateAsync(T? value, ValidationContext context)
    {
        if (value is null)
        {
            return AllowNull
                ? Result<T>.Success(value!)
                : Result<T>.Failure(new ValidationError(context.Path, "null_value", "Value cannot be null"));
        }

        var errors = await Rules.ExecuteAsync(value!, context);

        return errors == null
            ? Result<T>.Success(value)
            : Result<T>.Failure(errors);
    }

    protected void Use(IValidationRule<T> rule)
    {
        Rules.Add(rule);
    }

    internal ContextlessRuleEngine<T> GetRules()
    {
        return Rules;
    }

    public TSchema Nullable()
    {
        AllowNull = true;
        return this as TSchema ?? throw new InvalidOperationException();
    }

    public TSchema Refine(Func<T, bool> predicate, string message, string code = "custom_error")
    {
        Use(new RefinementRule<T>((val, ctx) =>
            predicate(val)
                ? null
                : new ValidationError(ctx.Path, code, message)));
        return this as TSchema ?? throw new InvalidOperationException();
    }

    public TSchema RefineAsync(Func<T, ValueTask<bool>> predicate, string message, string code = "custom_error")
    {
        Use(new RefinementRule<T>(async (val, ctx) =>
            await predicate(val)
                ? null
                : new ValidationError(ctx.Path, code, message)));
        return this as TSchema ?? throw new InvalidOperationException();
    }

    public TSchema RefineAsync(Func<T, CancellationToken, ValueTask<bool>> predicate, string message, string code = "custom_error")
    {
        Use(new RefinementRule<T>(async (val, ctx) =>
            await predicate(val, ctx.CancellationToken)
                ? null
                : new ValidationError(ctx.Path, code, message)));
        return this as TSchema ?? throw new InvalidOperationException();
    }
}