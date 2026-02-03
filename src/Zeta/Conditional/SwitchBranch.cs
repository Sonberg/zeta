using Zeta.Core;

namespace Zeta.Conditional;

internal sealed class SwitchBranch<T> : IContextlessConditionalBranch<T> where T : class
{
    private readonly List<(Func<T, bool> Condition, ISchema<T> Schema)> _cases;
    private readonly ISchema<T>? _default;
    private static readonly IReadOnlyList<ValidationError> EmptyErrors = [];

    public SwitchBranch(List<(Func<T, bool> Condition, ISchema<T> Schema)> cases, ISchema<T>? defaultSchema)
    {
        _cases = cases;
        _default = defaultSchema;
    }

    public async ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(T instance, ValidationContext context)
    {
        ISchema<T>? matched = null;
        foreach (var (condition, schema) in _cases)
        {
            if (condition(instance))
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
