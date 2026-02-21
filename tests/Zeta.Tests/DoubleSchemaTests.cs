namespace Zeta.Tests;

public class DoubleSchemaTests
{
    [Fact]
    public async Task Min_Valid_ReturnsSuccess()
    {
        var schema = Z.Double().Min(10.5);
        var result = await schema.ValidateAsync(10.5);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Min_Invalid_ReturnsFailure()
    {
        var schema = Z.Double().Min(10.5);
        var result = await schema.ValidateAsync(10.4);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "min_value");
    }

    [Fact]
    public async Task Max_Valid_ReturnsSuccess()
    {
        var schema = Z.Double().Max(100.0);
        var result = await schema.ValidateAsync(99.9);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Max_Invalid_ReturnsFailure()
    {
        var schema = Z.Double().Max(100.0);
        var result = await schema.ValidateAsync(100.1);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "max_value");
    }

    [Fact]
    public async Task Range_Valid_ReturnsSuccess()
    {
        var schema = Z.Double().Range(10.5, 100.0);
        var result = await schema.ValidateAsync(20.0);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Range_Invalid_ReturnsMinOrMaxFailure()
    {
        var schema = Z.Double().Range(10.5, 100.0);
        var below = await schema.ValidateAsync(10.4);
        var above = await schema.ValidateAsync(100.1);

        Assert.False(below.IsSuccess);
        Assert.Contains(below.Errors, e => e.Code == "min_value");
        Assert.False(above.IsSuccess);
        Assert.Contains(above.Errors, e => e.Code == "max_value");
    }

    [Fact]
    public void Range_MinGreaterThanMax_Throws()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => Z.Double().Range(2.0, 1.0));
        Assert.Equal("min", ex.ParamName);
    }

    [Fact]
    public async Task Positive_Valid_ReturnsSuccess()
    {
        var schema = Z.Double().Positive();
        var result = await schema.ValidateAsync(0.001);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Positive_Invalid_ReturnsFailure()
    {
        var schema = Z.Double().Positive();
        var result = await schema.ValidateAsync(-0.001);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "positive");
    }

    [Fact]
    public async Task Negative_Valid_ReturnsSuccess()
    {
        var schema = Z.Double().Negative();
        var result = await schema.ValidateAsync(-0.001);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Negative_Invalid_ReturnsFailure()
    {
        var schema = Z.Double().Negative();
        var result = await schema.ValidateAsync(0.0);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "negative");
    }

    [Fact]
    public async Task Finite_Valid_ReturnsSuccess()
    {
        var schema = Z.Double().Finite();
        var result = await schema.ValidateAsync(double.MaxValue);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Finite_Invalid_ReturnsFailure()
    {
        var schema = Z.Double().Finite();
        var result = await schema.ValidateAsync(double.PositiveInfinity);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "finite");
    }
}
