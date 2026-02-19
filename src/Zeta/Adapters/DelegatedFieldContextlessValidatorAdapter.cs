using Zeta.Core;
using Zeta.Validators;

namespace Zeta.Adapters;

internal sealed class DelegatedFieldContextlessValidatorAdapter<T, TProperty, TContext> : IFieldContextValidator<T, TContext>
{
    private readonly IFieldContextlessValidator<T> _inner;

    public DelegatedFieldContextlessValidatorAdapter(IFieldContextlessValidator<T> inner)
    {
        _inner = inner;
    }

    public async ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(T instance, ValidationContext<TContext> context)
    {
        return await _inner.ValidateAsync(instance, context);
    }
}
