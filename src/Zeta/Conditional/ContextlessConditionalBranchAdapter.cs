using Zeta.Core;

namespace Zeta.Conditional;

/// <summary>
/// Adapts a contextless conditional branch to work in a context-aware environment.
/// </summary>
internal sealed class ContextlessConditionalBranchAdapter<T, TContext> : IConditionalBranch<T, TContext>
{
    private readonly IContextlessConditionalBranch<T> _inner;

    public ContextlessConditionalBranchAdapter(IContextlessConditionalBranch<T> inner)
    {
        _inner = inner;
    }

    public ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(T instance, ValidationContext<TContext> context)
    {
        return _inner.ValidateAsync(instance, context.Execution);
    }
}
