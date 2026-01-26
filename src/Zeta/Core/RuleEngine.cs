using Zeta.Rules;

namespace Zeta.Core;

/// <summary>
/// Shared rule execution engine for contextless schemas.
/// </summary>
public sealed class RuleEngine<T>
{
    private readonly List<IValidationRule<T>> _syncRules = [];
    private readonly List<IAsyncValidationRule<T>> _asyncRules = [];

    public void Add(IValidationRule<T> rule) => _syncRules.Add(rule);
    public void Add(IAsyncValidationRule<T> rule) => _asyncRules.Add(rule);

    public async ValueTask<List<ValidationError>?> ExecuteAsync(T value, ValidationExecutionContext execution)
    {
        List<ValidationError>? errors = null;

        foreach (var rule in _syncRules)
        {
            var error = rule.Validate(value, execution);
            if (error == null) continue;
            errors ??= [];
            errors.Add(error);
        }

        foreach (var rule in _asyncRules)
        {
            var error = await rule.ValidateAsync(value, execution);
            if (error == null) continue;
            errors ??= [];
            errors.Add(error);
        }

        return errors;
    }
}

/// <summary>
/// Shared rule execution engine for context-aware schemas.
/// </summary>
public sealed class ContextRuleEngine<T, TContext>
{
    private readonly List<IValidationRule<T, TContext>> _syncRules = [];
    private readonly List<IAsyncValidationRule<T, TContext>> _asyncRules = [];

    public void Add(IValidationRule<T, TContext> rule) => _syncRules.Add(rule);
    public void Add(IAsyncValidationRule<T, TContext> rule) => _asyncRules.Add(rule);

    public async ValueTask<List<ValidationError>?> ExecuteAsync(T value, ValidationContext<TContext> context)
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

        return errors;
    }
}
