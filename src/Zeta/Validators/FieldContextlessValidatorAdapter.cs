using Zeta.Core;

namespace Zeta.Validators;

/// <summary>
/// Adapts a contextless field validator to work in a context-aware environment.
/// </summary>
internal sealed class FieldContextlessValidatorAdapter<T, TContext> : IFieldContextValidator<T, TContext>
{
    private readonly IFieldContextlessValidator<T> _inner;

    public FieldContextlessValidatorAdapter(IFieldContextlessValidator<T> inner)
    {
        _inner = inner;
    }

    public ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(T instance, ValidationContext<TContext> context)
    {
        return _inner.ValidateAsync(instance, context);
    }
}
