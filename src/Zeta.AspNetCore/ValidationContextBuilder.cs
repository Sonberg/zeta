using Zeta;

public record ValidationContextBuilder
{
    private CancellationToken? Cancellation { get; set; }
    
    private IServiceProvider? ServiceProvider { get; set; }
    
    private TimeProvider? TimeProvider { get; set; }

    public ValidationContextBuilder WithCancellation(CancellationToken cancellationToken) => this with
    {
        Cancellation = cancellationToken
    };

    public ValidationContextBuilder WithServiceProvider(IServiceProvider serviceProvider) => this with
    {
        ServiceProvider = serviceProvider
    };

    public ValidationContextBuilder WithTimeProvider(TimeProvider timeProvider)
        => this with
        {
            TimeProvider = timeProvider
        };

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