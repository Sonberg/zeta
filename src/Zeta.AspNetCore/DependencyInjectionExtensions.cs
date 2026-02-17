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
}
