using Zeta.Core;

namespace Zeta.Validators;

internal sealed class FieldContextValidatorAdapter<TInstance, TOldContext, TNewContext> : IFieldContextValidator<TInstance, TNewContext>
{
    private readonly IFieldContextValidator<TInstance, TOldContext> _inner;
    private readonly Func<TInstance, IServiceProvider, CancellationToken, ValueTask<TOldContext>> _contextResolver;

    public FieldContextValidatorAdapter(
        IFieldContextValidator<TInstance, TOldContext> inner,
        Func<TInstance, IServiceProvider, CancellationToken, ValueTask<TOldContext>> contextResolver)
    {
        _inner = inner;
        _contextResolver = contextResolver;
    }

    public async ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(TInstance instance, ValidationContext<TNewContext> context)
    {
        var oldContextData = await _contextResolver(instance, context.ServiceProvider!, context.CancellationToken);
        var oldContext = new ValidationContext<TOldContext>(context.Path, oldContextData, context.TimeProvider, context.CancellationToken, context.ServiceProvider);
        return await _inner.ValidateAsync(instance, oldContext);
    }
}
