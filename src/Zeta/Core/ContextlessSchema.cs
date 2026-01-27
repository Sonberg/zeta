using Zeta.Rules;

namespace Zeta.Core;

public abstract class ContextlessSchema<T> : ISchema<T>
{
    protected ContextlessRuleEngine<T> Rules { get; }

    protected ContextlessSchema() : this(new ContextlessRuleEngine<T>())
    {
    }

    protected ContextlessSchema(ContextlessRuleEngine<T> rules)
    {
        Rules = rules;
    }

    public ValueTask<Result<T>> ValidateAsync(T value)
    {
        return ValidateAsync(value, ValidationContext.Empty);
    }

    public virtual async ValueTask<Result<T>> ValidateAsync(T value, ValidationContext context)
    {
        var errors = await Rules.ExecuteAsync(value, context);

        return errors == null
            ? Result<T>.Success(value)
            : Result<T>.Failure(errors);
    }

    protected void Use(IValidationRule<T> rule)
    {
        Rules.Add(rule);
    }
}