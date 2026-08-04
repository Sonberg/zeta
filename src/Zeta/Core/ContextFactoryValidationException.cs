namespace Zeta.Core;

/// <summary>
/// Thrown internally when a validation-aware context factory (<c>.Using&lt;TContext&gt;(... =&gt; Result&lt;TContext&gt;)</c>)
/// returns a failure. Carries the factory's errors so <see cref="ContextFactoryResolver.ResolveAndValidateAsync"/>
/// can surface them as a normal validation failure instead of an unhandled exception (HTTP 500).
/// Never leaves the resolver.
/// </summary>
internal sealed class ContextFactoryValidationException : Exception
{
    public IReadOnlyList<ValidationError> Errors { get; }

    public ContextFactoryValidationException(IReadOnlyList<ValidationError> errors)
        => Errors = errors;
}
