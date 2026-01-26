using Zeta.Rules;

namespace Zeta.Core;

/// <summary>
/// Base class for context-aware schemas.
/// </summary>
public abstract class BaseSchema<T, TContext> : ISchema<T, TContext>
{
    private readonly List<IValidationRule<T, TContext>> _rules = [];

    public virtual async ValueTask<Result> ValidateAsync(T value, ValidationContext<TContext> context)
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

    protected void Use(IValidationRule<T, TContext> rule)
    {
        _rules.Add(rule);
    }
}
