using Zeta.Core;
using Zeta.Schemas;

namespace Zeta.Tests;

public class NullableSchemaTests
{
    // ==================== String Schema Nullable ====================

    [Fact]
    public async Task NullableString_NullValue_ReturnsSuccess()
    {
        var schema = Z.String().MinLength(3).Nullable();
        var result = await schema.ValidateAsync(null);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task NullableString_ValidValue_ReturnsSuccess()
    {
        var schema = Z.String().MinLength(3).Nullable();
        var result = await schema.ValidateAsync("abc");

        Assert.True(result.IsSuccess);
        Assert.Equal("abc", result.Value);
    }

    [Fact]
    public async Task NullableString_InvalidValue_ReturnsFailure()
    {
        var schema = Z.String().MinLength(3).Nullable();
        var result = await schema.ValidateAsync("ab");

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "min_length");
    }

    // ==================== Int Schema Nullable ====================

    [Fact]
    public async Task NullableInt_NullValue_ReturnsSuccess()
    {
        var schema = Z.Int().Min(0).Nullable();
        var result = await schema.ValidateAsync(null);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task NullableInt_ValidValue_ReturnsSuccess()
    {
        var schema = Z.Int().Min(0).Nullable();
        var result = await schema.ValidateAsync(42);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public async Task NullableInt_InvalidValue_ReturnsFailure()
    {
        var schema = Z.Int().Min(0).Nullable();
        var result = await schema.ValidateAsync(-1);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "min_value");
    }

    // ==================== Double Schema Nullable ====================

    [Fact]
    public async Task NullableDouble_NullValue_ReturnsSuccess()
    {
        var schema = Z.Double().Positive().Nullable();
        var result = await schema.ValidateAsync(null);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task NullableDouble_ValidValue_ReturnsSuccess()
    {
        var schema = Z.Double().Positive().Nullable();
        var result = await schema.ValidateAsync(3.14);

        Assert.True(result.IsSuccess);
        Assert.Equal(3.14, result.Value);
    }

    [Fact]
    public async Task NullableDouble_InvalidValue_ReturnsFailure()
    {
        var schema = Z.Double().Positive().Nullable();
        var result = await schema.ValidateAsync(-1.0);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "positive");
    }

    // ==================== Decimal Schema Nullable ====================

    [Fact]
    public async Task NullableDecimal_NullValue_ReturnsSuccess()
    {
        var schema = Z.Decimal().Min(0m).Nullable();
        var result = await schema.ValidateAsync(null);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task NullableDecimal_ValidValue_ReturnsSuccess()
    {
        var schema = Z.Decimal().Min(0m).Nullable();
        var result = await schema.ValidateAsync(99.99m);

        Assert.True(result.IsSuccess);
        Assert.Equal(99.99m, result.Value);
    }

    [Fact]
    public async Task NullableDecimal_InvalidValue_ReturnsFailure()
    {
        var schema = Z.Decimal().Min(0m).Nullable();
        var result = await schema.ValidateAsync(-10m);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "min_value");
    }

    // ==================== Object Schema Nullable ====================

    record Person(string Name, int Age);

    [Fact]
    public async Task NullableObject_NullValue_ReturnsSuccess()
    {
        var schema = Z.Object<Person>()
            .Field(p => p.Name, Z.String().MinLength(1))
            .Field(p => p.Age, Z.Int().Min(0))
            .Nullable();

        var result = await schema.ValidateAsync(null);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task NullableObject_ValidValue_ReturnsSuccess()
    {
        var schema = Z.Object<Person>()
            .Field(p => p.Name, Z.String().MinLength(1))
            .Field(p => p.Age, Z.Int().Min(0))
            .Nullable();

        var person = new Person("John", 30);
        var result = await schema.ValidateAsync(person);

        Assert.True(result.IsSuccess);
        Assert.Equal(person, result.Value);
    }

    [Fact]
    public async Task NullableObject_InvalidValue_ReturnsFailure()
    {
        var schema = Z.Object<Person>()
            .Field(p => p.Name, Z.String().MinLength(1))
            .Field(p => p.Age, Z.Int().Min(0))
            .Nullable();

        var person = new Person("", -1);
        var result = await schema.ValidateAsync(person);

        Assert.False(result.IsSuccess);
        Assert.Equal(2, result.Errors.Count);
    }

    // ==================== Array Schema Nullable ====================

    [Fact]
    public async Task NullableArray_NullValue_ReturnsSuccess()
    {
        var schema = Z.Array(Z.Int().Min(0)).Nullable();
        var result = await schema.ValidateAsync(null);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task NullableArray_ValidValue_ReturnsSuccess()
    {
        var schema = Z.Array(Z.Int().Min(0)).Nullable();
        var result = await schema.ValidateAsync([1, 2, 3]);

        Assert.True(result.IsSuccess);
        Assert.Equal([1, 2, 3], result.Value);
    }

    [Fact]
    public async Task NullableArray_InvalidValue_ReturnsFailure()
    {
        var schema = Z.Array(Z.Int().Min(0)).Nullable();
        var result = await schema.ValidateAsync([1, -1, 3]);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Path == "[1]");
    }

    // ==================== List Schema Nullable ====================

    [Fact]
    public async Task NullableList_NullValue_ReturnsSuccess()
    {
        var schema = Z.List(Z.String().Email()).Nullable();
        var result = await schema.ValidateAsync(null);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task NullableList_ValidValue_ReturnsSuccess()
    {
        var schema = Z.List(Z.String().Email()).Nullable();
        var result = await schema.ValidateAsync(["a@b.com", "c@d.com"]);

        Assert.True(result.IsSuccess);
        Assert.Equal(["a@b.com", "c@d.com"], result.Value);
    }

    [Fact]
    public async Task NullableList_InvalidValue_ReturnsFailure()
    {
        var schema = Z.List(Z.String().Email()).Nullable();
        var result = await schema.ValidateAsync(["a@b.com", "invalid"]);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Path == "[1]" && e.Code == "email");
    }

    // ==================== Context-Aware Nullable ====================

    record LimitContext(int MaxValue);

    [Fact]
    public async Task NullableWithContext_NullValue_ReturnsSuccess()
    {
        var schema = Z.Int<LimitContext>()
            .Refine((val, ctx) => val <= ctx.MaxValue, "Exceeds limit")
            .Nullable();

        var context = new ValidationContext<LimitContext>(
            new LimitContext(100),
            ValidationExecutionContext.Empty);

        var result = await schema.ValidateAsync(null, context);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task NullableWithContext_ValidValue_ReturnsSuccess()
    {
        var schema = Z.Int<LimitContext>()
            .Refine((val, ctx) => val <= ctx.MaxValue, "Exceeds limit")
            .Nullable();

        var context = new ValidationContext<LimitContext>(
            new LimitContext(100),
            ValidationExecutionContext.Empty);

        var result = await schema.ValidateAsync(50, context);

        Assert.True(result.IsSuccess);
        Assert.Equal(50, result.Value);
    }

    [Fact]
    public async Task NullableWithContext_InvalidValue_ReturnsFailure()
    {
        var schema = Z.Int<LimitContext>()
            .Refine((val, ctx) => val <= ctx.MaxValue, "Exceeds limit")
            .Nullable();

        var context = new ValidationContext<LimitContext>(
            new LimitContext(100),
            ValidationExecutionContext.Empty);

        var result = await schema.ValidateAsync(150, context);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Message == "Exceeds limit");
    }

    // ==================== Nullable in Object Fields ====================

    record UserProfile(string Name, int? Age, string? Bio);

    [Fact]
    public async Task NullableFieldInObject_NullField_ReturnsSuccess()
    {
        var schema = Z.Object<UserProfile>()
            .Field(u => u.Name, Z.String().MinLength(1))
            .Field(u => u.Age, Z.Int().Min(0).Nullable())
            .Field(u => u.Bio, Z.String().MaxLength(500).Nullable());

        var profile = new UserProfile("John", null, null);
        var result = await schema.ValidateAsync(profile);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task NullableFieldInObject_ValidFields_ReturnsSuccess()
    {
        var schema = Z.Object<UserProfile>()
            .Field(u => u.Name, Z.String().MinLength(1))
            .Field(u => u.Age, Z.Int().Min(0).Nullable())
            .Field(u => u.Bio, Z.String().MaxLength(500).Nullable());

        var profile = new UserProfile("John", 30, "Hello world");
        var result = await schema.ValidateAsync(profile);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task NullableFieldInObject_InvalidField_ReturnsFailure()
    {
        var schema = Z.Object<UserProfile>()
            .Field(u => u.Name, Z.String().MinLength(1))
            .Field(u => u.Age, Z.Int().Min(0).Nullable())
            .Field(u => u.Bio, Z.String().MaxLength(10).Nullable());

        var profile = new UserProfile("John", -5, "This bio is way too long");
        var result = await schema.ValidateAsync(profile);

        Assert.False(result.IsSuccess);
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains(result.Errors, e => e.Path == "age" && e.Code == "min_value");
        Assert.Contains(result.Errors, e => e.Path == "bio" && e.Code == "max_length");
    }
}
