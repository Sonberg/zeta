using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Text.Json;
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

    [Fact]
    public void Build_UsesJsonOptionsPropertyNamingPolicy_ForPathRendering()
    {
        var services = new ServiceCollection();
        services.AddOptions<JsonOptions>()
            .Configure(o => o.SerializerOptions.PropertyNamingPolicy = new PrefixNamingPolicy("p_"));
        using var provider = services.BuildServiceProvider();

        var context = new ValidationContextBuilder()
            .WithServiceProvider(provider)
            .Build();

        Assert.Equal("p_FirstName", context.Push("FirstName").Path);
    }

    [Fact]
    public void Build_UsesJsonOptionsDictionaryKeyPolicy_ForPathRendering()
    {
        var services = new ServiceCollection();
        services.AddOptions<JsonOptions>()
            .Configure(o => o.SerializerOptions.DictionaryKeyPolicy = new PrefixNamingPolicy("k_"));
        using var provider = services.BuildServiceProvider();

        var context = new ValidationContextBuilder()
            .WithServiceProvider(provider)
            .Build();

        Assert.Equal("[k_Alpha]", context.PushKey("Alpha").Path);
    }

    [Theory]
    [MemberData(nameof(BuiltInPropertyPolicies))]
    public void Build_UsesBuiltInJsonPropertyNamingPolicies(JsonNamingPolicy namingPolicy, string expectedSegment)
    {
        var services = new ServiceCollection();
        services.AddOptions<JsonOptions>()
            .Configure(o => o.SerializerOptions.PropertyNamingPolicy = namingPolicy);
        using var provider = services.BuildServiceProvider();

        var context = new ValidationContextBuilder()
            .WithServiceProvider(provider)
            .Build();

        Assert.Equal(expectedSegment, context.Push("FirstName").Path);
    }

    [Theory]
    [MemberData(nameof(BuiltInDictionaryPolicies))]
    public void Build_UsesBuiltInJsonDictionaryKeyPolicies(JsonNamingPolicy namingPolicy, string expectedPath)
    {
        var services = new ServiceCollection();
        services.AddOptions<JsonOptions>()
            .Configure(o => o.SerializerOptions.DictionaryKeyPolicy = namingPolicy);
        using var provider = services.BuildServiceProvider();

        var context = new ValidationContextBuilder()
            .WithServiceProvider(provider)
            .Build();

        Assert.Equal(expectedPath, context.PushKey("FirstName").Path);
    }


    [Fact]
    public async Task ZetaValidator_ValidateAsync_Contextless_UsesBuilderContext()
    {
        var services = new ServiceCollection()
            .AddScoped<IZetaValidator, ZetaValidator>()
            .BuildServiceProvider();

        using var scope = services.CreateScope();
        var validator = scope.ServiceProvider.GetRequiredService<IZetaValidator>();
        var schema = Z.String().RefineAsync(
            (_, ct) => ValueTask.FromResult(ct.CanBeCanceled),
            "token missing",
            "token_missing");
        using var cts = new CancellationTokenSource();

        var result = await validator.ValidateAsync("abc", schema, cts.Token);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ZetaValidator_ValidateAsync_ContextAware_UsesBuilderContext()
    {
        var services = new ServiceCollection()
            .AddScoped<IZetaValidator, ZetaValidator>()
            .BuildServiceProvider();

        using var scope = services.CreateScope();
        var validator = scope.ServiceProvider.GetRequiredService<IZetaValidator>();

        var schema = Z.String()
            .Using<TestContext>((_, _, _) => ValueTask.FromResult(new TestContext(true)))
            .Refine((_, ctx) => ctx.IsStrict, "context missing", "context_missing")
            .RefineAsync((_, _, ct) => ValueTask.FromResult(ct.CanBeCanceled), "token missing", "token_missing");

        using var cts = new CancellationTokenSource();
        var result = await validator.ValidateAsync(
            "abc",
            schema,
            opt => opt
                .WithCancellation(cts.Token)
                .WithTimeProvider(new FakeTimeProvider()));

        Assert.True(result.IsSuccess);
    }

    private sealed record TestContext(bool IsStrict);

    private sealed class PrefixNamingPolicy(string prefix) : System.Text.Json.JsonNamingPolicy
    {
        public override string ConvertName(string name) => prefix + name;
    }

    public static IEnumerable<object[]> BuiltInPropertyPolicies()
    {
        yield return [JsonNamingPolicy.CamelCase, "firstName"];
        yield return [JsonNamingPolicy.SnakeCaseLower, "first_name"];
        yield return [JsonNamingPolicy.KebabCaseLower, "first-name"];
    }

    public static IEnumerable<object[]> BuiltInDictionaryPolicies()
    {
        yield return [JsonNamingPolicy.CamelCase, "[firstName]"];
        yield return [JsonNamingPolicy.SnakeCaseLower, "[first_name]"];
        yield return [JsonNamingPolicy.KebabCaseLower, "[first-name]"];
    }
}
