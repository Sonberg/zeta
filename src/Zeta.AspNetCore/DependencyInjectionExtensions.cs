using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Zeta;

namespace Zeta.AspNetCore;

public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Registers Zeta validation services and scans for context factories in the specified assemblies.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">The assemblies to scan for factories.</param>
    public static IServiceCollection AddZeta(this IServiceCollection services, params Assembly[] assemblies)
    {
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
