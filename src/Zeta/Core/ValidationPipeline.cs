namespace Zeta.Core;

/// <summary>
/// Runs an ordered list of validation stages against a value and aggregates their errors.
///
/// This is the single home of the "run every stage, never short-circuit, collect all errors"
/// invariant that object, collection, and dictionary schemas all share. Each schema keeps its own
/// stage order (object runs rules last; collection/dictionary run them first) by declaring the order
/// of the array it passes in — so the ordering is visible as data rather than an implicit call
/// sequence. The null-guard and <c>Result</c> wrapping stay in each schema's ValidateAsync because
/// they are the only genuinely type-specific parts (<c>Result&lt;T&gt;</c> vs <c>Result&lt;T, TContext&gt;</c>).
///
/// One generic runner serves both the contextless and context-aware hierarchies because
/// <see cref="ValidationRun{TData}"/> derives from <see cref="ValidationRun"/>.
/// </summary>
internal static class ValidationPipeline
{
    public static async ValueTask<List<ValidationError>?> RunAsync<TValue, TCtx>(
        TValue value,
        TCtx context,
        IReadOnlyList<Func<TValue, TCtx, ValueTask<IReadOnlyList<ValidationError>?>>> stages)
        where TCtx : ValidationRun
    {
        List<ValidationError>? errors = null;

        foreach (var stage in stages)
        {
            var stageErrors = await stage(value, context);
            if (stageErrors is null || stageErrors.Count == 0) continue;
            errors ??= [];
            errors.AddRange(stageErrors);
        }

        return errors;
    }
}
