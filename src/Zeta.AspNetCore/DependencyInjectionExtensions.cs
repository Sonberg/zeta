using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Zeta.AspNetCore;


/// <summary>
/// Extension methods for registering Zeta validation services with dependency injection.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Registers Zeta validation services including IZetaValidator.
    /// Optionally scans for context factories in the specified assemblies.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">Optional assemblies to scan for context factories.</param>
    public static IServiceCollection AddZeta(this IServiceCollection services, params Assembly[] assemblies)
    {
        // Register the validator service
        services.AddScoped<IZetaValidator, ZetaValidator>();

        if (assemblies.Length == 0)
        {
            return services;
        }

        var factoryType = typeof(IValidationContextFactory<,>);

        var factories = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == factoryType)
                .Select(i => new { Interface = i, Implementation = t }));

        foreach (var factory in factories)
        {
            // Register as Scoped because they likely depend on Scoped services like DbContext
            services.AddScoped(factory.Interface, factory.Implementation);
        }

        return services;
    }
}

