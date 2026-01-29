using Zeta.Schemas;

namespace Zeta.Tests;

public class CollectionSchemaTests
{
    [Fact]
    public async Task Array_ValidElements_ReturnsSuccess()
    {
        var schema = Z.Collection<int>().Each(n => n.Min(0));
        var result = await schema.ValidateAsync([1, 2, 3]);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Array_InvalidElement_ReturnsFailure()
    {
        var schema = Z.Collection<int>().Each(n => n.Min(0));
        var result = await schema.ValidateAsync([1, -1, 3]);

        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Equal("[1]", result.Errors[0].Path);
        Assert.Equal("min_value", result.Errors[0].Code);
    }

    [Fact]
    public async Task Array_MultipleInvalidElements_ReturnsAllErrors()
    {
        var schema = Z.Collection<int>().Each(n => n.Min(0));
        var result = await schema.ValidateAsync([-1, -2, 3]);

        Assert.False(result.IsSuccess);
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains(result.Errors, e => e.Path == "[0]");
        Assert.Contains(result.Errors, e => e.Path == "[1]");
    }

    [Fact]
    public async Task Array_MinLength_Valid_ReturnsSuccess()
    {
        var schema = Z.Collection<string>().MinLength(2);
        var result = await schema.ValidateAsync(["a", "b"]);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Array_MinLength_Invalid_ReturnsFailure()
    {
        var schema = Z.Collection<string>().MinLength(2);
        var result = await schema.ValidateAsync(["a"]);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "min_length");
    }

    [Fact]
    public async Task Array_MaxLength_Invalid_ReturnsFailure()
    {
        var schema = Z.Collection<string>().MaxLength(2);
        var result = await schema.ValidateAsync(["a", "b", "c"]);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "max_length");
    }

    [Fact]
    public async Task Array_NotEmpty_Invalid_ReturnsFailure()
    {
        var schema = Z.Collection<string>().NotEmpty();
        var result = await schema.ValidateAsync([]);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "min_length");
    }

    [Fact]
    public async Task List_ValidElements_ReturnsSuccess()
    {
        var schema = Z.Collection<string>().Each(s => s.Email());
        var result = await schema.ValidateAsync(["a@b.com", "c@d.com"]);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task List_InvalidElement_ReturnsFailure()
    {
        var schema = Z.Collection<string>().Each(s => s.Email());
        var result = await schema.ValidateAsync(["a@b.com", "invalid", "c@d.com"]);

        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Equal("[1]", result.Errors[0].Path);
        Assert.Equal("email", result.Errors[0].Code);
    }

    [Fact]
    public async Task List_MinLength_Invalid_ReturnsFailure()
    {
        var schema = Z.Collection<int>().MinLength(3);
        var result = await schema.ValidateAsync([1, 2]);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "min_length");
    }

    [Fact]
    public async Task Array_NestedInObject_PropagatesPath()
    {
        var schema = Z.Object<Order>()
            .Field(o => o.Items, items => items.Each(s => s.MinLength(3)));

        var order = new Order(new List<string> { "abc", "ab", "abcd" }); // "ab" is invalid
        var result = await schema.ValidateAsync(order);

        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Equal("items[1]", result.Errors[0].Path);
    }

    record Order(List<string> Items);

    // RFC 003: .Each() method tests
    [Fact]
    public async Task Each_WithRefinement_AppliesTransformation()
    {
        var schema = Z.Collection<string>()
            .Each(s => s.MinLength(3));

        var result = await schema.ValidateAsync(["abc", "abcd"]);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Each_WithRefinement_InvalidElement_ReturnsFailure()
    {
        var schema = Z.Collection<string>()
            .Each(s => s.MinLength(3));

        var result = await schema.ValidateAsync(["abc", "ab"]);

        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Equal("[1]", result.Errors[0].Path);
        Assert.Equal("min_length", result.Errors[0].Code);
    }

    [Fact]
    public async Task Each_ChainedWithCollectionValidation_AppliesBoth()
    {
        var schema = Z.Collection<int>()
            .Each(n => n.Min(0))
            .MinLength(1)
            .MaxLength(10);

        var result = await schema.ValidateAsync([1, 2, 3]);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Each_ChainedWithCollectionValidation_BothFail_ReturnsAllErrors()
    {
        var schema = Z.Collection<int>()
            .Each(n => n.Min(0))
            .MinLength(1);

        var result = await schema.ValidateAsync([-1, -2]);

        Assert.False(result.IsSuccess);
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains(result.Errors, e => e.Path == "[0]");
        Assert.Contains(result.Errors, e => e.Path == "[1]");
    }

    [Fact]
    public async Task Each_IntegrationWithObjectField_WorksCorrectly()
    {
        var schema = Z.Object<UserWithRoles>()
            .Field(u => u.Roles, roles => roles
                .Each(r => r.Refine(v => v == "Admin" || v == "User", "Invalid role"))
                .NotEmpty());

        var user = new UserWithRoles(new List<string> { "Admin", "User" });
        var result = await schema.ValidateAsync(user);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Each_IntegrationWithObjectField_InvalidRole_ReturnsFailure()
    {
        var schema = Z.Object<UserWithRoles>()
            .Field(u => u.Roles, roles => roles
                .Each(r => r.Refine(v => v == "Admin", "Must be Admin"))
                .NotEmpty());

        var user = new UserWithRoles(new List<string> { "Admin", "Reader" });
        var result = await schema.ValidateAsync(user);

        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Equal("roles[1]", result.Errors[0].Path);
        Assert.Equal("custom_error", result.Errors[0].Code);
        Assert.Equal("Must be Admin", result.Errors[0].Message);
    }

    [Fact]
    public async Task Each_MultipleTransformations_ComposesCorrectly()
    {
        var schema = Z.Collection<string>()
            .Each(s => s.MinLength(3))
            .Each(s => s.MaxLength(10));

        var result = await schema.ValidateAsync(["abc", "abcd"]);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Each_MultipleTransformations_Failure_ReturnsError()
    {
        var schema = Z.Collection<string>()
            .Each(s => s.MinLength(3))
            .Each(s => s.MaxLength(5));

        var result = await schema.ValidateAsync(["abc", "abcdefg"]);

        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Equal("[1]", result.Errors[0].Path);
        Assert.Equal("max_length", result.Errors[0].Code);
    }

    [Fact]
    public async Task Each_WithNumericTypes_WorksCorrectly()
    {
        var schema = Z.Collection<int>()
            .Each(n => n.Min(0).Max(100));

        var result = await schema.ValidateAsync([1, 50, 100]);
        Assert.True(result.IsSuccess);

        var result2 = await schema.ValidateAsync([1, 150]);
        Assert.False(result2.IsSuccess);
        Assert.Equal("[1]", result2.Errors[0].Path);
    }

    [Fact]
    public async Task Each_WithDecimal_PrecisionValidation()
    {
        var schema = Z.Collection<decimal>()
            .Each(d => d.Positive().Precision(2));

        var result = await schema.ValidateAsync([1.5m, 2.75m]);
        Assert.True(result.IsSuccess);

        var result2 = await schema.ValidateAsync([1.555m]);
        Assert.False(result2.IsSuccess);
        Assert.Equal("[0]", result2.Errors[0].Path);
        Assert.Equal("precision", result2.Errors[0].Code);
    }

    [Fact]
    public async Task Each_EmptyCollection_DoesNotValidateElements()
    {
        var schema = Z.Collection<string>()
            .Each(s => s.MinLength(100)); // Very strict, but shouldn't matter for empty collection

        var result = await schema.ValidateAsync([]);
        Assert.True(result.IsSuccess);
    }

    record UserWithRoles(List<string> Roles);

    class TestContext
    {
    }
}