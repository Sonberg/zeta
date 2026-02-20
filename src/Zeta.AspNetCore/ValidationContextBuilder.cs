using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using Zeta;

namespace Zeta.AspNetCore;

/// <summary>
/// A builder for creating <see cref="ValidationContext"/> instances with fluent configuration.
/// </summary>
public record ValidationContextBuilder
{
    private CancellationToken? Cancellation { get; set; }
    
    private IServiceProvider? ServiceProvider { get; set; }
    
    private TimeProvider? TimeProvider { get; set; }
    
    private PathFormattingOptions? PathOptions { get; set; }

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
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider))
    };

    /// <summary>
    /// Configures the time provider for the validation context.
    /// </summary>
    /// <param name="timeProvider">The time provider to use.</param>
    /// <returns>A new builder instance with the time provider configured.</returns>
    public ValidationContextBuilder WithTimeProvider(TimeProvider timeProvider)
        => this with
        {
            TimeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider))
        };

    /// <summary>
    /// Configures path formatting for the validation context.
    /// </summary>
    /// <param name="pathFormattingOptions">The path formatting options to use.</param>
    /// <returns>A new builder instance with path formatting configured.</returns>
    public ValidationContextBuilder WithPathFormatting(PathFormattingOptions pathFormattingOptions)
        => this with
        {
            PathOptions = pathFormattingOptions ?? throw new ArgumentNullException(nameof(pathFormattingOptions))
        };

    /// <summary>
    /// Builds a <see cref="ValidationContext"/> from the configured values.
    /// </summary>
    /// <returns>A new <see cref="ValidationContext"/> instance.</returns>
    public ValidationContext Build()
    {
        var pathFormatting = PathOptions
                             ?? ServiceProvider?.GetService(typeof(PathFormattingOptions)) as PathFormattingOptions
                             ?? ResolvePathFormattingFromJsonOptions(ServiceProvider)
                             ?? PathFormattingOptions.Default;

        return new ValidationContext(
            timeProvider: TimeProvider
                          ?? ServiceProvider?.GetService(typeof(TimeProvider)) as TimeProvider
                          ?? TimeProvider.System,
            cancellationToken: Cancellation ?? CancellationToken.None,
            serviceProvider: ServiceProvider,
            pathFormattingOptions: pathFormatting);
    }

    /// <summary>
    /// Builds a <see cref="ValidationContext{TData}"/> from the configured values.
    /// </summary>
    /// <typeparam name="TData">The context data type.</typeparam>
    /// <param name="data">The context data value.</param>
    /// <returns>A new <see cref="ValidationContext{TData}"/> instance.</returns>
    public ValidationContext<TData> Build<TData>(TData data)
    {
        var context = Build();
        return new ValidationContext<TData>(
            data,
            context.TimeProvider,
            context.CancellationToken,
            context.ServiceProvider,
            context.PathFormattingOptions);
    }

    /// <summary>
    /// Builds the ValidationContext from the builder.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static implicit operator ValidationContext(ValidationContextBuilder builder) => builder.Build();

    private static PathFormattingOptions? ResolvePathFormattingFromJsonOptions(IServiceProvider? serviceProvider)
    {
        if (serviceProvider is null)
            return null;

        var httpJsonOptions = serviceProvider.GetService(typeof(IOptions<JsonOptions>)) as IOptions<JsonOptions>;
        var serializerOptions = httpJsonOptions?.Value.SerializerOptions;

        if (serializerOptions is null)
            return null;

        var propertyNamingPolicy = serializerOptions.PropertyNamingPolicy;
        var dictionaryKeyPolicy = serializerOptions.DictionaryKeyPolicy;

        return new PathFormattingOptions
        {
            PropertyNameFormatter = propertyNamingPolicy is null
                ? PathFormattingOptions.Default.PropertyNameFormatter
                : propertyNamingPolicy.ConvertName,
            DictionaryKeyFormatter = key =>
            {
                if (key is string strKey && dictionaryKeyPolicy is not null)
                    return dictionaryKeyPolicy.ConvertName(strKey);

                return key.ToString() ?? string.Empty;
            }
        };
    }
}
