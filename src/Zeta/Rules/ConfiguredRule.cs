using Zeta.Core;

namespace Zeta.Rules;

/// <summary>Wraps a rule and rewrites its error via an <see cref="ErrorConfig"/> (RFC 006).</summary>
internal sealed class ConfiguredRule<T> : IValidationRule<T>
{
    private readonly IValidationRule<T> _inner;
    private readonly ErrorConfig _config;

    public ConfiguredRule(IValidationRule<T> inner, ErrorConfig config)
    {
        _inner = inner;
        _config = config;
    }

    public async ValueTask<ValidationError?> ValidateAsync(T value, ValidationRun context)
        => _config.Apply(await _inner.ValidateAsync(value, context));
}

/// <summary>Context-aware variant of <see cref="ConfiguredRule{T}"/>.</summary>
internal sealed class ConfiguredRule<T, TContext> : IValidationRule<T, TContext>
{
    private readonly IValidationRule<T, TContext> _inner;
    private readonly ErrorConfig _config;

    public ConfiguredRule(IValidationRule<T, TContext> inner, ErrorConfig config)
    {
        _inner = inner;
        _config = config;
    }

    public async ValueTask<ValidationError?> ValidateAsync(T value, ValidationRun<TContext> context)
        => _config.Apply(await _inner.ValidateAsync(value, context));
}
