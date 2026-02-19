using Zeta.Rules;

namespace Zeta.Core;

internal sealed class RuleNode<T>
{
    public IValidationRule<T> Rule { get; }
    public RuleNode<T>? Previous { get; }

    public RuleNode(IValidationRule<T> rule, RuleNode<T>? previous)
    {
        Rule = rule;
        Previous = previous;
    }
}

internal sealed class RuleNode<T, TContext>
{
    public IValidationRule<T, TContext> Rule { get; }
    public RuleNode<T, TContext>? Previous { get; }

    public RuleNode(IValidationRule<T, TContext> rule, RuleNode<T, TContext>? previous)
    {
        Rule = rule;
        Previous = previous;
    }
}

/// <summary>
/// Immutable rule execution engine for contextless schemas.
/// Uses a persistent linked list for O(1) append with structural sharing.
/// </summary>
public sealed class ContextlessRuleEngine<T>
{
    private readonly RuleNode<T>? _head;
    private IValidationRule<T>[]? _materialized;

    public ContextlessRuleEngine()
    {
        _head = null;
    }

    private ContextlessRuleEngine(RuleNode<T>? head)
    {
        _head = head;
    }

    public ContextlessRuleEngine<T> Add(IValidationRule<T> rule)
        => new(new RuleNode<T>(rule, _head));

    private IValidationRule<T>[] Materialize()
    {
        if (_materialized != null) return _materialized;

        // Count nodes
        var count = 0;
        var node = _head;
        while (node != null)
        {
            count++;
            node = node.Previous;
        }

        if (count == 0)
        {
            _materialized = [];
            return _materialized;
        }

        // Fill array in reverse (linked list is LIFO, rules should execute in insertion order)
        var array = new IValidationRule<T>[count];
        node = _head;
        for (var i = count - 1; i >= 0; i--)
        {
            array[i] = node!.Rule;
            node = node.Previous;
        }

        _materialized = array;
        return _materialized;
    }

    public async ValueTask<List<ValidationError>?> ExecuteAsync(T value, ValidationContext context)
    {
        var rules = Materialize();
        List<ValidationError>? errors = null;

        foreach (var rule in rules)
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
        var rules = Materialize();
        var engine = new ContextRuleEngine<T, TContext>();

        foreach (var rule in rules)
        {
            engine = engine.Add(new ContextlessRuleAdapter<T, TContext>(rule));
        }

        return engine;
    }
}

/// <summary>
/// Immutable rule execution engine for context-aware schemas.
/// Uses a persistent linked list for O(1) append with structural sharing.
/// </summary>
public sealed class ContextRuleEngine<T, TContext>
{
    private readonly RuleNode<T, TContext>? _head;
    private IValidationRule<T, TContext>[]? _materialized;

    public ContextRuleEngine()
    {
        _head = null;
    }

    private ContextRuleEngine(RuleNode<T, TContext>? head)
    {
        _head = head;
    }

    public ContextRuleEngine<T, TContext> Add(IValidationRule<T, TContext> rule)
        => new(new RuleNode<T, TContext>(rule, _head));

    private IValidationRule<T, TContext>[] Materialize()
    {
        if (_materialized != null) return _materialized;

        var count = 0;
        var node = _head;
        while (node != null)
        {
            count++;
            node = node.Previous;
        }

        if (count == 0)
        {
            _materialized = [];
            return _materialized;
        }

        var array = new IValidationRule<T, TContext>[count];
        node = _head;
        for (var i = count - 1; i >= 0; i--)
        {
            array[i] = node!.Rule;
            node = node.Previous;
        }

        _materialized = array;
        return _materialized;
    }

    public async ValueTask<List<ValidationError>?> ExecuteAsync(T value, ValidationContext<TContext> context)
    {
        var rules = Materialize();
        List<ValidationError>? errors = null;

        foreach (var rule in rules)
        {
            var error = await rule.ValidateAsync(value, context);
            if (error == null) continue;
            errors ??= [];
            errors.Add(error);
        }

        return errors;
    }

    public ContextRuleEngine<T, TNewContext> Adapt<TNewContext>(
        Func<T, IServiceProvider, CancellationToken, ValueTask<TContext>> contextResolver)
    {
        var rules = Materialize();
        var engine = new ContextRuleEngine<T, TNewContext>();

        foreach (var rule in rules)
        {
            engine = engine.Add(new ContextRuleAdapter<T, TContext, TNewContext>(rule, contextResolver));
        }

        return engine;
    }
}
