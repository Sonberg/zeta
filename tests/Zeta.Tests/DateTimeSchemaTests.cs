namespace Zeta.Tests;

public class DateTimeSchemaTests
{
    [Fact]
    public async Task Min_Valid_ReturnsSuccess()
    {
        var minDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var schema = Z.DateTime().Min(minDate);
        var result = await schema.ValidateAsync(new DateTime(2020, 6, 15, 0, 0, 0, DateTimeKind.Utc));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Min_Invalid_ReturnsFailure()
    {
        var minDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var schema = Z.DateTime().Min(minDate);
        var result = await schema.ValidateAsync(new DateTime(2019, 12, 31, 0, 0, 0, DateTimeKind.Utc));

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "min_date");
    }

    [Fact]
    public async Task Max_Valid_ReturnsSuccess()
    {
        var maxDate = new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc);
        var schema = Z.DateTime().Max(maxDate);
        var result = await schema.ValidateAsync(new DateTime(2024, 6, 15, 0, 0, 0, DateTimeKind.Utc));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Max_Invalid_ReturnsFailure()
    {
        var maxDate = new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc);
        var schema = Z.DateTime().Max(maxDate);
        var result = await schema.ValidateAsync(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "max_date");
    }

    [Fact]
    public async Task Past_Valid_ReturnsSuccess()
    {
        var schema = Z.DateTime().Past();
        var result = await schema.ValidateAsync(DateTime.UtcNow.AddDays(-1));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Past_Invalid_ReturnsFailure()
    {
        var schema = Z.DateTime().Past();
        var result = await schema.ValidateAsync(DateTime.UtcNow.AddDays(1));

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "past");
    }

    [Fact]
    public async Task Future_Valid_ReturnsSuccess()
    {
        var schema = Z.DateTime().Future();
        var result = await schema.ValidateAsync(DateTime.UtcNow.AddDays(1));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Future_Invalid_ReturnsFailure()
    {
        var schema = Z.DateTime().Future();
        var result = await schema.ValidateAsync(DateTime.UtcNow.AddDays(-1));

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "future");
    }

    [Fact]
    public async Task Between_Valid_ReturnsSuccess()
    {
        var min = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var max = new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc);
        var schema = Z.DateTime().Between(min, max);
        var result = await schema.ValidateAsync(new DateTime(2023, 6, 15, 0, 0, 0, DateTimeKind.Utc));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Between_Invalid_ReturnsFailure()
    {
        var min = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var max = new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc);
        var schema = Z.DateTime().Between(min, max);
        var result = await schema.ValidateAsync(new DateTime(2019, 6, 15, 0, 0, 0, DateTimeKind.Utc));

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "between");
    }

    [Fact]
    public async Task Weekday_Valid_ReturnsSuccess()
    {
        var schema = Z.DateTime().Weekday();
        // Monday
        var result = await schema.ValidateAsync(new DateTime(2024, 1, 8, 0, 0, 0, DateTimeKind.Utc));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Weekday_Invalid_ReturnsFailure()
    {
        var schema = Z.DateTime().Weekday();
        // Saturday
        var result = await schema.ValidateAsync(new DateTime(2024, 1, 6, 0, 0, 0, DateTimeKind.Utc));

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "weekday");
    }

    [Fact]
    public async Task Weekend_Valid_ReturnsSuccess()
    {
        var schema = Z.DateTime().Weekend();
        // Saturday
        var result = await schema.ValidateAsync(new DateTime(2024, 1, 6, 0, 0, 0, DateTimeKind.Utc));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Weekend_Invalid_ReturnsFailure()
    {
        var schema = Z.DateTime().Weekend();
        // Monday
        var result = await schema.ValidateAsync(new DateTime(2024, 1, 8, 0, 0, 0, DateTimeKind.Utc));

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "weekend");
    }

    [Fact]
    public async Task MinAge_Valid_ReturnsSuccess()
    {
        var schema = Z.DateTime().MinAge(18);
        var result = await schema.ValidateAsync(DateTime.UtcNow.AddYears(-20));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task MinAge_Invalid_ReturnsFailure()
    {
        var schema = Z.DateTime().MinAge(18);
        var result = await schema.ValidateAsync(DateTime.UtcNow.AddYears(-16));

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "min_age");
    }

    [Fact]
    public async Task MaxAge_Valid_ReturnsSuccess()
    {
        var schema = Z.DateTime().MaxAge(65);
        var result = await schema.ValidateAsync(DateTime.UtcNow.AddYears(-50));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task MaxAge_Invalid_ReturnsFailure()
    {
        var schema = Z.DateTime().MaxAge(65);
        var result = await schema.ValidateAsync(DateTime.UtcNow.AddYears(-70));

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "max_age");
    }

    [Fact]
    public async Task WithinDays_Valid_ReturnsSuccess()
    {
        var schema = Z.DateTime().WithinDays(7);
        var result = await schema.ValidateAsync(DateTime.UtcNow.AddDays(3));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task WithinDays_Invalid_ReturnsFailure()
    {
        var schema = Z.DateTime().WithinDays(7);
        var result = await schema.ValidateAsync(DateTime.UtcNow.AddDays(10));

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "within_days");
    }

    [Fact]
    public async Task Refine_Valid_ReturnsSuccess()
    {
        var schema = Z.DateTime().Refine(d => d.Hour >= 9 && d.Hour < 17, "Must be during business hours");
        var result = await schema.ValidateAsync(new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Refine_Invalid_ReturnsFailure()
    {
        var schema = Z.DateTime().Refine(d => d.Hour >= 9 && d.Hour < 17, "Must be during business hours");
        var result = await schema.ValidateAsync(new DateTime(2024, 1, 15, 20, 0, 0, DateTimeKind.Utc));

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Message == "Must be during business hours");
    }

    [Fact]
    public async Task AllowNull_ValidatesNonNullValues()
    {
        var schema = Z.DateTime().Past().Nullable();
        var result = await schema.ValidateAsync(DateTime.Now.AddDays(-1));
        Assert.True(result.IsSuccess);
    }
}
