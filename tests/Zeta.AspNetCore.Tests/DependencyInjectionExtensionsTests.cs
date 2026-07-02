using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Zeta.AspNetCore.Tests;

public class DependencyInjectionExtensionsTests
{
    [Fact]
    public void AddZeta_RegistersIZetaValidator()
    {
        var services = new ServiceCollection();

        services.AddZeta();
        using var provider = services.BuildServiceProvider();

        var validator = provider.GetRequiredService<IZetaValidator>();

        Assert.IsType<ZetaValidator>(validator);
    }

    [Fact]
    public void AddZeta_RegistersScopedLifetime_DifferentInstancesAcrossScopes()
    {
        var services = new ServiceCollection();

        services.AddZeta();
        using var provider = services.BuildServiceProvider();

        using var scope1 = provider.CreateScope();
        using var scope2 = provider.CreateScope();

        var validator1 = scope1.ServiceProvider.GetRequiredService<IZetaValidator>();
        var validator2 = scope2.ServiceProvider.GetRequiredService<IZetaValidator>();

        Assert.NotSame(validator1, validator2);
    }

    [Fact]
    public void AddZeta_ReturnsSameServiceCollectionForChaining()
    {
        var services = new ServiceCollection();

        var result = services.AddZeta();

        Assert.Same(services, result);
    }
}
