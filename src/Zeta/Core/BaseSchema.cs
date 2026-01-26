using Zeta.Rules;

namespace Zeta.Core;

/// <summary>
/// Base class for context-aware schemas.
/// </summary>
public abstract class BaseSchema<T, TContext> : ISchema<T, TContext>
{
    private readonly List<IValidationRule<T, TContext>> _syncRules = [];
    private readonly List<IAsyncValidationRule<T, TContext>> _asyncRules = [];

    public virtual async ValueTask<Result> ValidateAsync(T value, ValidationContext<TContext> context)
    {
        List<ValidationError>? errors = null;

        foreach (var rule in _syncRules)
        {
            var error = rule.Validate(value, context);
            if (error == null) continue;

            errors ??= [];
            errors.Add(error);
        }

        foreach (var rule in _asyncRules)
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
        _syncRules.Add(rule);
    }

    protected void Use(IAsyncValidationRule<T, TContext> rule)
    {
        _asyncRules.Add(rule);
    }
}
