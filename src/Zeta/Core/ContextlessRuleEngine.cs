using Zeta.Rules;

namespace Zeta.Core;

/// <summary>
/// Shared rule execution engine for contextless schemas.
/// </summary>
public sealed class ContextlessRuleEngine<T>
{
    private readonly List<IValidationRule<T>> _rules = [];

    public void Add(IValidationRule<T> rule) => _rules.Add(rule);
    
    public async ValueTask<List<ValidationError>?> ExecuteAsync(T value, ValidationContext context)
    {
        List<ValidationError>? errors = null;

        foreach (var rule in _rules)
        {
            var error = await rule.ValidateAsync(value, context);
            if (error == null) continue;
            errors ??= [];
            errors.Add(error);
        }

        return errors;
    }
    
    public ContextRuleEngine<T, TContext> ToContext<TContext>()
    {
        var newEngine = new ContextRuleEngine<T, TContext>();

        foreach (var rule in _rules)
        {
            newEngine.Add(new ContextlessRuleAdapter<T, TContext>(rule));
        }

        return newEngine;
    }
}

/// <summary>
/// Shared rule execution engine for context-aware schemas.
/// </summary>
public sealed class ContextRuleEngine<T, TContext>
{
    private readonly List<IValidationRule<T, TContext>> _rules = [];

    public void Add(IValidationRule<T, TContext> rule) => _rules.Add(rule);

    public async ValueTask<List<ValidationError>?> ExecuteAsync(T value, ValidationContext<TContext> context)
    {
        List<ValidationError>? errors = null;

        foreach (var rule in _rules)
        {
            var error = await rule.ValidateAsync(value, context);
            if (error == null) continue;
            errors ??= [];
            errors.Add(error);
        }

        return errors;
    }
}