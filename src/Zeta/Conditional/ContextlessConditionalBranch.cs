using Zeta.Core;

namespace Zeta.Conditional;

internal sealed class ContextlessConditionalBranch<T> : IContextlessConditionalBranch<T> where T : class
{
    private readonly Func<T, bool> _condition;
    private readonly ContextlessConditionalBuilder<T> _thenBranch;
    private readonly ContextlessConditionalBuilder<T>? _elseBranch;
    private static readonly IReadOnlyList<ValidationError> EmptyErrors = [];

    public ContextlessConditionalBranch(
        Func<T, bool> condition,
        ContextlessConditionalBuilder<T> thenBranch,
        ContextlessConditionalBuilder<T>? elseBranch)
    {
        _condition = condition;
        _thenBranch = thenBranch;
        _elseBranch = elseBranch;
    }

    public async ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(T instance, ValidationContext context)
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