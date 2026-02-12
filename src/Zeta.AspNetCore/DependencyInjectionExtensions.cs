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
    /// </summary>
    /// <param name="services">The service collection.</param>
    public static IServiceCollection AddZeta(this IServiceCollection services)
    {
        services.AddScoped<IZetaValidator, ZetaValidator>();
        return services;
    }

    /// <summary>
    /// Registers Zeta validation services including IZetaValidator.
    /// Assembly scanning for context factories is no longer needed.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">No longer used. Context factories are now inline delegates via .Using&lt;TContext&gt;(factory).</param>
    [Obsolete("Assembly scanning for context factories is no longer needed. Use AddZeta() without parameters.")]
    public static IServiceCollection AddZeta(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.AddScoped<IZetaValidator, ZetaValidator>();
        return services;
    }
}
