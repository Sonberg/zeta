using Zeta.Core;

namespace Zeta.Rules;

internal sealed class ContextRuleAdapter<T, TOldContext, TNewContext> : IValidationRule<T, TNewContext>
{
    private readonly IValidationRule<T, TOldContext> _rule;
    private readonly Func<T, IServiceProvider, CancellationToken, ValueTask<TOldContext>> _contextResolver;

    public ContextRuleAdapter(IValidationRule<T, TOldContext> rule, Func<T, IServiceProvider, CancellationToken, ValueTask<TOldContext>> contextResolver)
    {
        _rule = rule;
        _contextResolver = contextResolver;
    }

    public async ValueTask<ValidationError?> ValidateAsync(T value, ValidationContext<TNewContext> context)
    {
        var oldContextData = await _contextResolver(value, context.ServiceProvider!, context.CancellationToken);
        var oldContext = new ValidationContext<TOldContext>(context.Path, oldContextData, context.TimeProvider, context.CancellationToken, context.ServiceProvider);
        return await _rule.ValidateAsync(value, oldContext);
    }
}
