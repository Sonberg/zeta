using System.Collections.Generic;

namespace Zeta.Rules.Dictionary;

// Contextless: predicate sees (key, value, CancellationToken)
internal readonly struct EntryRefinement<TKey, TValue> where TKey : notnull
{
    public Func<TKey, TValue, CancellationToken, ValueTask<bool>> Predicate { get; }
    public string Message { get; }
    public string Code { get; }

    public EntryRefinement(
        Func<TKey, TValue, CancellationToken, ValueTask<bool>> predicate,
        string message,
        string code)
    {
        Predicate = predicate;
        Message = message;
        Code = code;
    }

    public EntryRefinement<TKey, TValue, TContext> ToContextAware<TContext>()
    {
        var pred = Predicate;
        return new EntryRefinement<TKey, TValue, TContext>(
            (k, v, _, ct) => pred(k, v, ct), Message, Code);
    }
}

// Context-aware: predicate sees (key, value, TContext, CancellationToken)
internal readonly struct EntryRefinement<TKey, TValue, TContext> where TKey : notnull
{
    public Func<TKey, TValue, TContext, CancellationToken, ValueTask<bool>> Predicate { get; }
    public string Message { get; }
    public string Code { get; }

    public EntryRefinement(
        Func<TKey, TValue, TContext, CancellationToken, ValueTask<bool>> predicate,
        string message,
        string code)
    {
        Predicate = predicate;
        Message = message;
        Code = code;
    }
}
