namespace Zeta.Tests;

public class SchemaExtensionsMethodTests
{
    private sealed record TestContext(int MaxAge);
    private sealed record Person(string? Nickname, int? Age);

    [Fact]
    public async Task ValidateAsync_ContextAwareExtension_ReturnsTypedResult()
    {
        var schema = Z.Int()
            .Using<TestContext>()
            .Refine((value, ctx) => value <= ctx.MaxAge, "too_big");

        var success = await SchemaExtensions.ValidateAsync(schema, 5, new TestContext(10));
        Assert.True(success.IsSuccess);
        Assert.Equal(5, success.Value);

        var failure = await SchemaExtensions.ValidateAsync(schema, 15, new TestContext(10));
        Assert.False(failure.IsSuccess);
        Assert.Contains(failure.Errors, e => e.Message == "too_big");
    }

    [Fact]
    public async Task ValidateAsync_ContextlessExtension_HandlesNullableReferenceType()
    {
        var schema = Z.String().Nullable();

        var result = await SchemaExtensions.ValidateAsync(schema, (string?)null);
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task Field_PromotionExtension_IsCovered()
    {
        var fieldSchema = Z.String()
            .Using<TestContext>()
            .MinLength(3);

        var schema = Z.Object<Person>()
            .Field(x => x.Nickname, fieldSchema);

        var result = await schema.ValidateAsync(new Person("abcd", 10), new TestContext(99));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Field_ContextAwareNullableReference_IsCovered()
    {
        var schema = Z.Object<Person>().Using<TestContext>()
            .Field(x => x.Nickname, Z.String().MinLength(3));

        var result = await schema.ValidateAsync(new Person("ab", 10), new TestContext(99));
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Path == "$.nickname" && e.Code == "min_length");
    }

    [Fact]
    public async Task Field_ContextlessNullableReference_IsCovered()
    {
        var schema = Z.Object<Person>()
            .Field(x => x.Nickname, Z.String().MinLength(3));

        var result = await schema.ValidateAsync(new Person("ab", 10));
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Path == "$.nickname" && e.Code == "min_length");
    }
}
