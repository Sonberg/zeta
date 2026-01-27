using Zeta.Rules;

namespace Zeta.Core;

/// <summary>
/// Shared rule execution engine for contextless schemas.
/// </summary>
public sealed class ContextlessRuleEngine<T>
{
    private readonly List<IValidationRule<T>> _rules = [];

    public void Add(IValidationRule<T> rule) => _rules.Add(rule);

    public IReadOnlyList<IValidationRule<T>> GetRules() => _rules;

    public async ValueTask<List<ValidationError>?> ExecuteAsync(T value, ValidationExecutionContext execution)
    {
        List<ValidationError>? errors = null;

        foreach (var rule in _rules)
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
    private readonly List<IValidationRule<T, TContext>> _rules = [];

    public void Add(IValidationRule<T, TContext> rule) => _rules.Add(rule);

    /// <summary>
    /// Copies rules from a contextless rule engine, adapting them for context-aware execution.
    /// </summary>
    public void CopyFrom(ContextlessRuleEngine<T> source)
    {
        foreach (var rule in source.GetRules())
        {
            _rules.Add(new ContextlessRuleAdapter<T, TContext>(rule));
        }
    }

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
