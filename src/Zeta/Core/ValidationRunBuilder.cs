using Zeta.Core;

namespace Zeta;

/// <summary>
/// A builder for creating <see cref="ValidationRun"/> instances with fluent configuration.
/// Lives in the core package so validation can be executed outside ASP.NET Core (e.g. an
/// Application layer) without an HTTP dependency.
/// </summary>
public record ValidationRunBuilder
{
    private CancellationToken? Cancellation { get; set; }

    private IServiceProvider? ServiceProvider { get; set; }

    private TimeProvider? TimeProvider { get; set; }

    private PathFormattingOptions? PathOptions { get; set; }

    /// <summary>
    /// Configures the cancellation token for the validation run.
    /// </summary>
    public ValidationRunBuilder WithCancellation(CancellationToken cancellationToken) => this with
    {
        Cancellation = cancellationToken
    };

    /// <summary>
    /// Configures the service provider for the validation run.
    /// </summary>
    public ValidationRunBuilder WithServiceProvider(IServiceProvider serviceProvider) => this with
    {
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider))
    };

    /// <summary>
    /// Configures the time provider for the validation run.
    /// </summary>
    public ValidationRunBuilder WithTimeProvider(TimeProvider timeProvider)
        => this with
        {
            TimeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider))
        };

    /// <summary>
    /// Configures path formatting for the validation run.
    /// </summary>
    public ValidationRunBuilder WithPathFormatting(PathFormattingOptions pathFormattingOptions)
        => this with
        {
            PathOptions = pathFormattingOptions ?? throw new ArgumentNullException(nameof(pathFormattingOptions))
        };

    /// <summary>
    /// Builds a <see cref="ValidationRun"/> from the configured values. Path formatting resolves in
    /// order: explicit <see cref="WithPathFormatting"/>, a <see cref="PathFormattingOptions"/> registered
    /// in the service provider (ASP.NET Core registers one derived from the app's JSON naming policy via
    /// <c>AddZeta</c>), then <see cref="PathFormattingOptions.Default"/>.
    /// </summary>
    public ValidationRun Build()
    {
        var pathFormatting = PathOptions
                             ?? ServiceProvider?.GetService(typeof(PathFormattingOptions)) as PathFormattingOptions
                             ?? PathFormattingOptions.Default;

        return new ValidationRun(
            timeProvider: TimeProvider
                          ?? ServiceProvider?.GetService(typeof(TimeProvider)) as TimeProvider
                          ?? TimeProvider.System,
            cancellationToken: Cancellation ?? CancellationToken.None,
            serviceProvider: ServiceProvider,
            pathFormattingOptions: pathFormatting);
    }

    /// <summary>
    /// Builds a <see cref="ValidationRun{TData}"/> from the configured values.
    /// </summary>
    public ValidationRun<TData> Build<TData>(TData data)
    {
        var context = Build();
        return new ValidationRun<TData>(
            data,
            context.TimeProvider,
            context.CancellationToken,
            context.ServiceProvider,
            context.PathFormattingOptions);
    }

    /// <summary>
    /// Implicitly builds the <see cref="ValidationRun"/> from the builder.
    /// </summary>
    public static implicit operator ValidationRun(ValidationRunBuilder builder) => builder.Build();
}
