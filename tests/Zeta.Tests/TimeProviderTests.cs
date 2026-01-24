using Microsoft.Extensions.Time.Testing;
using Zeta.Core;

namespace Zeta.Tests;

public class TimeProviderTests
{
    [Fact]
    public async Task DateTimeSchema_Past_UsesFakeTimeProvider()
    {
        // Arrange - "now" is 2024-06-15
        var fakeTime = new FakeTimeProvider(new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero));
        var context = new ValidationExecutionContext(timeProvider: fakeTime);
        var schema = Z.DateTime().Past();

        // Act - date before fake "now" should pass
        var result = await schema.ValidateAsync(new DateTime(2024, 6, 14, 0, 0, 0, DateTimeKind.Utc), context);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task DateTimeSchema_Past_FailsForFutureDate()
    {
        // Arrange - "now" is 2024-06-15
        var fakeTime = new FakeTimeProvider(new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero));
        var context = new ValidationExecutionContext(timeProvider: fakeTime);
        var schema = Z.DateTime().Past();

        // Act - date after fake "now" should fail
        var result = await schema.ValidateAsync(new DateTime(2024, 6, 16, 0, 0, 0, DateTimeKind.Utc), context);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == "past");
    }

    [Fact]
    public async Task DateTimeSchema_Future_UsesFakeTimeProvider()
    {
        // Arrange - "now" is 2024-06-15
        var fakeTime = new FakeTimeProvider(new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero));
        var context = new ValidationExecutionContext(timeProvider: fakeTime);
        var schema = Z.DateTime().Future();

        // Act - date after fake "now" should pass
        var result = await schema.ValidateAsync(new DateTime(2024, 6, 16, 0, 0, 0, DateTimeKind.Utc), context);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task DateTimeSchema_Future_FailsForPastDate()
    {
        // Arrange - "now" is 2024-06-15
        var fakeTime = new FakeTimeProvider(new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero));
        var context = new ValidationExecutionContext(timeProvider: fakeTime);
        var schema = Z.DateTime().Future();

        // Act - date before fake "now" should fail
        var result = await schema.ValidateAsync(new DateTime(2024, 6, 14, 0, 0, 0, DateTimeKind.Utc), context);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == "future");
    }

    [Fact]
    public async Task DateTimeSchema_MinAge_UsesFakeTimeProvider()
    {
        // Arrange - "now" is 2024-06-15, person born 2006-06-15 would be exactly 18
        var fakeTime = new FakeTimeProvider(new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero));
        var context = new ValidationExecutionContext(timeProvider: fakeTime);
        var schema = Z.DateTime().MinAge(18);

        // Act - someone born on 2006-06-15 is exactly 18 on 2024-06-15
        var birthDate = new DateTime(2006, 6, 15, 0, 0, 0, DateTimeKind.Utc);
        var result = await schema.ValidateAsync(birthDate, context);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task DateTimeSchema_MinAge_FailsWhenTooYoung()
    {
        // Arrange - "now" is 2024-06-15
        var fakeTime = new FakeTimeProvider(new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero));
        var context = new ValidationExecutionContext(timeProvider: fakeTime);
        var schema = Z.DateTime().MinAge(18);

        // Act - someone born on 2006-06-16 is not yet 18 on 2024-06-15
        var birthDate = new DateTime(2006, 6, 16, 0, 0, 0, DateTimeKind.Utc);
        var result = await schema.ValidateAsync(birthDate, context);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == "min_age");
    }

    [Fact]
    public async Task DateTimeSchema_MinAge_PassesAfterTimeAdvances()
    {
        // Arrange - start at 2024-06-15, person born 2006-06-16 is 17
        var fakeTime = new FakeTimeProvider(new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero));
        var context = new ValidationExecutionContext(timeProvider: fakeTime);
        var schema = Z.DateTime().MinAge(18);
        var birthDate = new DateTime(2006, 6, 16, 0, 0, 0, DateTimeKind.Utc);

        // Initially fails - not yet 18
        var result1 = await schema.ValidateAsync(birthDate, context);
        Assert.True(result1.IsFailure);

        // Advance time by 1 day - now they're 18
        fakeTime.Advance(TimeSpan.FromDays(1));
        var newContext = new ValidationExecutionContext(timeProvider: fakeTime);

        // Act
        var result2 = await schema.ValidateAsync(birthDate, newContext);

        // Assert
        Assert.True(result2.IsSuccess);
    }

    [Fact]
    public async Task DateTimeSchema_WithinDays_UsesFakeTimeProvider()
    {
        // Arrange - "now" is 2024-06-15
        var fakeTime = new FakeTimeProvider(new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero));
        var context = new ValidationExecutionContext(timeProvider: fakeTime);
        var schema = Z.DateTime().WithinDays(7);

        // Act - date within 7 days of fake "now" should pass
        var result = await schema.ValidateAsync(new DateTime(2024, 6, 20, 0, 0, 0, DateTimeKind.Utc), context);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task DateTimeSchema_WithinDays_FailsOutsideRange()
    {
        // Arrange - "now" is 2024-06-15
        var fakeTime = new FakeTimeProvider(new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero));
        var context = new ValidationExecutionContext(timeProvider: fakeTime);
        var schema = Z.DateTime().WithinDays(7);

        // Act - date more than 7 days from fake "now" should fail
        var result = await schema.ValidateAsync(new DateTime(2024, 6, 30, 0, 0, 0, DateTimeKind.Utc), context);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == "within_days");
    }

    [Fact]
    public async Task DateOnlySchema_Past_UsesFakeTimeProvider()
    {
        // Arrange - "now" is 2024-06-15
        var fakeTime = new FakeTimeProvider(new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero));
        var context = new ValidationExecutionContext(timeProvider: fakeTime);
        var schema = Z.DateOnly().Past();

        // Act - date before fake "now" should pass
        var result = await schema.ValidateAsync(new DateOnly(2024, 6, 14), context);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task DateOnlySchema_Future_UsesFakeTimeProvider()
    {
        // Arrange - "now" is 2024-06-15
        var fakeTime = new FakeTimeProvider(new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero));
        var context = new ValidationExecutionContext(timeProvider: fakeTime);
        var schema = Z.DateOnly().Future();

        // Act - date after fake "now" should pass
        var result = await schema.ValidateAsync(new DateOnly(2024, 6, 16), context);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task DateOnlySchema_MinAge_UsesFakeTimeProvider()
    {
        // Arrange - "now" is 2024-06-15
        var fakeTime = new FakeTimeProvider(new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero));
        var context = new ValidationExecutionContext(timeProvider: fakeTime);
        var schema = Z.DateOnly().MinAge(21);

        // Act - someone born 2003-06-15 is exactly 21
        var result = await schema.ValidateAsync(new DateOnly(2003, 6, 15), context);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ValidationExecutionContext_PropagatesTimeProviderOnPush()
    {
        // Arrange
        var fakeTime = new FakeTimeProvider(new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero));
        var context = new ValidationExecutionContext(timeProvider: fakeTime);

        // Act
        var nested = context.Push("child");
        var indexed = nested.PushIndex(0);

        // Assert
        Assert.Same(fakeTime, nested.TimeProvider);
        Assert.Same(fakeTime, indexed.TimeProvider);
    }
}
