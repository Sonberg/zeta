using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Zeta.AspNetCore.Tests;

public class ValidationContextBuilderTests
{
    private sealed class FakeTimeProvider : TimeProvider
    {
    }

    [Fact]
    public void Build_DefaultsToSystemTimeAndNoCancellation()
    {
        var context = new ValidationContextBuilder().Build();

        Assert.Same(TimeProvider.System, context.TimeProvider);
        Assert.Equal(CancellationToken.None, context.CancellationToken);
    }

    [Fact]
    public void Build_UsesCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        var context = new ValidationContextBuilder()
            .WithCancellation(token)
            .Build();

        Assert.Equal(token, context.CancellationToken);
    }

    [Fact]
    public void Build_UsesServiceProviderTimeProvider()
    {
        var timeProvider = new FakeTimeProvider();
        var services = new ServiceCollection()
            .AddSingleton<TimeProvider>(timeProvider)
            .BuildServiceProvider();

        var context = new ValidationContextBuilder()
            .WithServiceProvider(services)
            .Build();

        Assert.Same(timeProvider, context.TimeProvider);
    }

    [Fact]
    public void Build_ExplicitTimeProviderOverridesServiceProvider()
    {
        var serviceProviderTimeProvider = new FakeTimeProvider();
        var explicitTimeProvider = new FakeTimeProvider();
        var services = new ServiceCollection()
            .AddSingleton<TimeProvider>(serviceProviderTimeProvider)
            .BuildServiceProvider();

        var context = new ValidationContextBuilder()
            .WithServiceProvider(services)
            .WithTimeProvider(explicitTimeProvider)
            .Build();

        Assert.Same(explicitTimeProvider, context.TimeProvider);
    }

    [Fact]
    public void Build_TypedContextCarriesDataAndSettings()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;
        var timeProvider = new FakeTimeProvider();

        var context = new ValidationContextBuilder()
            .WithCancellation(token)
            .WithTimeProvider(timeProvider)
            .Build("payload");

        Assert.Equal("payload", context.Data);
        Assert.Equal(token, context.CancellationToken);
        Assert.Same(timeProvider, context.TimeProvider);
    }

    [Fact]
    public void WithServiceProvider_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new ValidationContextBuilder().WithServiceProvider(null!));
    }

    [Fact]
    public void WithTimeProvider_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new ValidationContextBuilder().WithTimeProvider(null!));
    }

    [Fact]
    public void ImplicitOperator_BuildsValidationContext()
    {
        var timeProvider = new FakeTimeProvider();
        ValidationContext context = new ValidationContextBuilder().WithTimeProvider(timeProvider);

        Assert.Same(timeProvider, context.TimeProvider);
    }
}
