namespace Zeta;

/// <summary>
/// Defines a factory for creating a validation context asynchronously.
/// </summary>
/// <typeparam name="TInput">The type of the input being validated.</typeparam>
/// <typeparam name="TContext">The type of the validation context data.</typeparam>
public interface IValidationContextFactory<in TInput, TContext>
{
    /// <summary>
    /// Creates the context data asynchronously.
    /// </summary>
    Task<TContext> CreateAsync(TInput input, CancellationToken ct);
}
