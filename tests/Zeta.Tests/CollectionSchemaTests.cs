namespace Zeta.Tests;

public class CollectionSchemaTests
{
    [Fact]
    public async Task Array_ValidElements_ReturnsSuccess()
    {
        var schema = Z.Collection(Z.Int().Min(0));
        var result = await schema.ValidateAsync([1, 2, 3]);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Array_InvalidElement_ReturnsFailure()
    {
        var schema = Z.Collection(Z.Int().Min(0));
        var result = await schema.ValidateAsync([1, -1, 3]);

        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Equal("[1]", result.Errors[0].Path);
        Assert.Equal("min_value", result.Errors[0].Code);
    }

    [Fact]
    public async Task Array_MultipleInvalidElements_ReturnsAllErrors()
    {
        var schema = Z.Collection(Z.Int().Min(0));
        var result = await schema.ValidateAsync([-1, -2, 3]);

        Assert.False(result.IsSuccess);
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains(result.Errors, e => e.Path == "[0]");
        Assert.Contains(result.Errors, e => e.Path == "[1]");
    }

    [Fact]
    public async Task Array_MinLength_Valid_ReturnsSuccess()
    {
        var schema = Z.Collection(Z.String()).MinLength(2);
        var result = await schema.ValidateAsync(["a", "b"]);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Array_MinLength_Invalid_ReturnsFailure()
    {
        var schema = Z.Collection(Z.String()).MinLength(2);
        var result = await schema.ValidateAsync(["a"]);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "min_length");
    }

    [Fact]
    public async Task Array_MaxLength_Invalid_ReturnsFailure()
    {
        var schema = Z.Collection(Z.String()).MaxLength(2);
        var result = await schema.ValidateAsync(["a", "b", "c"]);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "max_length");
    }

    [Fact]
    public async Task Array_NotEmpty_Invalid_ReturnsFailure()
    {
        var schema = Z.Collection(Z.String()).NotEmpty();
        var result = await schema.ValidateAsync([]);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "min_length");
    }

    [Fact]
    public async Task List_ValidElements_ReturnsSuccess()
    {
        var schema = Z.Collection(Z.String().Email());
        var result = await schema.ValidateAsync(["a@b.com", "c@d.com"]);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task List_InvalidElement_ReturnsFailure()
    {
        var schema = Z.Collection(Z.String().Email());
        var result = await schema.ValidateAsync(["a@b.com", "invalid", "c@d.com"]);

        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Equal("[1]", result.Errors[0].Path);
        Assert.Equal("email", result.Errors[0].Code);
    }

    [Fact]
    public async Task List_MinLength_Invalid_ReturnsFailure()
    {
        var schema = Z.Collection(Z.Int()).MinLength(3);
        var result = await schema.ValidateAsync([1, 2]);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "min_length");
    }

    [Fact]
    public async Task Array_NestedInObject_PropagatesPath()
    {
        var schema = Z.Object<Order>()
            .Field(o => o.Items, Z.Collection(Z.String().MinLength(3)));

        var order = new Order(["abc", "ab", "abcd"]); // "ab" is invalid
        var result = await schema.ValidateAsync(order);

        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Equal("items[1]", result.Errors[0].Path);
    }

    record Order(string[] Items);
}