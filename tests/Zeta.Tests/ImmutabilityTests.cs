using Zeta.Core;

namespace Zeta.Tests;

/// <summary>
/// Tests that schema operations are immutable — every fluent method returns a new instance
/// without affecting the original.
/// </summary>
public class ImmutabilityTests
{
    record User(string Name, int Age, string? Email);
    record StrictContext(bool IsStrict);

    [Fact]
    public async Task SchemaBranching_DoesNotAffectOriginal()
    {
        var baseSchema = Z.String().MinLength(5);
        var branchedSchema = baseSchema.MaxLength(10);

        // baseSchema should NOT have MaxLength
        var result = await baseSchema.ValidateAsync("a]very-long-string-here");
        Assert.True(result.IsSuccess);

        // branchedSchema should have both MinLength and MaxLength
        var result2 = await branchedSchema.ValidateAsync("a-very-long-string-here");
        Assert.False(result2.IsSuccess);
    }

    [Fact]
    public async Task Nullable_ReturnsNewInstance()
    {
        var required = Z.String().MinLength(3);
        var optional = required.Nullable();

        // Null should fail on required
        var requiredResult = await required.ValidateAsync(null);
        Assert.False(requiredResult.IsSuccess);

        // Null should pass on optional
        var optionalResult = await optional.ValidateAsync(null);
        Assert.True(optionalResult.IsSuccess);
    }

    [Fact]
    public async Task If_ReturnsNewInstance()
    {
        var baseSchema = Z.Int();
        var conditionalSchema = baseSchema.If(v => v >= 18, s => s.Max(65));

        // baseSchema should not have the conditional
        var baseResult = await baseSchema.ValidateAsync(70);
        Assert.True(baseResult.IsSuccess);

        // conditionalSchema should have the conditional
        var condResult = await conditionalSchema.ValidateAsync(70);
        Assert.False(condResult.IsSuccess);
    }

    [Fact]
    public async Task ObjectSchema_Field_ReturnsNewInstance()
    {
        var baseSchema = Z.Object<User>();
        var withName = baseSchema.Field(u => u.Name, s => s.MinLength(3));

        // baseSchema should have no field validation
        var baseResult = await baseSchema.ValidateAsync(new User("", 25, null));
        Assert.True(baseResult.IsSuccess);

        // withName should validate the Name field
        var withNameResult = await withName.ValidateAsync(new User("", 25, null));
        Assert.False(withNameResult.IsSuccess);
    }

    [Fact]
    public async Task ObjectSchema_MultipleFields_ChainCorrectly()
    {
        var schema1 = Z.Object<User>()
            .Field(u => u.Name, s => s.MinLength(3));
        var schema2 = schema1
            .Field(u => u.Age, s => s.Min(18));

        // schema1 only validates Name
        var result1 = await schema1.ValidateAsync(new User("Alice", 10, null));
        Assert.True(result1.IsSuccess);

        // schema2 validates both Name and Age
        var result2 = await schema2.ValidateAsync(new User("Alice", 10, null));
        Assert.False(result2.IsSuccess);
    }

    [Fact]
    public async Task CollectionSchema_Each_ReturnsNewInstance()
    {
        var baseSchema = Z.Collection<string>().MinLength(1);
        var withEach = baseSchema.Each(Z.String().Email());

        // baseSchema should not validate elements
        var baseResult = await baseSchema.ValidateAsync(new[] { "not-an-email" });
        Assert.True(baseResult.IsSuccess);

        // withEach should validate each element
        var eachResult = await withEach.ValidateAsync(new[] { "not-an-email" });
        Assert.False(eachResult.IsSuccess);
    }

    [Fact]
    public async Task Refine_ReturnsNewInstance()
    {
        var baseSchema = Z.String();
        var refined = baseSchema.Refine(v => v.Contains('@'), "Must contain @");

        // baseSchema should not have the refinement
        var baseResult = await baseSchema.ValidateAsync("hello");
        Assert.True(baseResult.IsSuccess);

        // refined should have the refinement
        var refinedResult = await refined.ValidateAsync("hello");
        Assert.False(refinedResult.IsSuccess);
    }

    [Fact]
    public async Task ContextAware_Nullable_ReturnsNewInstance()
    {
        var required = Z.String().MinLength(3).Using<StrictContext>();
        var optional = required.Nullable();

        var ctx = new ValidationContext<StrictContext>(new StrictContext(true));

        // Null should fail on required
        var requiredResult = await required.ValidateAsync(null, ctx);
        Assert.False(requiredResult.IsSuccess);

        // Null should pass on optional
        var optionalResult = await optional.ValidateAsync(null, ctx);
        Assert.True(optionalResult.IsSuccess);
    }

    [Fact]
    public async Task ContextAware_If_ReturnsNewInstance()
    {
        var baseSchema = Z.Int().Using<StrictContext>();
        var conditionalSchema = baseSchema.If((v, ctx) => ctx.IsStrict, s => s.Max(65));

        var ctx = new ValidationContext<StrictContext>(new StrictContext(true));

        // baseSchema should not have the conditional
        var baseResult = await baseSchema.ValidateAsync(70, ctx);
        Assert.True(baseResult.IsSuccess);

        // conditionalSchema should have the conditional
        var condResult = await conditionalSchema.ValidateAsync(70, ctx);
        Assert.False(condResult.IsSuccess);
    }

    [Fact]
    public async Task SchemaReuse_SafeForMultipleBranches()
    {
        var baseSchema = Z.String().MinLength(1);
        var emailSchema = baseSchema.Email();
        var urlSchema = baseSchema.Url();

        // emailSchema validates email
        var emailResult = await emailSchema.ValidateAsync("test@example.com");
        Assert.True(emailResult.IsSuccess);
        var emailFail = await emailSchema.ValidateAsync("not-a-url");
        Assert.False(emailFail.IsSuccess);

        // urlSchema validates URL
        var urlResult = await urlSchema.ValidateAsync("https://example.com");
        Assert.True(urlResult.IsSuccess);
        var urlFail = await urlSchema.ValidateAsync("not-a-url");
        Assert.False(urlFail.IsSuccess);

        // baseSchema still only validates MinLength
        var baseResult = await baseSchema.ValidateAsync("x");
        Assert.True(baseResult.IsSuccess);
    }

    [Fact]
    public async Task CollectionSchema_Nullable_PreservedByEach()
    {
        var schema = Z.Collection<string>()
            .Nullable()
            .Each(Z.String().MinLength(1));

        // Null should pass because Nullable was set before Each
        var result = await schema.ValidateAsync(null);
        Assert.True(result.IsSuccess);
    }
}
