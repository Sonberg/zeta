# Testing with Zeta

This guide covers strategies for testing Zeta schemas, including time-based validation testing with `TimeProvider`.

---

## Unit Testing Schemas

Schemas can be tested directly without any setup:

```csharp
[Fact]
public async Task Email_InvalidFormat_ReturnsError()
{
    var schema = Z.String().Email();

    var result = await schema.ValidateAsync("not-an-email");

    Assert.True(result.IsFailure);
    Assert.Contains(result.Errors, e => e.Code == "email");
}

[Fact]
public async Task Object_ValidInput_Succeeds()
{
    var schema = Z.Object<User>()
        .Field(u => u.Email, Z.String().Email())
        .Field(u => u.Age, Z.Int().Min(18));

    var result = await schema.ValidateAsync(new User("test@example.com", 25));

    Assert.True(result.IsSuccess);
}
```

---

## Testing Time-Based Validation

Zeta supports `TimeProvider` for testing time-dependent validations like `Past()`, `Future()`, `MinAge()`, and `WithinDays()`.

### Using FakeTimeProvider

The `FakeTimeProvider` class from `Microsoft.Extensions.TimeProvider.Testing` allows you to control time in tests:

```csharp
using Microsoft.Extensions.Time.Testing;
using Zeta;

[Fact]
public async Task MinAge_WithFakeTime_ValidatesCorrectly()
{
    // Arrange - "now" is June 15, 2024
    var fakeTime = new FakeTimeProvider(
        new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero));
    var context = new ValidationContext(timeProvider: fakeTime);
    var schema = Z.DateTime().MinAge(18);

    // Someone born June 16, 2006 is not yet 18 on June 15, 2024
    var birthDate = new DateTime(2006, 6, 16, 0, 0, 0, DateTimeKind.Utc);

    // Act
    var result = await schema.ValidateAsync(birthDate, context);

    // Assert
    Assert.True(result.IsFailure);
    Assert.Contains(result.Errors, e => e.Code == "min_age");

    // Advance time by 1 day - now they're 18
    fakeTime.Advance(TimeSpan.FromDays(1));
    var newContext = new ValidationContext(timeProvider: fakeTime);

    var result2 = await schema.ValidateAsync(birthDate, newContext);
    Assert.True(result2.IsSuccess);
}
```

### Testing Future/Past Dates

```csharp
[Fact]
public async Task Future_DateInPast_Fails()
{
    var fakeTime = new FakeTimeProvider(
        new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero));
    var context = new ValidationContext(timeProvider: fakeTime);
    var schema = Z.DateTime().Future();

    var pastDate = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);

    var result = await schema.ValidateAsync(pastDate, context);

    Assert.True(result.IsFailure);
    Assert.Contains(result.Errors, e => e.Code == "future");
}

[Fact]
public async Task WithinDays_OutsideRange_Fails()
{
    var fakeTime = new FakeTimeProvider(
        new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));
    var context = new ValidationContext(timeProvider: fakeTime);
    var schema = Z.DateTime().WithinDays(7);

    var farDate = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);

    var result = await schema.ValidateAsync(farDate, context);

    Assert.True(result.IsFailure);
    Assert.Contains(result.Errors, e => e.Code == "within_days");
}
```

---

## Testing with IZetaValidator

Register `TimeProvider` in the service collection to control time in service tests:

```csharp
[Fact]
public async Task CreateUser_UnderageUser_ReturnsValidationError()
{
    // Arrange
    var fakeTime = new FakeTimeProvider(
        new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero));

    var services = new ServiceCollection();
    services.AddSingleton<TimeProvider>(fakeTime);
    services.AddScoped<IZetaValidator, ZetaValidator>();
    var serviceProvider = services.BuildServiceProvider();

var validator = serviceProvider.GetRequiredService<IZetaValidator>();
    var schema = Z.Object<UserRequest>()
        .Field(x => x.Email, Z.String().Email())
        .Field(x => x.BirthDate, Z.DateTime().MinAge(18));

    var request = new UserRequest("test@example.com", new DateTime(2010, 1, 1));

// Act
var result = await validator.ValidateAsync(request, schema);

    // Assert
Assert.True(result.IsFailure);
Assert.Contains(result.Errors, e => e.Code == "min_age");
}
```

You can also override execution options per call:

```csharp
var result = await validator.ValidateAsync(
    request,
    schema,
    b => b.WithTimeProvider(fakeTime).WithCancellation(ct));
```

---

## Integration Testing with WebApplicationFactory

For end-to-end API tests:

```csharp
public class ApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task CreateBooking_FutureDate_Succeeds()
    {
        var fakeTime = new FakeTimeProvider(
            new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));

        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton<TimeProvider>(fakeTime);
                });
            });

        var client = factory.CreateClient();

        // Date is in the "future" relative to fake time
        var response = await client.PostAsJsonAsync("/bookings", new
        {
            date = "2024-06-15"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
```

---

## Testing Context-Aware Schemas

When testing schemas that use validation context:

```csharp
[Fact]
public async Task Email_AlreadyExists_ReturnsError()
{
    var schema = Z.String()
        .Email()
        .Using<UserContext>()
        .Refine((email, ctx) => !ctx.EmailExists, "Email already taken");

    var context = new UserContext(EmailExists: true);

    var result = await schema.ValidateAsync("test@example.com", context);

    Assert.True(result.IsFailure);
    Assert.Contains(result.Errors, e => e.Message == "Email already taken");
}

[Fact]
public async Task Email_NotExists_Succeeds()
{
    var schema = Z.String()
        .Email()
        .Using<UserContext>()
        .Refine((email, ctx) => !ctx.EmailExists, "Email already taken");

    var context = new UserContext(EmailExists: false);

    var result = await schema.ValidateAsync("test@example.com", context);

    Assert.True(result.IsSuccess);
}
```

---

## Testing Custom Refinements

Test custom logic through schema results:

```csharp
[Fact]
public async Task StartsWithUpper_LowerCase_ReturnsError()
{
    var schema = Z.String()
        .Refine(
            value => value.Length > 0 && char.IsUpper(value[0]),
            "Must start with uppercase",
            "starts_upper");

    var result = await schema.ValidateAsync("hello");

    Assert.True(result.IsFailure);
    Assert.Contains(result.Errors, e => e.Code == "starts_upper");
}

[Fact]
public async Task StartsWithUpper_UpperCase_Succeeds()
{
    var schema = Z.String()
        .Refine(
            value => value.Length > 0 && char.IsUpper(value[0]),
            "Must start with uppercase",
            "starts_upper");

    var result = await schema.ValidateAsync("Hello");

    Assert.True(result.IsSuccess);
}
```

---

## Package Reference

Add the testing package for `FakeTimeProvider`:

```bash
dotnet add package Microsoft.Extensions.TimeProvider.Testing
```
