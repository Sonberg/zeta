using Zeta.Rules;

namespace Zeta.Core;

/// <summary>
/// Base class for context-aware schemas.
/// </summary>
public abstract class ContextSchema<T, TContext> : ISchema<T, TContext>
{
    protected ContextRuleEngine<T, TContext> Rules = new();

    public virtual async ValueTask<Result> ValidateAsync(T value, ValidationContext<TContext> context)
    {
        var errors = await Rules.ExecuteAsync(value, context);

        return errors == null
            ? Result.Success()
            : Result.Failure(errors);
    }

    protected void Use(IValidationRule<T, TContext> rule)
    {
        Rules.Add(rule);
    }

    /// <summary>
    /// Copies rules from a contextless schema's rule engine.
    /// </summary>
    internal void CopyRulesFrom(ContextlessRuleEngine<T> source)
    {
        Rules.CopyFrom(source);
    }
}