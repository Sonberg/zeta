using Zeta;

/// <summary>
/// A builder for creating <see cref="ValidationContext"/> instances with fluent configuration.
/// </summary>
public record ValidationContextBuilder
{
    private CancellationToken? Cancellation { get; set; }
    
    private IServiceProvider? ServiceProvider { get; set; }
    
    private TimeProvider? TimeProvider { get; set; }

    /// <summary>
    /// Configures the cancellation token for the validation context.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to use.</param>
    /// <returns>A new builder instance with the cancellation token configured.</returns>
    public ValidationContextBuilder WithCancellation(CancellationToken cancellationToken) => this with
    {
        Cancellation = cancellationToken
    };

    /// <summary>
    /// Configures the service provider for the validation context.
    /// </summary>
    /// <param name="serviceProvider">The service provider to use.</param>
    /// <returns>A new builder instance with the service provider configured.</returns>
    public ValidationContextBuilder WithServiceProvider(IServiceProvider serviceProvider) => this with
    {
        ServiceProvider = serviceProvider
    };

    /// <summary>
    /// Configures the time provider for the validation context.
    /// </summary>
    /// <param name="timeProvider">The time provider to use.</param>
    /// <returns>A new builder instance with the time provider configured.</returns>
    public ValidationContextBuilder WithTimeProvider(TimeProvider timeProvider)
        => this with
        {
            TimeProvider = timeProvider
        };

    /// <summary>
    /// Builds a <see cref="ValidationContext"/> from the configured values.
    /// </summary>
    /// <returns>A new <see cref="ValidationContext"/> instance.</returns>
    public ValidationContext Build()
    {
        return new ValidationContext(
            timeProvider: TimeProvider
                          ?? ServiceProvider?.GetService(typeof(TimeProvider)) as TimeProvider
                          ?? TimeProvider.System,
            cancellationToken: Cancellation ?? CancellationToken.None);
    }

    /// <summary>
    /// Builds the ValidationContext from the builder.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static implicit operator ValidationContext(ValidationContextBuilder builder) => builder.Build();
}