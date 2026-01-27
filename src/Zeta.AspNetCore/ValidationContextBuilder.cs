namespace Zeta.AspNetCore;

public record ValidationContextBuilder
{
    private CancellationToken? Cancellation { get; set; }

    private IServiceProvider? ServiceProvider { get; set; }

    private TimeProvider? TimeProvider { get; set; }

    public ValidationContextBuilder WithCancellation(CancellationToken cancellationToken)
    {
        return this with
        {
            Cancellation = cancellationToken
        };
    }

    public ValidationContextBuilder WithServiceProvider(IServiceProvider serviceProvider)
    {
        return this with
        {
            ServiceProvider = serviceProvider
        };
    }

    public ValidationContextBuilder WithTimeProvider(TimeProvider timeProvider)
    {
        return this with
        {
            TimeProvider = timeProvider
        };
    }

    public ValidationContext Build()
    {
        return new ValidationContext(
            timeProvider: TimeProvider ?? ServiceProvider?.GetService(typeof(TimeProvider)) as TimeProvider ?? TimeProvider.System,
            cancellationToken: Cancellation ?? CancellationToken.None);
    }
}