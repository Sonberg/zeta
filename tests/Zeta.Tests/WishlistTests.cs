using Zeta.Core;
using Zeta.Schemas;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Zeta.Tests;

public class WishlistTests
{
    private record AppContext(int MaxValue, string BannedCode);

    [Fact]
    public async Task Wish2_RefineAt_WithErrorCode()
    {
        var schema = Z.Object<Command>()
            .Field(x => x.Value, Z.Int())
            .RefineAt(x => x.Value, val => val < 100, "too_high", "Value is too high");

        var result = await schema.ValidateAsync(new Command(150));
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "too_high" && e.Path == "$.value");
    }

    [Fact]
    public async Task Wish1_Field_WithDelegatedSchemaFactory()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IDynamicSchemaProvider, DynamicSchemaProvider>();
        using var sp = services.BuildServiceProvider();

        var schema = Z.Object<Command>()
            .Field(x => x.Value, (input, provider) => 
                provider.GetRequiredService<IDynamicSchemaProvider>().GetSchema(input.Value > 10 ? "strict" : "loose"));

        // Loose schema allows any int
        var looseResult = await schema.ValidateAsync(new Command(5), new ValidationContext(serviceProvider: sp));
        Assert.True(looseResult.IsSuccess);

        // Strict schema requires < 50
        var strictResult = await schema.ValidateAsync(new Command(60), new ValidationContext(serviceProvider: sp));
        Assert.False(strictResult.IsSuccess);
        Assert.Contains(strictResult.Errors, e => e.Path == "$.value");
    }

    [Fact]
    public async Task Wish3_Collection_Each_WithFactory()
    {
        var schema = Z.Collection<int>()
            .Each((val, index) => index % 2 == 0 ? Z.Int().Min(0) : Z.Int().Max(0));

        // [1, -1, 2, -2] -> valid (even indices >= 0, odd indices <= 0)
        var validResult = await schema.ValidateAsync([1, -1, 2, -2]);
        Assert.True(validResult.IsSuccess);

        // [1, 1, 1, 1] -> second element (index 1) fails Max(0)
        var invalidResult = await schema.ValidateAsync([1, 1, 1, 1]);
        Assert.False(invalidResult.IsSuccess);
        Assert.Contains(invalidResult.Errors, e => e.Path == "$[1]");
    }

    [Fact]
    public async Task Wish3_Dictionary_EachValue_WithFactory()
    {
        var schema = Z.Dictionary<string, int>()
            .EachValue((val, key) => key.StartsWith("p") ? Z.Int().Min(0) : Z.Int());

        var result = await schema.ValidateAsync(new Dictionary<string, int> { ["positive"] = -1 });
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Path == "$.values[0]");
    }

    [Fact]
    public async Task Wish7_Chained_Using_Contexts()
    {
        var schema = Z.Object<Command>()
            .Using<int>(async (cmd, sp, ct) => 100) // First context: MaxValue = 100
            .Using<AppContext>(async (cmd, max, sp, ct) => 
            {
                await Task.Delay(1);
                return new AppContext(max, "BANNED");
            })
            .Refine((cmd, ctx) => cmd.Value <= ctx.MaxValue, "Too high")
            .Refine((cmd, ctx) => cmd.Code != ctx.BannedCode, "Banned code");

        // The schema now has a chained context factory. 
        // We can pass null context to trigger factory resolution.
        var validResult = await schema.ValidateAsync(new Command(50, "OK"), context: null);
        Assert.True(validResult.IsSuccess);

        var tooHighResult = await schema.ValidateAsync(new Command(150, "OK"), context: null);
        Assert.False(tooHighResult.IsSuccess);
        Assert.Contains(tooHighResult.Errors, e => e.Message == "Too high");

        var bannedResult = await schema.ValidateAsync(new Command(50, "BANNED"), context: null);
        Assert.False(bannedResult.IsSuccess);
        Assert.Contains(bannedResult.Errors, e => e.Message == "Banned code");
    }

    [Fact]
    public async Task Wish4_Conditional_Refinements_Using_If()
    {
        var schema = Z.Object<Command>()
            .Field(x => x.Value, Z.Int())
            .If(x => x.Value > 100, then => then.Refine(x => x.Value < 200, "Must be < 200 if > 100"));

        var validResult = await schema.ValidateAsync(new Command(150));
        Assert.True(validResult.IsSuccess);

        var invalidResult = await schema.ValidateAsync(new Command(250));
        Assert.False(invalidResult.IsSuccess);
    }

    private record Command(int Value, string? Code = null);

    private interface IDynamicSchemaProvider
    {
        ISchema<int> GetSchema(string type);
    }

    private class DynamicSchemaProvider : IDynamicSchemaProvider
    {
        public ISchema<int> GetSchema(string type) => type == "strict" ? Z.Int().Max(50) : Z.Int();
    }
}
