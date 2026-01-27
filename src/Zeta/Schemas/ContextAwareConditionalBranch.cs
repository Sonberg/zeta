namespace Zeta.Schemas;

internal sealed class ContextAwareConditionalBranch<T, TContext> : IConditionalBranch<T, TContext> where T : class
{
    private readonly Func<T, TContext, bool> _condition;
    private readonly ConditionalBuilder<T, TContext> _thenBranch;
    private readonly ConditionalBuilder<T, TContext>? _elseBranch;
    private static readonly IReadOnlyList<ValidationError> EmptyErrors = Array.Empty<ValidationError>();

    public ContextAwareConditionalBranch(
        Func<T, TContext, bool> condition,
        ConditionalBuilder<T, TContext> thenBranch,
        ConditionalBuilder<T, TContext>? elseBranch)
    {
        _condition = condition;
        _thenBranch = thenBranch;
        _elseBranch = elseBranch;
    }

    public async ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(T instance, ValidationContext<TContext> context)
    {
        var branch = _condition(instance, context.Data) ? _thenBranch : _elseBranch;
        if (branch == null)
            return EmptyErrors;

        List<ValidationError>? errors = null;
        foreach (var validator in branch.Validators)
        {
            var fieldErrors = await validator.ValidateAsync(instance, context);
            if (fieldErrors.Count > 0)
            {
                errors ??= [];
                errors.AddRange(fieldErrors);
            }
        }

        return errors ?? EmptyErrors;
    }
}