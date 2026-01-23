namespace Zeta.Tests;

public class DateOnlySchemaTests
{
    [Fact]
    public async Task Min_Valid_ReturnsSuccess()
    {
        var minDate = new DateOnly(2020, 1, 1);
        var schema = Z.DateOnly().Min(minDate);
        var result = await schema.ValidateAsync(new DateOnly(2020, 6, 15));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Min_Invalid_ReturnsFailure()
    {
        var minDate = new DateOnly(2020, 1, 1);
        var schema = Z.DateOnly().Min(minDate);
        var result = await schema.ValidateAsync(new DateOnly(2019, 12, 31));

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "min_date");
    }

    [Fact]
    public async Task Max_Valid_ReturnsSuccess()
    {
        var maxDate = new DateOnly(2025, 12, 31);
        var schema = Z.DateOnly().Max(maxDate);
        var result = await schema.ValidateAsync(new DateOnly(2024, 6, 15));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Max_Invalid_ReturnsFailure()
    {
        var maxDate = new DateOnly(2025, 12, 31);
        var schema = Z.DateOnly().Max(maxDate);
        var result = await schema.ValidateAsync(new DateOnly(2026, 1, 1));

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "max_date");
    }

    [Fact]
    public async Task Past_Valid_ReturnsSuccess()
    {
        var schema = Z.DateOnly().Past();
        var result = await schema.ValidateAsync(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Past_Invalid_ReturnsFailure()
    {
        var schema = Z.DateOnly().Past();
        var result = await schema.ValidateAsync(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)));

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "past");
    }

    [Fact]
    public async Task Future_Valid_ReturnsSuccess()
    {
        var schema = Z.DateOnly().Future();
        var result = await schema.ValidateAsync(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Future_Invalid_ReturnsFailure()
    {
        var schema = Z.DateOnly().Future();
        var result = await schema.ValidateAsync(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)));

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "future");
    }

    [Fact]
    public async Task Between_Valid_ReturnsSuccess()
    {
        var min = new DateOnly(2020, 1, 1);
        var max = new DateOnly(2025, 12, 31);
        var schema = Z.DateOnly().Between(min, max);
        var result = await schema.ValidateAsync(new DateOnly(2023, 6, 15));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Between_Invalid_ReturnsFailure()
    {
        var min = new DateOnly(2020, 1, 1);
        var max = new DateOnly(2025, 12, 31);
        var schema = Z.DateOnly().Between(min, max);
        var result = await schema.ValidateAsync(new DateOnly(2019, 6, 15));

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "between");
    }

    [Fact]
    public async Task Weekday_Valid_ReturnsSuccess()
    {
        var schema = Z.DateOnly().Weekday();
        // Monday
        var result = await schema.ValidateAsync(new DateOnly(2024, 1, 8));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Weekday_Invalid_ReturnsFailure()
    {
        var schema = Z.DateOnly().Weekday();
        // Saturday
        var result = await schema.ValidateAsync(new DateOnly(2024, 1, 6));

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "weekday");
    }

    [Fact]
    public async Task Weekend_Valid_ReturnsSuccess()
    {
        var schema = Z.DateOnly().Weekend();
        // Sunday
        var result = await schema.ValidateAsync(new DateOnly(2024, 1, 7));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Weekend_Invalid_ReturnsFailure()
    {
        var schema = Z.DateOnly().Weekend();
        // Wednesday
        var result = await schema.ValidateAsync(new DateOnly(2024, 1, 10));

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "weekend");
    }

    [Fact]
    public async Task MinAge_Valid_ReturnsSuccess()
    {
        var schema = Z.DateOnly().MinAge(18);
        var result = await schema.ValidateAsync(DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20)));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task MinAge_Invalid_ReturnsFailure()
    {
        var schema = Z.DateOnly().MinAge(18);
        var result = await schema.ValidateAsync(DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-16)));

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "min_age");
    }

    [Fact]
    public async Task MaxAge_Valid_ReturnsSuccess()
    {
        var schema = Z.DateOnly().MaxAge(65);
        var result = await schema.ValidateAsync(DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-50)));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task MaxAge_Invalid_ReturnsFailure()
    {
        var schema = Z.DateOnly().MaxAge(65);
        var result = await schema.ValidateAsync(DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-70)));

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "max_age");
    }

    [Fact]
    public async Task Nullable_AllowsNull()
    {
        var schema = Z.DateOnly().Past().Nullable();
        var result = await schema.ValidateAsync(null);
        Assert.True(result.IsSuccess);
    }
}
