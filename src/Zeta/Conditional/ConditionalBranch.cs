namespace Zeta.Schemas;

internal sealed class ConditionalBranch<T, TContext> : IConditionalBranch<T, TContext> where T : class
{
    private readonly Func<T, bool> _condition;
    private readonly ConditionalBuilder<T, TContext> _thenBranch;
    private readonly ConditionalBuilder<T, TContext>? _elseBranch;
    private static readonly IReadOnlyList<ValidationError> EmptyErrors = Array.Empty<ValidationError>();

    public ConditionalBranch(
        Func<T, bool> condition,
        ConditionalBuilder<T, TContext> thenBranch,
        ConditionalBuilder<T, TContext>? elseBranch)
    {
        _condition = condition;
        _thenBranch = thenBranch;
        _elseBranch = elseBranch;
    }

    public async ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(T instance, ValidationContext<TContext> context)
    {
        var branch = _condition(instance) ? _thenBranch : _elseBranch;
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