namespace Zeta.Tests;

public class TimeOnlySchemaTests
{
    [Fact]
    public async Task Min_Valid_ReturnsSuccess()
    {
        var minTime = new TimeOnly(9, 0);
        var schema = Z.TimeOnly().Min(minTime);
        var result = await schema.ValidateAsync(new TimeOnly(10, 30));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Min_Invalid_ReturnsFailure()
    {
        var minTime = new TimeOnly(9, 0);
        var schema = Z.TimeOnly().Min(minTime);
        var result = await schema.ValidateAsync(new TimeOnly(8, 30));

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "min_time");
    }

    [Fact]
    public async Task Max_Valid_ReturnsSuccess()
    {
        var maxTime = new TimeOnly(17, 0);
        var schema = Z.TimeOnly().Max(maxTime);
        var result = await schema.ValidateAsync(new TimeOnly(16, 30));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Max_Invalid_ReturnsFailure()
    {
        var maxTime = new TimeOnly(17, 0);
        var schema = Z.TimeOnly().Max(maxTime);
        var result = await schema.ValidateAsync(new TimeOnly(18, 0));

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "max_time");
    }

    [Fact]
    public async Task Between_Valid_ReturnsSuccess()
    {
        var min = new TimeOnly(9, 0);
        var max = new TimeOnly(17, 0);
        var schema = Z.TimeOnly().Between(min, max);
        var result = await schema.ValidateAsync(new TimeOnly(12, 0));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Between_Invalid_ReturnsFailure()
    {
        var min = new TimeOnly(9, 0);
        var max = new TimeOnly(17, 0);
        var schema = Z.TimeOnly().Between(min, max);
        var result = await schema.ValidateAsync(new TimeOnly(20, 0));

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "between");
    }

    [Fact]
    public async Task BusinessHours_Default_Valid_ReturnsSuccess()
    {
        var schema = Z.TimeOnly().BusinessHours();
        var result = await schema.ValidateAsync(new TimeOnly(10, 30));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task BusinessHours_Default_Invalid_ReturnsFailure()
    {
        var schema = Z.TimeOnly().BusinessHours();
        var result = await schema.ValidateAsync(new TimeOnly(20, 0));

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "business_hours");
    }

    [Fact]
    public async Task BusinessHours_Custom_Valid_ReturnsSuccess()
    {
        var schema = Z.TimeOnly().BusinessHours(new TimeOnly(8, 0), new TimeOnly(18, 0));
        var result = await schema.ValidateAsync(new TimeOnly(8, 30));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Morning_Valid_ReturnsSuccess()
    {
        var schema = Z.TimeOnly().Morning();
        var result = await schema.ValidateAsync(new TimeOnly(10, 0));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Morning_Invalid_ReturnsFailure()
    {
        var schema = Z.TimeOnly().Morning();
        var result = await schema.ValidateAsync(new TimeOnly(14, 0));

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "morning");
    }

    [Fact]
    public async Task Afternoon_Valid_ReturnsSuccess()
    {
        var schema = Z.TimeOnly().Afternoon();
        var result = await schema.ValidateAsync(new TimeOnly(14, 0));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Afternoon_Invalid_ReturnsFailure()
    {
        var schema = Z.TimeOnly().Afternoon();
        var result = await schema.ValidateAsync(new TimeOnly(10, 0));

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "afternoon");
    }

    [Fact]
    public async Task Evening_Valid_ReturnsSuccess()
    {
        var schema = Z.TimeOnly().Evening();
        var result = await schema.ValidateAsync(new TimeOnly(20, 0));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Evening_Invalid_ReturnsFailure()
    {
        var schema = Z.TimeOnly().Evening();
        var result = await schema.ValidateAsync(new TimeOnly(14, 0));

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "evening");
    }

    [Fact]
    public async Task Refine_Valid_ReturnsSuccess()
    {
        // Only allow times at 15-minute intervals
        var schema = Z.TimeOnly().Refine(t => t.Minute % 15 == 0, "Must be at 15-minute interval");
        var result = await schema.ValidateAsync(new TimeOnly(10, 30));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Refine_Invalid_ReturnsFailure()
    {
        var schema = Z.TimeOnly().Refine(t => t.Minute % 15 == 0, "Must be at 15-minute interval");
        var result = await schema.ValidateAsync(new TimeOnly(10, 37));

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Message == "Must be at 15-minute interval");
    }

    [Fact]
    public async Task Nullable_AllowsNull()
    {
        var schema = Z.TimeOnly().BusinessHours().Nullable();
        var result = await schema.ValidateAsync(null);
        Assert.True(result.IsSuccess);
    }
}
