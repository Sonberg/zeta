using Zeta.Core;
using Zeta.Schemas;

namespace Zeta.Tests;

public class WithContextTests
{
    private record UserContext(string BannedEmail, int MaxValue);

    [Fact]
    public async Task WithContext_BasicRefine_Works()
    {
        var schema = Z.String()
            .Email()
            .WithContext<UserContext>()
            .Refine((email, ctx) => email != ctx.BannedEmail, "Email is banned", "banned_email");

        var context = new UserContext("banned@example.com", 100);

        // Valid email, not banned
        var validResult = await schema.ValidateAsync("user@example.com", context);
        Assert.True(validResult.IsSuccess);

        // Banned email
        var bannedResult = await schema.ValidateAsync("banned@example.com", context);
        Assert.False(bannedResult.IsSuccess);
        Assert.Contains(bannedResult.Errors, e => e.Code == "banned_email");
    }

    [Fact]
    public async Task WithContext_RulesTransfer_FromContextlessToContextAware()
    {
        // Rules TRANSFER when calling WithContext() - they can be added before
        var schema = Z.String()
            .Email()
            .MinLength(10)
            .WithContext<UserContext>()
            .Refine((email, ctx) => email != ctx.BannedEmail, "Email is banned");

        var context = new UserContext("banned@example.com", 100);

        // Invalid email format - inner validation should fail (rule transferred from contextless)
        var invalidEmailResult = await schema.ValidateAsync("notanemail", context);
        Assert.False(invalidEmailResult.IsSuccess);
        Assert.Contains(invalidEmailResult.Errors, e => e.Code == "email");

        // Valid email but too short (MinLength rule transferred from contextless)
        var shortResult = await schema.ValidateAsync("a@b.co", context);
        Assert.False(shortResult.IsSuccess);
        Assert.Contains(shortResult.Errors, e => e.Code == "min_length");
    }

    [Fact]
    public async Task WithContext_MultipleRefines_AllRun()
    {
        var schema = Z.String()
            .WithContext<UserContext>()
            .Refine((val, ctx) => val != ctx.BannedEmail, "Email is banned", "banned")
            .Refine((val, ctx) => val.Length <= ctx.MaxValue, "Too long", "too_long");

        var context = new UserContext("banned@example.com", 5);

        // Both should fail
        var bothFailResult = await schema.ValidateAsync("banned@example.com", context);
        Assert.False(bothFailResult.IsSuccess);
        Assert.Contains(bothFailResult.Errors, e => e.Code == "banned");
        Assert.Contains(bothFailResult.Errors, e => e.Code == "too_long");
    }

    [Fact]
    public async Task WithContext_AsyncRefine_Works()
    {
        var schema = Z.String()
            .WithContext<UserContext>()
            .RefineAsync(async (val, ctx, ct) =>
            {
                await Task.Delay(1, ct);
                return val != ctx.BannedEmail;
            }, "Email is banned", "banned");

        var context = new UserContext("banned@example.com", 100);

        var validResult = await schema.ValidateAsync("user@example.com", context);
        Assert.True(validResult.IsSuccess);

        var bannedResult = await schema.ValidateAsync("banned@example.com", context);
        Assert.False(bannedResult.IsSuccess);
        Assert.Contains(bannedResult.Errors, e => e.Code == "banned");
    }

    [Fact]
    public async Task WithContext_IntSchema_Works()
    {
        var schema = Z.Int()
            .Min(0)
            .WithContext<UserContext>()
            .Refine((val, ctx) => val <= ctx.MaxValue, "Exceeds maximum");

        var context = new UserContext("", 100);

        var validResult = await schema.ValidateAsync(50, context);
        Assert.True(validResult.IsSuccess);

        var exceedsResult = await schema.ValidateAsync(150, context);
        Assert.False(exceedsResult.IsSuccess);
    }

    [Fact]
    public async Task WithContext_WithNullable_Works()
    {
        var schema = Z.String()
            .Email()
            .WithContext<UserContext>()
            .Refine((val, ctx) => val != ctx.BannedEmail, "Email is banned")
            .Nullable();

        var context = new ValidationContext<UserContext>(
            new UserContext("banned@example.com", 100),
            ValidationExecutionContext.Empty);

        // Null should be valid
        var nullResult = await schema.ValidateAsync(null, context);
        Assert.True(nullResult.IsSuccess);

        // Valid email should pass
        var validResult = await schema.ValidateAsync("user@example.com", context);
        Assert.True(validResult.IsSuccess);

        // Banned email should fail
        var bannedResult = await schema.ValidateAsync("banned@example.com", context);
        Assert.False(bannedResult.IsSuccess);
    }

    [Fact]
    public async Task WithContext_IntWithNullable_Works()
    {
        var schema = Z.Int()
            .Min(0)
            .WithContext<UserContext>()
            .Refine((val, ctx) => val <= ctx.MaxValue, "Exceeds maximum")
            .Nullable();

        var context = new ValidationContext<UserContext>(
            new UserContext("", 100),
            ValidationExecutionContext.Empty);

        // Null should be valid
        var nullResult = await schema.ValidateAsync(null, context);
        Assert.True(nullResult.IsSuccess);

        // Valid value should pass
        var validResult = await schema.ValidateAsync(50, context);
        Assert.True(validResult.IsSuccess);

        // Exceeding value should fail
        var exceedsResult = await schema.ValidateAsync(150, context);
        Assert.False(exceedsResult.IsSuccess);
    }

    [Fact]
    public async Task WithContext_InObjectSchema_Works()
    {
        var userSchema = Z.Object<User>()
            .Field(u => u.Email, Z.String().Email())
            .WithContext<UserContext>()
            .Refine((user, ctx) => user.Email != ctx.BannedEmail, "Email is banned", "banned_email");

        var context = new UserContext("banned@example.com", 100);

        var validUser = new User("user@example.com");
        var validResult = await userSchema.ValidateAsync(validUser, context);
        Assert.True(validResult.IsSuccess);

        var bannedUser = new User("banned@example.com");
        var bannedResult = await userSchema.ValidateAsync(bannedUser, context);
        Assert.False(bannedResult.IsSuccess);
        Assert.Contains(bannedResult.Errors, e => e.Code == "banned_email");
    }

    [Fact]
    public async Task WithContext_ErrorPath_IncludesPath()
    {
        var schema = Z.String()
            .WithContext<UserContext>()
            .Refine((val, ctx) => val != ctx.BannedEmail, "Banned");

        var execution = ValidationExecutionContext.Empty.Push("user").Push("email");
        var context = new ValidationContext<UserContext>(
            new UserContext("banned@example.com", 100),
            execution);

        var result = await schema.ValidateAsync("banned@example.com", context);
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Path == "user.email");
    }

    [Fact]
    public async Task WithContext_DecimalSchema_Works()
    {
        var schema = Z.Decimal()
            .Min(0m)
            .WithContext<UserContext>()
            .Refine((val, ctx) => val <= ctx.MaxValue, "Exceeds maximum");

        var context = new UserContext("", 100);

        var validResult = await schema.ValidateAsync(50m, context);
        Assert.True(validResult.IsSuccess);

        var exceedsResult = await schema.ValidateAsync(150m, context);
        Assert.False(exceedsResult.IsSuccess);
    }

    [Fact]
    public async Task WithContext_BoolSchema_Works()
    {
        var schema = Z.Bool()
            .WithContext<UserContext>()
            .Refine((val, ctx) => ctx.MaxValue > 0 || !val, "Cannot be true when max is 0");

        var context = new UserContext("", 0);

        var falseResult = await schema.ValidateAsync(false, context);
        Assert.True(falseResult.IsSuccess);

        var trueResult = await schema.ValidateAsync(true, context);
        Assert.False(trueResult.IsSuccess);
    }

    [Fact]
    public async Task WithContext_DoubleSchema_Works()
    {
        var schema = Z.Double()
            .WithContext<UserContext>()
            .Refine((val, ctx) => val <= ctx.MaxValue, "Exceeds maximum");

        var context = new UserContext("", 100);

        var validResult = await schema.ValidateAsync(50.5, context);
        Assert.True(validResult.IsSuccess);

        var exceedsResult = await schema.ValidateAsync(150.5, context);
        Assert.False(exceedsResult.IsSuccess);
    }

    [Fact]
    public async Task WithContext_GuidSchema_Works()
    {
        var bannedId = Guid.NewGuid();
        var context = new GuidContext(bannedId);

        var schema = Z.Guid()
            .WithContext<GuidContext>()
            .Refine((val, ctx) => val != ctx.BannedId, "ID is banned");

        var validResult = await schema.ValidateAsync(Guid.NewGuid(), context);
        Assert.True(validResult.IsSuccess);

        var bannedResult = await schema.ValidateAsync(bannedId, context);
        Assert.False(bannedResult.IsSuccess);
    }

    [Fact]
    public async Task WithContext_DateTimeSchema_Works()
    {
        var context = new DateContext(DateTime.Now.AddDays(1));

        var schema = Z.DateTime()
            .WithContext<DateContext>()
            .Refine((val, ctx) => val < ctx.MaxDate, "Date too late");

        var validResult = await schema.ValidateAsync(DateTime.Now, context);
        Assert.True(validResult.IsSuccess);

        var lateResult = await schema.ValidateAsync(DateTime.Now.AddDays(2), context);
        Assert.False(lateResult.IsSuccess);
    }

    [Fact]
    public async Task WithContext_ObjectSchema_Works()
    {
        var innerSchema = Z.Object<User>()
            .Field(u => u.Email, Z.String().Email());

        var schema = innerSchema
            .WithContext<UserContext>()
            .Refine((user, ctx) => user.Email != ctx.BannedEmail, "Email is banned");

        var context = new UserContext("banned@example.com", 100);

        var validResult = await schema.ValidateAsync(new User("user@example.com"), context);
        Assert.True(validResult.IsSuccess);

        var bannedResult = await schema.ValidateAsync(new User("banned@example.com"), context);
        Assert.False(bannedResult.IsSuccess);
    }

    [Fact]
    public async Task WithContext_ArraySchema_Works()
    {
        var schema = Z.Array(Z.Int())
            .MinLength(0)
            .WithContext<UserContext>()
            .Refine((arr, ctx) => arr.Length <= ctx.MaxValue, "Too many items");

        var context = new UserContext("", 3);

        var validResult = await schema.ValidateAsync([1, 2, 3], context);
        Assert.True(validResult.IsSuccess);

        var tooManyResult = await schema.ValidateAsync([1, 2, 3, 4, 5], context);
        Assert.False(tooManyResult.IsSuccess);
    }

    [Fact]
    public async Task WithContext_ListSchema_Works()
    {
        var schema = Z.List(Z.String())
            .WithContext<UserContext>()
            .Refine((list, ctx) => list.Count <= ctx.MaxValue, "Too many items");

        var context = new UserContext("", 2);

        var validResult = await schema.ValidateAsync(new List<string>
        {
            "a",
            "b"
        }, context);
        Assert.True(validResult.IsSuccess);

        var tooManyResult = await schema.ValidateAsync(new List<string>
        {
            "a",
            "b",
            "c"
        }, context);
        Assert.False(tooManyResult.IsSuccess);
    }

    [Fact]
    public async Task WithContext_RefineWithoutContext_Works()
    {
        var schema = Z.String()
            .WithContext<UserContext>()
            .Refine(val => val.Length > 3, "Too short")
            .Refine((val, ctx) => val != ctx.BannedEmail, "Email is banned");

        var context = new UserContext("banned@example.com", 100);

        // Valid
        var validResult = await schema.ValidateAsync("user@example.com", context);
        Assert.True(validResult.IsSuccess);

        // Too short (context-free refine)
        var shortResult = await schema.ValidateAsync("ab", context);
        Assert.False(shortResult.IsSuccess);
        Assert.Contains(shortResult.Errors, e => e.Message == "Too short");

        // Banned (context-aware refine)
        var bannedResult = await schema.ValidateAsync("banned@example.com", context);
        Assert.False(bannedResult.IsSuccess);
        Assert.Contains(bannedResult.Errors, e => e.Message == "Email is banned");
    }

    [Fact]
    public async Task WithContext_RefineAsyncWithoutContext_Works()
    {
        var schema = Z.String()
            .WithContext<UserContext>()
            .RefineAsync(async (val, ct) =>
            {
                await Task.Delay(1, ct);
                return val.Length > 3;
            }, "Too short");

        var context = new UserContext("", 100);

        var validResult = await schema.ValidateAsync("valid", context);
        Assert.True(validResult.IsSuccess);

        var shortResult = await schema.ValidateAsync("ab", context);
        Assert.False(shortResult.IsSuccess);
    }

    [Fact]
    public async Task WithContext_FieldAfterWithContext_Works()
    {
        // Can add fields after WithContext
        var schema = Z.Object<User>()
            .WithContext<UserContext>()
            .Field(u => u.Email, Z.String().Email())
            .Refine((user, ctx) => user.Email != ctx.BannedEmail, "Email is banned");

        var context = new UserContext("banned@example.com", 100);

        var validResult = await schema.ValidateAsync(new User("user@example.com"), context);
        Assert.True(validResult.IsSuccess);

        // Invalid email format
        var invalidEmailResult = await schema.ValidateAsync(new User("notanemail"), context);
        Assert.False(invalidEmailResult.IsSuccess);
        Assert.Contains(invalidEmailResult.Errors, e => e.Code == "email");

        // Banned email
        var bannedResult = await schema.ValidateAsync(new User("banned@example.com"), context);
        Assert.False(bannedResult.IsSuccess);
        Assert.Contains(bannedResult.Errors, e => e.Message == "Email is banned");
    }

    [Fact]
    public async Task WithContext_FieldsTransferFromContextless()
    {
        // Fields TRANSFER when calling WithContext() - they can be added before
        var schema = Z.Object<Person>()
            .Field(p => p.Name, Z.String().MinLength(2))
            .Field(p => p.Age, Z.Int().Min(0))
            .WithContext<UserContext>()
            .Refine((person, ctx) => person.Age <= ctx.MaxValue, "Age exceeds maximum");

        var context = new UserContext("", 50);

        var validResult = await schema.ValidateAsync(new Person("John", 30), context);
        Assert.True(validResult.IsSuccess);

        // Name too short (field transferred from contextless)
        var shortNameResult = await schema.ValidateAsync(new Person("J", 30), context);
        Assert.False(shortNameResult.IsSuccess);
        Assert.Contains(shortNameResult.Errors, e => e.Path == "name");

        // Age negative (field transferred from contextless)
        var negativeAgeResult = await schema.ValidateAsync(new Person("John", -1), context);
        Assert.False(negativeAgeResult.IsSuccess);
        Assert.Contains(negativeAgeResult.Errors, e => e.Path == "age");

        // Age exceeds context max (context-aware refine added after WithContext)
        var oldResult = await schema.ValidateAsync(new Person("John", 100), context);
        Assert.False(oldResult.IsSuccess);
        Assert.Contains(oldResult.Errors, e => e.Message == "Age exceeds maximum");
    }

    [Fact]
    public async Task WithContext_WhenAfterWithContext_Works()
    {
        var schema = Z.Object<Order>()
            .WithContext<UserContext>()
            .Field(o => o.Total, Z.Decimal().Min(0))
            .When(
                o => o.Total > 100,
                then => then.Require(o => o.DiscountCode));

        var context = new UserContext("", 100);

        // Small order without discount code - valid
        var smallOrderResult = await schema.ValidateAsync(new Order(50m, null), context);
        Assert.True(smallOrderResult.IsSuccess);

        // Large order with discount code - valid
        var largeOrderWithCodeResult = await schema.ValidateAsync(new Order(150m, "SAVE10"), context);
        Assert.True(largeOrderWithCodeResult.IsSuccess);

        // Large order without discount code - fails (requires discount code)
        var largeOrderWithoutCodeResult = await schema.ValidateAsync(new Order(150m, null), context);
        Assert.False(largeOrderWithoutCodeResult.IsSuccess);
        Assert.Contains(largeOrderWithoutCodeResult.Errors, e => e.Code == "required");
    }

    private record User(string Email);
    private record Person(string Name, int Age);
    private record Order(decimal Total, string? DiscountCode);
    private record GuidContext(Guid BannedId);
    private record DateContext(DateTime MaxDate);
}