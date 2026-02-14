using Microsoft.Extensions.Time.Testing;

namespace Zeta.Tests;

public class ContextAwareBuiltInMethodTests
{
    private sealed record TestContext;
    private sealed record User(string Name);

    [Fact]
    public async Task BoolContextAwareMethods_AreCovered()
    {
        var isTrueSchema = Z.Bool().Using<TestContext>().IsTrue();
        var isFalseSchema = Z.Bool().Using<TestContext>().IsFalse();

        var trueResult = await isTrueSchema.ValidateAsync(true, new TestContext());
        Assert.True(trueResult.IsSuccess);

        var falseResult = await isFalseSchema.ValidateAsync(false, new TestContext());
        Assert.True(falseResult.IsSuccess);
    }

    [Fact]
    public async Task IntContextAwareMethods_AreCovered()
    {
        var schema = Z.Int().Using<TestContext>().Min(1).Max(10);

        var result = await schema.ValidateAsync(5, new TestContext());
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task DoubleContextAwareMethods_AreCovered()
    {
        var rangeSchema = Z.Double().Using<TestContext>().Min(-10).Max(10).Finite();
        var positiveSchema = Z.Double().Using<TestContext>().Positive();
        var negativeSchema = Z.Double().Using<TestContext>().Negative();

        Assert.True((await rangeSchema.ValidateAsync(1.5, new TestContext())).IsSuccess);
        Assert.True((await positiveSchema.ValidateAsync(2.0, new TestContext())).IsSuccess);
        Assert.True((await negativeSchema.ValidateAsync(-2.0, new TestContext())).IsSuccess);
    }

    [Fact]
    public async Task DecimalContextAwareMethods_AreCovered()
    {
        var rangeSchema = Z.Decimal().Using<TestContext>().Min(-10m).Max(10m).Precision(2);
        var positiveSchema = Z.Decimal().Using<TestContext>().Positive();
        var negativeSchema = Z.Decimal().Using<TestContext>().Negative();
        var multipleOfSchema = Z.Decimal().Using<TestContext>().MultipleOf(0.25m);

        Assert.True((await rangeSchema.ValidateAsync(1.25m, new TestContext())).IsSuccess);
        Assert.True((await positiveSchema.ValidateAsync(2m, new TestContext())).IsSuccess);
        Assert.True((await negativeSchema.ValidateAsync(-2m, new TestContext())).IsSuccess);
        Assert.True((await multipleOfSchema.ValidateAsync(1.5m, new TestContext())).IsSuccess);
    }

    [Fact]
    public async Task GuidContextAwareMethods_AreCovered()
    {
        var notEmptySchema = Z.Guid().Using<TestContext>().NotEmpty();
        var versionSchema = Z.Guid().Using<TestContext>().Version(4);

        Assert.True((await notEmptySchema.ValidateAsync(Guid.NewGuid(), new TestContext())).IsSuccess);
        Assert.True((await versionSchema.ValidateAsync(Guid.NewGuid(), new TestContext())).IsSuccess);
    }

    [Fact]
    public async Task StringContextAwareMethods_AreCovered()
    {
        var schema = Z.String()
            .Using<TestContext>()
            .MinLength(5)
            .MaxLength(10)
            .Length(8)
            .NotEmpty()
            .Email()
            .Contains("@")
            .EndsWith(".com")
            .StartsWith("test")
            .Regex(@"^test.*\.com$")
            .Url()
            .Uri(UriKind.Absolute)
            .Alphanumeric("ignored")
            .Uuid();

        var emailResult = await schema.ValidateAsync("test1234@example.com", new TestContext());
        Assert.False(emailResult.IsSuccess);
    }

    [Fact]
    public async Task CollectionContextAwareMethods_AreCovered()
    {
        var schema = Z.Collection<int>()
            .Using<TestContext>()
            .Each(Z.Int().Using<TestContext>().Min(1))
            .MinLength(1)
            .MaxLength(3)
            .Length(2)
            .NotEmpty();

        var result = await schema.ValidateAsync(new[] { 1, 2 }, new TestContext());
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task DateOnlyContextAwareMethods_AreCovered()
    {
        var fakeTime = new FakeTimeProvider(new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero));
        var context = new ValidationContext<TestContext>(new TestContext(), fakeTime);

        var minSchema = Z.DateOnly().Using<TestContext>().Min(new DateOnly(2024, 1, 1));
        var maxSchema = Z.DateOnly().Using<TestContext>().Max(new DateOnly(2024, 12, 31));
        var pastSchema = Z.DateOnly().Using<TestContext>().Past();
        var futureSchema = Z.DateOnly().Using<TestContext>().Future();
        var betweenSchema = Z.DateOnly().Using<TestContext>().Between(new DateOnly(2024, 1, 1), new DateOnly(2024, 12, 31));
        var weekdaySchema = Z.DateOnly().Using<TestContext>().Weekday();
        var weekendSchema = Z.DateOnly().Using<TestContext>().Weekend();
        var minAgeSchema = Z.DateOnly().Using<TestContext>().MinAge(18);
        var maxAgeSchema = Z.DateOnly().Using<TestContext>().MaxAge(80);

        Assert.True((await minSchema.ValidateAsync(new DateOnly(2024, 6, 1), context)).IsSuccess);
        Assert.True((await maxSchema.ValidateAsync(new DateOnly(2024, 6, 1), context)).IsSuccess);
        Assert.True((await pastSchema.ValidateAsync(new DateOnly(2024, 6, 1), context)).IsSuccess);
        Assert.True((await futureSchema.ValidateAsync(new DateOnly(2024, 6, 20), context)).IsSuccess);
        Assert.True((await betweenSchema.ValidateAsync(new DateOnly(2024, 6, 1), context)).IsSuccess);
        Assert.True((await weekdaySchema.ValidateAsync(new DateOnly(2024, 6, 14), context)).IsSuccess);
        Assert.True((await weekendSchema.ValidateAsync(new DateOnly(2024, 6, 16), context)).IsSuccess);
        Assert.True((await minAgeSchema.ValidateAsync(new DateOnly(1990, 1, 1), context)).IsSuccess);
        Assert.True((await maxAgeSchema.ValidateAsync(new DateOnly(1990, 1, 1), context)).IsSuccess);
    }

    [Fact]
    public async Task DateTimeContextAwareMethods_AreCovered()
    {
        var fakeTime = new FakeTimeProvider(new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero));
        var context = new ValidationContext<TestContext>(new TestContext(), fakeTime);

        var minSchema = Z.DateTime().Using<TestContext>().Min(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var maxSchema = Z.DateTime().Using<TestContext>().Max(new DateTime(2024, 12, 31, 0, 0, 0, DateTimeKind.Utc));
        var pastSchema = Z.DateTime().Using<TestContext>().Past();
        var futureSchema = Z.DateTime().Using<TestContext>().Future();
        var betweenSchema = Z.DateTime().Using<TestContext>().Between(
            new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2024, 12, 31, 0, 0, 0, DateTimeKind.Utc));
        var weekdaySchema = Z.DateTime().Using<TestContext>().Weekday();
        var weekendSchema = Z.DateTime().Using<TestContext>().Weekend();
        var withinDaysSchema = Z.DateTime().Using<TestContext>().WithinDays(10);
        var minAgeSchema = Z.DateTime().Using<TestContext>().MinAge(18);
        var maxAgeSchema = Z.DateTime().Using<TestContext>().MaxAge(80);

        Assert.True((await minSchema.ValidateAsync(new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc), context)).IsSuccess);
        Assert.True((await maxSchema.ValidateAsync(new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc), context)).IsSuccess);
        Assert.True((await pastSchema.ValidateAsync(new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc), context)).IsSuccess);
        Assert.True((await futureSchema.ValidateAsync(new DateTime(2024, 6, 20, 0, 0, 0, DateTimeKind.Utc), context)).IsSuccess);
        Assert.True((await betweenSchema.ValidateAsync(new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc), context)).IsSuccess);
        Assert.True((await weekdaySchema.ValidateAsync(new DateTime(2024, 6, 14, 0, 0, 0, DateTimeKind.Utc), context)).IsSuccess);
        Assert.True((await weekendSchema.ValidateAsync(new DateTime(2024, 6, 16, 0, 0, 0, DateTimeKind.Utc), context)).IsSuccess);
        Assert.True((await withinDaysSchema.ValidateAsync(new DateTime(2024, 6, 20, 0, 0, 0, DateTimeKind.Utc), context)).IsSuccess);
        Assert.True((await minAgeSchema.ValidateAsync(new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc), context)).IsSuccess);
        Assert.True((await maxAgeSchema.ValidateAsync(new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc), context)).IsSuccess);
    }

    [Fact]
    public async Task TimeOnlyContextAwareMethods_AreCovered()
    {
        var context = new TestContext();

        var minSchema = Z.TimeOnly().Using<TestContext>().Min(new TimeOnly(9, 0));
        var maxSchema = Z.TimeOnly().Using<TestContext>().Max(new TimeOnly(18, 0));
        var betweenSchema = Z.TimeOnly().Using<TestContext>().Between(new TimeOnly(9, 0), new TimeOnly(18, 0));
        var businessSchema = Z.TimeOnly().Using<TestContext>().BusinessHours();
        var morningSchema = Z.TimeOnly().Using<TestContext>().Morning();
        var afternoonSchema = Z.TimeOnly().Using<TestContext>().Afternoon();
        var eveningSchema = Z.TimeOnly().Using<TestContext>().Evening();

        Assert.True((await minSchema.ValidateAsync(new TimeOnly(10, 0), context)).IsSuccess);
        Assert.True((await maxSchema.ValidateAsync(new TimeOnly(10, 0), context)).IsSuccess);
        Assert.True((await betweenSchema.ValidateAsync(new TimeOnly(10, 0), context)).IsSuccess);
        Assert.True((await businessSchema.ValidateAsync(new TimeOnly(10, 0), context)).IsSuccess);
        Assert.True((await morningSchema.ValidateAsync(new TimeOnly(9, 0), context)).IsSuccess);
        Assert.True((await afternoonSchema.ValidateAsync(new TimeOnly(14, 0), context)).IsSuccess);
        Assert.True((await eveningSchema.ValidateAsync(new TimeOnly(19, 0), context)).IsSuccess);
    }

    [Fact]
    public async Task ObjectField_ContextAwareSchemaExtension_IsCovered()
    {
        var nameSchema = Z.String()
            .Using<TestContext>()
            .MinLength(3);

        var schema = SchemaExtensions.Field(
            Z.Object<User>(),
            x => x.Name,
            nameSchema);

        var result = await schema.ValidateAsync(new User("abc"), new TestContext());
        Assert.True(result.IsSuccess);
    }
}
