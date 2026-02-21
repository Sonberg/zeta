using Microsoft.Extensions.DependencyInjection;
using Zeta.Schemas;

namespace Zeta.Tests;

public class AdditionalCoverageTests
{
    private sealed record Ctx(int Value);

    private interface IAnimal;
    private sealed record Dog(int Age) : IAnimal;
    private sealed record Cat(int Lives) : IAnimal;

    [Fact]
    public async Task ContextSchema_ExplicitInterface_WithoutServiceProvider_Throws()
    {
        var schema = Z.Int().Using<Ctx>();
        var contextlessSchema = (ISchema<int>)schema;

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            contextlessSchema.ValidateAsync(1, new ValidationContext()).AsTask());
    }

    [Fact]
    public async Task ContextSchema_ExplicitInterface_WithServiceProvider_ResolvesFactory()
    {
        var schema = Z.Int()
            .Using<Ctx>((value, _, _) => ValueTask.FromResult(new Ctx(value)))
            .Refine((value, ctx) => value == ctx.Value, "mismatch");

        var contextlessSchema = (ISchema<int>)schema;
        var result = await contextlessSchema.ValidateAsync(
            7,
            new ValidationContext(serviceProvider: new ServiceCollection().BuildServiceProvider()));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ObjectContextlessSchema_If_ContextSchemaWithoutFactory_Throws()
    {
        var branch = Z.Object<Dog>().Using<Ctx>();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            Z.Object<IAnimal>().If<Dog, Ctx>(_ => true, branch));

        Assert.Contains("No context factory found", exception.Message);
    }

    [Fact]
    public void ObjectContextlessSchema_If_ContextSchemaWithMultipleFactories_Throws()
    {
        var branch = Z.Object<Dog>()
            .Using<Ctx>((dog, _, _) => ValueTask.FromResult(new Ctx(dog.Age)))
            .If(_ => true, Z.Object<Dog>().Using<Ctx>((dog, _, _) => ValueTask.FromResult(new Ctx(dog.Age + 1))));

        var exception = Assert.Throws<InvalidOperationException>(() =>
            Z.Object<IAnimal>().If<Dog, Ctx>(_ => true, branch));

        Assert.Contains("Multiple context factories found", exception.Message);
    }

    [Fact]
    public async Task CollectionSchemaExtensions_DateOnlyAndTimeOnly_Work()
    {
        var dateSchema = Z.Collection<DateOnly>().Each(d => d.Past());
        var dateResult = await dateSchema.ValidateAsync([DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))]);
        Assert.True(dateResult.IsFailure);
        Assert.Equal("$[0]", dateResult.Errors[0].PathString);

        var timeSchema = Z.Collection<TimeOnly>().Each(t => t.Morning());
        var timeResult = await timeSchema.ValidateAsync([new TimeOnly(13, 0)]);
        Assert.True(timeResult.IsFailure);
        Assert.Equal("$[0]", timeResult.Errors[0].PathString);
    }

    [Fact]
    public async Task DictionarySchemaExtensions_DateOnlyAndTimeOnly_Work()
    {
        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var keySchema = Z.Dictionary<DateOnly, int>().EachKey(k => k.Past());
        var keyResult = await keySchema.ValidateAsync(new Dictionary<DateOnly, int> { [futureDate] = 1 });
        Assert.True(keyResult.IsFailure);
        Assert.Equal("$.keys[0]", keyResult.Errors[0].PathString);

        var valueSchema = Z.Dictionary<string, TimeOnly>().EachValue(v => v.Morning());
        var valueResult = await valueSchema.ValidateAsync(new Dictionary<string, TimeOnly> { ["a"] = new(15, 0) });
        Assert.True(valueResult.IsFailure);
        Assert.Equal("$.values[0]", valueResult.Errors[0].PathString);
    }

    [Fact]
    public async Task DictionaryContextSchema_RefineEachEntry_OverloadsWork()
    {
        var context = new ValidationContext<Ctx>(new Ctx(1));

        var sync = Z.Dictionary<string, int>()
            .Using<Ctx>()
            .RefineEachEntry((_, value) => value > 1, "sync", "sync_code");
        var syncResult = await sync.ValidateAsync(new Dictionary<string, int> { ["a"] = 1 }, context);
        Assert.True(syncResult.IsFailure);
        Assert.Contains(syncResult.Errors, e => e.Code == "sync_code" && e.PathString == "$[a]");

        var syncCtx = Z.Dictionary<string, int>()
            .Using<Ctx>()
            .RefineEachEntry((_, value, ctx) => value > ctx.Value, "sync_ctx", "sync_ctx_code");
        var syncCtxResult = await syncCtx.ValidateAsync(new Dictionary<string, int> { ["a"] = 1 }, context);
        Assert.True(syncCtxResult.IsFailure);
        Assert.Contains(syncCtxResult.Errors, e => e.Code == "sync_ctx_code" && e.PathString == "$[a]");

        var asyncNoCtx = Z.Dictionary<string, int>()
            .Using<Ctx>()
            .RefineEachEntryAsync((_, value, _) => ValueTask.FromResult(value > 1), "async", "async_code");
        var asyncNoCtxResult = await asyncNoCtx.ValidateAsync(new Dictionary<string, int> { ["a"] = 1 }, context);
        Assert.True(asyncNoCtxResult.IsFailure);
        Assert.Contains(asyncNoCtxResult.Errors, e => e.Code == "async_code" && e.PathString == "$[a]");

        var asyncCtx = Z.Dictionary<string, int>()
            .Using<Ctx>()
            .RefineEachEntryAsync((_, value, ctx, _) => ValueTask.FromResult(value > ctx.Value), "async_ctx", "async_ctx_code");
        var asyncCtxResult = await asyncCtx.ValidateAsync(new Dictionary<string, int> { ["a"] = 1 }, context);
        Assert.True(asyncCtxResult.IsFailure);
        Assert.Contains(asyncCtxResult.Errors, e => e.Code == "async_ctx_code" && e.PathString == "$[a]");
    }

    [Fact]
    public void ValidationPath_InternalConcatAndRelativeTo_Work()
    {
        var prefix = ValidationPath.Root.Append(PathSegment.Property("items"));
        var path = prefix
            .Append(PathSegment.Index(2))
            .Append(PathSegment.Property("name"));
        var suffix = ValidationPath.Root.Append(PathSegment.Property("meta"));

        var concat = path.Concat(suffix);
        var relative = path.RelativeTo(prefix);

        Assert.Equal("$.items[2].name.meta", concat.ToPathString());
        Assert.Equal("$[2].name", relative.ToPathString());
    }

    [Fact]
    public async Task TypeAssertion_ToContext_Validates()
    {
        var assertion = new ContextlessTypeAssertion<IAnimal, Dog>(
            Z.Object<Dog>().Field(x => x.Age, s => s.Min(1)));

        var contextAware = assertion.ToContext<Ctx>();
        var context = new ValidationContext<Ctx>(new Ctx(0));

        var valid = await contextAware.ValidateAsync(new Dog(2), context);
        Assert.Empty(valid);

        var mismatch = await contextAware.ValidateAsync(new Cat(9), context);
        Assert.Single(mismatch);
        Assert.Equal("type_mismatch", mismatch[0].Code);
    }
}
