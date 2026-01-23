namespace Zeta.Tests;

public class DecimalSchemaTests
{
    [Fact]
    public async Task Min_Valid_ReturnsSuccess()
    {
        var schema = Z.Decimal().Min(10.5m);
        var result = await schema.ValidateAsync(10.5m);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Min_Invalid_ReturnsFailure()
    {
        var schema = Z.Decimal().Min(10.5m);
        var result = await schema.ValidateAsync(10.4m);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "min_value");
    }

    [Fact]
    public async Task Precision_Valid_ReturnsSuccess()
    {
        var schema = Z.Decimal().Precision(2);
        var result = await schema.ValidateAsync(10.55m);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Precision_Invalid_ReturnsFailure()
    {
        var schema = Z.Decimal().Precision(2);
        var result = await schema.ValidateAsync(10.555m);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "precision");
    }

    [Fact]
    public async Task MultipleOf_Valid_ReturnsSuccess()
    {
        var schema = Z.Decimal().MultipleOf(0.25m);
        var result = await schema.ValidateAsync(1.75m);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task MultipleOf_Invalid_ReturnsFailure()
    {
        var schema = Z.Decimal().MultipleOf(0.25m);
        var result = await schema.ValidateAsync(1.30m);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "multiple_of");
    }

    [Fact]
    public async Task Positive_Valid_ReturnsSuccess()
    {
        var schema = Z.Decimal().Positive();
        var result = await schema.ValidateAsync(0.01m);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Positive_Invalid_ReturnsFailure()
    {
        var schema = Z.Decimal().Positive();
        var result = await schema.ValidateAsync(0m);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "positive");
    }
}
