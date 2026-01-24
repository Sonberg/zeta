using Zeta.Rules;

namespace Zeta.Core;

public abstract class BaseSchema<T, TContext> : ISchema<T, TContext>
{
    private readonly ICollection<ISyncRule<T, TContext>> _syncRules = [];
    private readonly ICollection<IAsyncRule<T, TContext>> _asyncRules = [];

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

    protected void Use(ISyncRule<T, TContext> rule)
    {
        _syncRules.Add(rule);
    }

    protected void Use(IAsyncRule<T, TContext> rule)
    {
        _asyncRules.Add(rule);
    }
}