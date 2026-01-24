using Zeta.Rules;

namespace Zeta.Core;

public abstract class BaseSchema<T, TContext> : ISchema<T, TContext>
{
    private readonly ICollection<ISyncRule<T, TContext>> _syncRules = [];
    private readonly ICollection<IAsyncRule<T, TContext>> _asyncRules = [];

    public async ValueTask<Result> ValidateAsync(T value, ValidationContext<TContext> context)
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

    public BaseSchema<T, TContext> Use(ISyncRule<T, TContext> rule)
    {
        _syncRules.Add(rule);
        return this;
    }

    public BaseSchema<T, TContext> Use(IAsyncRule<T, TContext> rule)
    {
        _asyncRules.Add(rule);
        return this;
    }
}