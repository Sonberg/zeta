using Zeta.Rules;

namespace Zeta.Core;

/// <summary>
/// An immutable, append-only chain of <typeparamref name="TRule"/> with O(1) append via structural
/// sharing and lazy conversion to insertion order. Backs both rule engines — the only piece they
/// share, and the only piece with non-trivial (LIFO-reversal) logic.
/// </summary>
internal sealed class RuleChain<TRule>
{
    private sealed class Node
    {
        public readonly TRule Rule;
        public readonly Node? Previous;

        public Node(TRule rule, Node? previous)
        {
            Rule = rule;
            Previous = previous;
        }
    }

    private readonly Node? _head;
    private TRule[]? _array;

    public RuleChain()
    {
    }

    private RuleChain(Node? head)
    {
        _head = head;
    }

    public RuleChain<TRule> Add(TRule rule) => new(new Node(rule, _head));

    /// <summary>Rules in insertion order. The chain is LIFO, so fill the array back-to-front. Cached per instance.</summary>
    public TRule[] ToArray()
    {
        if (_array != null) return _array;

        var count = 0;
        var node = _head;
        while (node != null)
        {
            count++;
            node = node.Previous;
        }

        if (count == 0) return _array = [];

        var array = new TRule[count];
        node = _head;
        for (var i = count - 1; i >= 0; i--)
        {
            array[i] = node!.Rule;
            node = node.Previous;
        }

        return _array = array;
    }
}

/// <summary>
/// Immutable rule execution engine for contextless schemas.
/// Uses a persistent linked list for O(1) append with structural sharing.
/// </summary>
public sealed class ContextlessRuleEngine<T>
{
    private readonly RuleChain<IValidationRule<T>> _chain;

    public ContextlessRuleEngine()
    {
        _chain = new RuleChain<IValidationRule<T>>();
    }

    private ContextlessRuleEngine(RuleChain<IValidationRule<T>> chain)
    {
        _chain = chain;
    }

    public ContextlessRuleEngine<T> Add(IValidationRule<T> rule)
        => new(_chain.Add(rule));

    public async ValueTask<List<ValidationError>?> ExecuteAsync(T value, ValidationRun context)
    {
        var rules = _chain.ToArray();
        List<ValidationError>? errors = null;

        foreach (var rule in rules)
        {
            var error = await rule.ValidateAsync(value, context);
            if (error == null) continue;
            errors ??= [];
            errors.Add(error.HasAttemptedValue ? error : error with { AttemptedValue = value, HasAttemptedValue = true });
        }

        return errors;
    }

    public ContextRuleEngine<T, TContext> ToContext<TContext>()
    {
        var rules = _chain.ToArray();
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
    private readonly RuleChain<IValidationRule<T, TContext>> _chain;

    public ContextRuleEngine()
    {
        _chain = new RuleChain<IValidationRule<T, TContext>>();
    }

    private ContextRuleEngine(RuleChain<IValidationRule<T, TContext>> chain)
    {
        _chain = chain;
    }

    public ContextRuleEngine<T, TContext> Add(IValidationRule<T, TContext> rule)
        => new(_chain.Add(rule));

    public async ValueTask<List<ValidationError>?> ExecuteAsync(T value, ValidationRun<TContext> context)
    {
        var rules = _chain.ToArray();
        List<ValidationError>? errors = null;

        foreach (var rule in rules)
        {
            var error = await rule.ValidateAsync(value, context);
            if (error == null) continue;
            errors ??= [];
            errors.Add(error.HasAttemptedValue ? error : error with { AttemptedValue = value, HasAttemptedValue = true });
        }

        return errors;
    }
}
