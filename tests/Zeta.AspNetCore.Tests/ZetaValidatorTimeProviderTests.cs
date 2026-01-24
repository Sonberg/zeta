using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Xunit;
using Zeta;
using Zeta.AspNetCore;

namespace Zeta.AspNetCore.Tests;

public class ZetaValidatorTimeProviderTests
{
    private record UserRegistration(string Email, DateTime BirthDate);

    [Fact]
    public async Task ValidateAsync_WithFakeTimeProvider_UsesProvidedTime()
    {
        // Arrange - "now" is 2024-06-15
        var fakeTime = new FakeTimeProvider(new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero));

        var services = new ServiceCollection();
        services.AddSingleton<TimeProvider>(fakeTime);
        services.AddScoped<IZetaValidator, ZetaValidator>();
        var serviceProvider = services.BuildServiceProvider();

        var validator = serviceProvider.GetRequiredService<IZetaValidator>();
        var schema = Z.Object<UserRegistration>()
            .Field(x => x.Email, Z.String().Email())
            .Field(x => x.BirthDate, Z.DateTime().MinAge(18));

        // Someone born 2006-06-16 is 17 years old on 2024-06-15
        var request = new UserRegistration("test@example.com", new DateTime(2006, 6, 16, 0, 0, 0, DateTimeKind.Utc));

        // Act
        var result = await validator.ValidateAsync(request, schema);

        // Assert - should fail because they're not yet 18
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == "min_age");
    }

    [Fact]
    public async Task ValidateAsync_WithAdvancedTime_ChangesValidationResult()
    {
        // Arrange - start at 2024-06-15
        var fakeTime = new FakeTimeProvider(new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero));

        var services = new ServiceCollection();
        services.AddSingleton<TimeProvider>(fakeTime);
        services.AddScoped<IZetaValidator, ZetaValidator>();
        var serviceProvider = services.BuildServiceProvider();

        var schema = Z.Object<UserRegistration>()
            .Field(x => x.Email, Z.String().Email())
            .Field(x => x.BirthDate, Z.DateTime().MinAge(18));

        // Someone born 2006-06-16 is 17 on 2024-06-15, but 18 on 2024-06-16
        var request = new UserRegistration("test@example.com", new DateTime(2006, 6, 16, 0, 0, 0, DateTimeKind.Utc));

        // Initially fails
        using (var scope1 = serviceProvider.CreateScope())
        {
            var validator1 = scope1.ServiceProvider.GetRequiredService<IZetaValidator>();
            var result1 = await validator1.ValidateAsync(request, schema);
            Assert.True(result1.IsFailure);
        }

        // Advance time by 1 day
        fakeTime.Advance(TimeSpan.FromDays(1));

        // Now passes
        using (var scope2 = serviceProvider.CreateScope())
        {
            var validator2 = scope2.ServiceProvider.GetRequiredService<IZetaValidator>();
            var result2 = await validator2.ValidateAsync(request, schema);
            Assert.True(result2.IsSuccess);
        }
    }

    [Fact]
    public async Task ValidateAsync_FutureValidation_UsesFakeTimeProvider()
    {
        // Arrange - "now" is 2024-06-15
        var fakeTime = new FakeTimeProvider(new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero));

        var services = new ServiceCollection();
        services.AddSingleton<TimeProvider>(fakeTime);
        services.AddScoped<IZetaValidator, ZetaValidator>();
        var serviceProvider = services.BuildServiceProvider();

        var validator = serviceProvider.GetRequiredService<IZetaValidator>();

        // Schema requires appointment to be in the future
        var schema = Z.DateTime().Future();

        // 2024-06-16 is in the future relative to fake "now"
        var appointmentDate = new DateTime(2024, 6, 16, 10, 0, 0, DateTimeKind.Utc);

        // Act
        var result = await validator.ValidateAsync(appointmentDate, schema);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ValidateAsync_PastValidation_UsesFakeTimeProvider()
    {
        // Arrange - "now" is 2024-06-15
        var fakeTime = new FakeTimeProvider(new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero));

        var services = new ServiceCollection();
        services.AddSingleton<TimeProvider>(fakeTime);
        services.AddScoped<IZetaValidator, ZetaValidator>();
        var serviceProvider = services.BuildServiceProvider();

        var validator = serviceProvider.GetRequiredService<IZetaValidator>();

        // Schema requires date to be in the past
        var schema = Z.DateTime().Past();

        // 2024-06-14 is in the past relative to fake "now"
        var historicalDate = new DateTime(2024, 6, 14, 10, 0, 0, DateTimeKind.Utc);

        // Act
        var result = await validator.ValidateAsync(historicalDate, schema);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ValidateAsync_WithinDays_UsesFakeTimeProvider()
    {
        // Arrange - "now" is 2024-06-15
        var fakeTime = new FakeTimeProvider(new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero));

        var services = new ServiceCollection();
        services.AddSingleton<TimeProvider>(fakeTime);
        services.AddScoped<IZetaValidator, ZetaValidator>();
        var serviceProvider = services.BuildServiceProvider();

        var validator = serviceProvider.GetRequiredService<IZetaValidator>();

        // Schema requires date to be within 7 days
        var schema = Z.DateTime().WithinDays(7);

        // 2024-06-20 is 5 days from fake "now" - should pass
        var nearDate = new DateTime(2024, 6, 20, 10, 0, 0, DateTimeKind.Utc);
        var result1 = await validator.ValidateAsync(nearDate, schema);
        Assert.True(result1.IsSuccess);

        // 2024-06-30 is 15 days from fake "now" - should fail
        var farDate = new DateTime(2024, 6, 30, 10, 0, 0, DateTimeKind.Utc);
        var result2 = await validator.ValidateAsync(farDate, schema);
        Assert.True(result2.IsFailure);
        Assert.Contains(result2.Errors, e => e.Code == "within_days");
    }

    [Fact]
    public async Task ValidateAsync_WithoutTimeProvider_UsesSystemTime()
    {
        // Arrange - no TimeProvider registered, should use TimeProvider.System
        var services = new ServiceCollection();
        services.AddScoped<IZetaValidator, ZetaValidator>();
        var serviceProvider = services.BuildServiceProvider();

        var validator = serviceProvider.GetRequiredService<IZetaValidator>();

        // Schema requires date to be in the future
        var schema = Z.DateTime().Future();

        // A date far in the future should always pass
        var futureDate = DateTime.UtcNow.AddYears(10);

        // Act
        var result = await validator.ValidateAsync(futureDate, schema);

        // Assert
        Assert.True(result.IsSuccess);
    }
}
