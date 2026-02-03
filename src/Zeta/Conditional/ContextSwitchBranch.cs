using Zeta.Core;

namespace Zeta.Conditional;

internal sealed class ContextSwitchBranch<T, TContext> : IConditionalBranch<T, TContext> where T : class
{
    private readonly List<(Func<T, TContext, bool> Condition, ISchema<T, TContext> Schema)> _cases;
    private readonly ISchema<T, TContext>? _default;
    private static readonly IReadOnlyList<ValidationError> EmptyErrors = [];

    public ContextSwitchBranch(List<(Func<T, TContext, bool> Condition, ISchema<T, TContext> Schema)> cases, ISchema<T, TContext>? defaultSchema)
    {
        _cases = cases;
        _default = defaultSchema;
    }

    public async ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(T instance, ValidationContext<TContext> context)
    {
        ISchema<T, TContext>? matched = null;
        foreach (var (condition, schema) in _cases)
        {
            if (condition(instance, context.Data))
            {
                matched = schema;
                break;
            }
        }

        matched ??= _default;
        if (matched == null)
            return EmptyErrors;

        var result = await matched.ValidateAsync(instance, context);
        return result.IsSuccess ? EmptyErrors : result.Errors;
    }
}
