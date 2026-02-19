using System.Reflection;
using Zeta.Core;
using Zeta.Schemas;

namespace Zeta.Tests;

public class ContextResultTests
{
    private sealed record TestContext(int MaxValue);
    private sealed record NestedContext(int MinLength, bool Strict);
    private sealed record Payload(string Type, List<string> Tags);

    [Fact]
    public async Task ContextAwareSchema_ValidateAsync_ReturnsResultWithValueAndContext()
    {
        ISchema<int, TestContext> schema = Z.Int()
            .Using<TestContext>()
            .Refine((value, ctx) => value <= ctx.MaxValue, "too_big");

        Result<int, TestContext> result = await schema.ValidateAsync(5, new ValidationContext<TestContext>(new TestContext(10)));

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value);
        Assert.Equal(10, result.Context.MaxValue);
    }

    [Fact]
    public async Task ContextAwareSchema_ResultCanBeAssignedToResultOfT()
    {
        ISchema<int, TestContext> schema = Z.Int()
            .Using<TestContext>()
            .Refine((value, ctx) => value <= ctx.MaxValue, "too_big");

        Result<int> result = await schema.ValidateAsync(5, new ValidationContext<TestContext>(new TestContext(10)));

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value);
    }

    [Fact]
    public async Task SchemaExtensions_ContextAwareValidateAsync_ReturnsResultWithContext()
    {
        var schema = Z.Int()
            .Using<TestContext>()
            .Refine((value, ctx) => value <= ctx.MaxValue, "too_big");

        Result<int, TestContext> result = await SchemaExtensions.ValidateAsync(schema, 5, new TestContext(10));

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value);
        Assert.Equal(10, result.Context.MaxValue);
    }

    [Fact]
    public void ResultOfTContext_InheritsMonadicOperationsFromResultOfT()
    {
        Result<int, TestContext> result = Result<int, TestContext>.Success(5, new TestContext(10));

        var mapped = result.Map(x => x * 2);
        var chained = result.Then(x => Result<string>.Success($"v:{x}"));
        var matched = result.Match(
            success: x => $"ok:{x}",
            failure: _ => "fail");

        Assert.True(mapped.IsSuccess);
        Assert.Equal(10, mapped.Value);
        Assert.True(chained.IsSuccess);
        Assert.Equal("v:5", chained.Value);
        Assert.Equal("ok:5", matched);
        Assert.Equal(5, result.GetOrDefault(0));
        Assert.Equal(5, result.GetOrThrow());
    }

    [Fact]
    public void ISchema_FirstTypeParameter_IsInvariant()
    {
        var genericArgs = typeof(ISchema<,>).GetGenericArguments();
        var variance = genericArgs[0].GenericParameterAttributes & GenericParameterAttributes.VarianceMask;

        Assert.Equal(GenericParameterAttributes.None, variance);
    }

    [Fact]
    public async Task NestedContext_IfWithEach_UsesContextForElementValidation()
    {
        var schema = Z.Collection<string>()
            .Using<NestedContext>()
            .If((_, ctx) => ctx.Strict, s => s
                .Each(item => item
                    .Using<NestedContext>()
                    .Refine((value, ctx) => value.Length >= ctx.MinLength, "too_short")));

        var strictResult = await schema.ValidateAsync(["ok", "x"], new NestedContext(2, true));
        Assert.True(strictResult.IsFailure);
        Assert.Contains(strictResult.Errors, e => e.Path == "$[1]" && e.Message == "too_short");

        var skippedResult = await schema.ValidateAsync(["x"], new NestedContext(2, false));
        Assert.True(skippedResult.IsSuccess);
        Assert.False(skippedResult.Context.Strict);
    }

    [Fact]
    public async Task NestedContext_ObjectIfAndEach_ComposesAndReportsPaths()
    {
        var schema = Z.Schema<Payload>()
            .Using<NestedContext>()
            .Property(x => x.Tags, tags => tags
                .Using<NestedContext>()
                .Each(item => item
                    .Using<NestedContext>()
                    .Refine((value, ctx) => value.Length >= ctx.MinLength, "tag_too_short")))
            .If((_, ctx) => ctx.Strict, s => s
                .RefineAt(x => x.Type, x => x.Type == "admin", "type_must_be_admin"));

        var result = await schema.ValidateAsync(
            new Payload("user", new List<string> { "ok", "x" }),
            new NestedContext(2, true));

        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Path == "$.type" && e.Message == "type_must_be_admin");
        Assert.Contains(result.Errors, e => e.Path == "$.tags[1]" && e.Message == "tag_too_short");
    }
}
