using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Zeta.AspNetCore;


/// <summary>
/// Extension methods for registering Zeta validation services with dependency injection.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Registers a scoped <see cref="IZetaValidator"/>. It does <b>not</b> discover schemas or enable
    /// automatic MVC/Minimal-API validation — schemas are plain immutable values you hold as
    /// <c>static readonly</c> fields and pass to <c>WithValidation(...)</c> or the validator explicitly.
    /// Also registers a <see cref="PathFormattingOptions"/> derived from the app's JSON naming policy so
    /// error paths match serialized property names.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public static IServiceCollection AddZeta(this IServiceCollection services)
    {
        services.AddScoped<IZetaValidator, ZetaValidator>();
        services.TryAddSingleton(sp => ResolvePathFormattingFromJsonOptions(sp) ?? PathFormattingOptions.Default);
        return services;
    }

    /// <summary>
    /// Registers a scoped <see cref="IZetaValidator"/> and configures <see cref="ZetaOptions"/> —
    /// use this overload to supply a custom <see cref="ZetaOptions.ValidationResultFactory"/> for a
    /// stable error envelope. Like the parameterless overload, it does not discover schemas.
    /// </summary>
    public static IServiceCollection AddZeta(this IServiceCollection services, Action<ZetaOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        services.Configure(configure);
        return services.AddZeta();
    }

    private static PathFormattingOptions? ResolvePathFormattingFromJsonOptions(IServiceProvider serviceProvider)
    {
        var serializerOptions = (serviceProvider.GetService(typeof(IOptions<JsonOptions>)) as IOptions<JsonOptions>)?.Value.SerializerOptions;
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
