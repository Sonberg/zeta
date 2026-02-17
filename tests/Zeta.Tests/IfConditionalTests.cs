using Microsoft.Extensions.DependencyInjection;
using Zeta.Core;

namespace Zeta.Tests;

public class IfConditionalTests
{
    // Test models
    record User(string Name, int Age, string? Email, string Type);
    record StrictContext(bool IsStrict);

    [Fact]
    public async Task ValueSchema_ConditionTrue_ValidatesInnerSchema()
    {
        var schema = Z.Int()
            .If(v => v >= 18, s => s.Max(65));

        var result = await schema.ValidateAsync(70);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "max_value");
    }

    [Fact]
    public async Task ValueSchema_ActionConfigure_ValidatesInnerSchema()
    {
        var schema = Z.Int()
            .If(v => v >= 18, s =>
            {
                s.Max(65);
            });

        var result = await schema.ValidateAsync(70);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "max_value");
    }

    [Fact]
    public async Task ValueSchema_ConditionFalse_SkipsInnerSchema()
    {
        var schema = Z.Int()
            .If(v => v >= 18, s => s.Max(65));

        var result = await schema.ValidateAsync(10);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ValueSchema_MultipleIfChains_AppliesCorrectGuard()
    {
        var schema = Z.Int()
            .If(v => v >= 18, s => s.Max(100))
            .If(v => v < 18, s => s.Min(0).Max(17));

        // Adult valid
        var adult = await schema.ValidateAsync(50);
        Assert.True(adult.IsSuccess);

        // Adult too high
        var adultHigh = await schema.ValidateAsync(101);
        Assert.False(adultHigh.IsSuccess);

        // Minor valid
        var minor = await schema.ValidateAsync(10);
        Assert.True(minor.IsSuccess);

        // Minor too high (18+ guard not met, minor guard applies)
        var minorHigh = await schema.ValidateAsync(17);
        Assert.True(minorHigh.IsSuccess);
    }

    [Fact]
    public async Task ObjectSchema_ConditionalFieldValidation()
    {
        var schema = Z.Object<User>()
            .If(u => u.Type == "admin", s => s
                .Field(u => u.Name, n => n.MinLength(5)));

        // Admin with short name fails
        var adminShort = await schema.ValidateAsync(new User("Jo", 30, null, "admin"));
        Assert.False(adminShort.IsSuccess);
        Assert.Contains(adminShort.Errors, e => e.Path == "$.name" && e.Code == "min_length");

        // Admin with valid name passes
        var adminOk = await schema.ValidateAsync(new User("Admin", 30, null, "admin"));
        Assert.True(adminOk.IsSuccess);
    }

    [Fact]
    public async Task ObjectSchema_ConditionFalse_SkipsValidation()
    {
        var schema = Z.Object<User>()
            .If(u => u.Type == "admin", s => s
                .Field(u => u.Name, n => n.MinLength(5)));

        // Non-admin with short name passes (condition not met)
        var result = await schema.ValidateAsync(new User("Jo", 30, null, "user"));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task CollectionSchema_ConditionalRules()
    {
        var schema = Z.Collection<string>()
            .If(items => items.Count > 0, s => s.MinLength(2));

        // Non-empty collection with < 2 items fails
        var tooFew = await schema.ValidateAsync(new List<string> { "a" });
        Assert.False(tooFew.IsSuccess);

        // Non-empty collection with >= 2 items passes
        var enough = await schema.ValidateAsync(new List<string> { "a", "b" });
        Assert.True(enough.IsSuccess);

        // Empty collection passes (condition not met)
        var empty = await schema.ValidateAsync(new List<string>());
        Assert.True(empty.IsSuccess);
    }

    [Fact]
    public async Task ContextAware_ValueOnlyPredicate()
    {
        var schema = Z.String()
            .Using<StrictContext>()
            .If(v => v.Length > 0, s => s.MinLength(3));

        var ctx = new ValidationContext<StrictContext>(new StrictContext(true));

        // Non-empty string too short
        var result = await schema.ValidateAsync("ab", ctx);
        Assert.False(result.IsSuccess);

        // Non-empty string valid
        var ok = await schema.ValidateAsync("abc", ctx);
        Assert.True(ok.IsSuccess);

        // Empty string (condition false, skipped)
        var empty = await schema.ValidateAsync("", ctx);
        Assert.True(empty.IsSuccess);
    }

    [Fact]
    public async Task ContextAware_ValueAndContextPredicate()
    {
        var schema = Z.String()
            .Using<StrictContext>()
            .If((v, ctx) => ctx.IsStrict, s => s.MinLength(10));

        var strictCtx = new ValidationContext<StrictContext>(new StrictContext(true));
        var lenientCtx = new ValidationContext<StrictContext>(new StrictContext(false));

        // Strict mode, too short
        var strictShort = await schema.ValidateAsync("abc", strictCtx);
        Assert.False(strictShort.IsSuccess);

        // Strict mode, valid
        var strictOk = await schema.ValidateAsync("abcdefghij", strictCtx);
        Assert.True(strictOk.IsSuccess);

        // Lenient mode, short is fine
        var lenientShort = await schema.ValidateAsync("abc", lenientCtx);
        Assert.True(lenientShort.IsSuccess);
    }

    [Fact]
    public async Task ContextPromotion_ValueSchema_TransfersConditionals()
    {
        var schema = Z.String()
            .If(v => v.StartsWith("A"), s => s.MinLength(5))
            .Using<StrictContext>();

        var ctx = new ValidationContext<StrictContext>(new StrictContext(false));

        // Starts with A, too short -> fails
        var result = await schema.ValidateAsync("Abc", ctx);
        Assert.False(result.IsSuccess);

        // Starts with A, long enough -> passes
        var ok = await schema.ValidateAsync("Abcde", ctx);
        Assert.True(ok.IsSuccess);

        // Doesn't start with A, short is fine
        var noA = await schema.ValidateAsync("bc", ctx);
        Assert.True(noA.IsSuccess);
    }

    [Fact]
    public async Task ContextPromotion_ObjectSchema_TransfersConditionals()
    {
        var schema = Z.Object<User>()
            .If(u => u.Type == "admin", s => s
                .Field(u => u.Name, n => n.MinLength(5)))
            .Using<StrictContext>();

        var ctx = new ValidationContext<StrictContext>(new StrictContext(false));

        // Admin with short name fails
        var result = await schema.ValidateAsync(new User("Jo", 30, null, "admin"), ctx);
        Assert.False(result.IsSuccess);

        // Non-admin passes
        var ok = await schema.ValidateAsync(new User("Jo", 30, null, "user"), ctx);
        Assert.True(ok.IsSuccess);
    }

    [Fact]
    public async Task NestedIf_InnerIfInsideBuilder()
    {
        var schema = Z.Int()
            .If(v => v >= 0, s => s
                .If(v => v >= 18, inner => inner.Max(100)));

        // Positive adult over 100 -> fails
        var result = await schema.ValidateAsync(101);
        Assert.False(result.IsSuccess);

        // Positive adult under 100 -> passes
        var ok = await schema.ValidateAsync(50);
        Assert.True(ok.IsSuccess);

        // Positive minor -> passes (outer met, inner not met)
        var minor = await schema.ValidateAsync(10);
        Assert.True(minor.IsSuccess);

        // Negative -> passes (outer not met)
        var neg = await schema.ValidateAsync(-5);
        Assert.True(neg.IsSuccess);
    }

    [Fact]
    public async Task StringSchema_If_WithStartsWith()
    {
        var schema = Z.String()
            .If(v => v.StartsWith("http"), s => s.Url());

        // Starts with http but not a valid URL
        var invalid = await schema.ValidateAsync("http not a url");
        Assert.False(invalid.IsSuccess);

        // Valid URL
        var valid = await schema.ValidateAsync("https://example.com");
        Assert.True(valid.IsSuccess);

        // Doesn't start with http, no URL validation
        var noHttp = await schema.ValidateAsync("just text");
        Assert.True(noHttp.IsSuccess);
    }

    [Fact]
    public async Task ObjectSchema_If_ContextAwareBranch_SelfResolves()
    {
        var adminSchema = Z.Object<User>()
            .Field(x => x.Name, n => n.MinLength(5))
            .Using<StrictContext>((_, _, _) => new ValueTask<StrictContext>(new StrictContext(false)))
            .Refine((_, ctx) => ctx.IsStrict, "Strict context required for admins");

        var schema = Z.Object<User>()
            .If(u => u.Type == "admin", adminSchema);

        var ctx = new ValidationContext(serviceProvider: new ServiceCollection().BuildServiceProvider());

        // Admin: factory returns strict=false, refine fails
        var admin = await schema.ValidateAsync(new User("Admin", 30, null, "admin"), ctx);
        Assert.False(admin.IsSuccess);
        Assert.Contains(admin.Errors, e => e.Message == "Strict context required for admins");

        // Non-admin: condition is false, branch skipped
        var regular = await schema.ValidateAsync(new User("RegularUser", 25, null, "user"), ctx);
        Assert.True(regular.IsSuccess);
    }

    [Fact]
    public async Task ObjectSchema_If_ContextAwareFieldSchema_SelfResolves()
    {
        var contextAwareName = Z.String()
            .Using<StrictContext>()
            .Refine((name, ctx) => !ctx.IsStrict || name.Length >= 5, "Name must be at least 5 in strict mode");

        var adminSchema = Z.Object<User>()
            .Using<StrictContext>((_, _, _) => new ValueTask<StrictContext>(new StrictContext(true)))
            .Field(x => x.Name, contextAwareName);

        var schema = Z.Object<User>()
            .If(u => u.Type == "admin", adminSchema);

        var ctx = new ValidationContext(serviceProvider: new ServiceCollection().BuildServiceProvider());

        // Admin with short name, strict mode from factory
        var strictShort = await schema.ValidateAsync(new User("Joe", 30, null, "admin"), ctx);
        Assert.False(strictShort.IsSuccess);
        Assert.Contains(strictShort.Errors, e => e.Path == "$.name" && e.Message == "Name must be at least 5 in strict mode");

        // Non-admin: branch skipped
        var regular = await schema.ValidateAsync(new User("Joe", 30, null, "user"), ctx);
        Assert.True(regular.IsSuccess);
    }
}
