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

    // ==================== Int Schema AllowNull (non-null values) ====================

    [Fact]
    public async Task AllowNullInt_ValidValue_ReturnsSuccess()
    {
        var schema = Z.Int().Min(0).Nullable();
        var result = await schema.ValidateAsync(42);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public async Task AllowNullInt_InvalidValue_ReturnsFailure()
    {
        var schema = Z.Int().Min(0).Nullable();
        var result = await schema.ValidateAsync(-1);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "min_value");
    }

    // ==================== Double Schema AllowNull (non-null values) ====================

    [Fact]
    public async Task AllowNullDouble_ValidValue_ReturnsSuccess()
    {
        var schema = Z.Double().Positive().Nullable();
        var result = await schema.ValidateAsync(3.14);

        Assert.True(result.IsSuccess);
        Assert.Equal(3.14, result.Value);
    }

    [Fact]
    public async Task AllowNullDouble_InvalidValue_ReturnsFailure()
    {
        var schema = Z.Double().Positive().Nullable();
        var result = await schema.ValidateAsync(-1.0);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "positive");
    }

    // ==================== Decimal Schema AllowNull (non-null values) ====================

    [Fact]
    public async Task AllowNullDecimal_ValidValue_ReturnsSuccess()
    {
        var schema = Z.Decimal().Min(0m).Nullable();
        var result = await schema.ValidateAsync(99.99m);

        Assert.True(result.IsSuccess);
        Assert.Equal(99.99m, result.Value);
    }

    [Fact]
    public async Task AllowNullDecimal_InvalidValue_ReturnsFailure()
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
        var schema = Z.Collection<int>().Each(n => n.Min(0)).Nullable();
        var result = await schema.ValidateAsync(null);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task NullableArray_ValidValue_ReturnsSuccess()
    {
        var schema = Z.Collection<int>().Each(n => n.Min(0)).Nullable();
        var result = await schema.ValidateAsync([1, 2, 3]);

        Assert.True(result.IsSuccess);
        Assert.Equal([1, 2, 3], result.Value);
    }

    [Fact]
    public async Task NullableArray_InvalidValue_ReturnsFailure()
    {
        var schema = Z.Collection<int>().Each(n => n.Min(0)).Nullable();
        var result = await schema.ValidateAsync([1, -1, 3]);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Path == "[1]");
    }

    // ==================== List Schema Nullable ====================

    [Fact]
    public async Task NullableList_NullValue_ReturnsSuccess()
    {
        var schema = Z.Collection<string>().Each(s => s.Email()).Nullable();
        var result = await schema.ValidateAsync(null);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task NullableList_ValidValue_ReturnsSuccess()
    {
        var schema = Z.Collection<string>().Each(s => s.Email()).Nullable();
        var result = await schema.ValidateAsync(["a@b.com", "c@d.com"]);

        Assert.True(result.IsSuccess);
        Assert.Equal(["a@b.com", "c@d.com"], result.Value);
    }

    [Fact]
    public async Task NullableList_InvalidValue_ReturnsFailure()
    {
        var schema = Z.Collection<string>().Each(s => s.Email()).Nullable();
        var result = await schema.ValidateAsync(["a@b.com", "invalid"]);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Path == "[1]" && e.Code == "email");
    }

    // ==================== Context-Aware Nullable ====================

    record LimitContext(int MaxValue);

    [Fact]
    public async Task NullableWithContext_StringNullValue_ReturnsSuccess()
    {
        var schema = Z.String()
            .WithContext<LimitContext>()
            .Refine((val, _) => val.Length <= 10, "Too long")
            .Nullable();

        var context = new ValidationContext<LimitContext>(new LimitContext(100));

        var result = await schema.ValidateAsync(null, context);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task NullableWithContext_ValidValue_ReturnsSuccess()
    {
        var schema = Z.Int()
            .WithContext<LimitContext>()
            .Refine((val, ctx) => val <= ctx.MaxValue, "Exceeds limit")
            .Nullable();

        var context = new ValidationContext<LimitContext>(
            new LimitContext(100));

        var result = await schema.ValidateAsync(50, context);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task NullableWithContext_InvalidValue_ReturnsFailure()
    {
        var schema = Z.Int()
            .WithContext<LimitContext>()
            .Refine((val, ctx) => val <= ctx.MaxValue, "Exceeds limit")
            .Nullable();

        var context = new ValidationContext<LimitContext>(
            new LimitContext(100));

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
            .Field(u => u.Age, Z.Int().Min(0))
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
            .Field(u => u.Age, Z.Int().Min(0))
            .Field(u => u.Bio, Z.String().MaxLength(10).Nullable());

        var profile = new UserProfile("John", -5, "This bio is way too long");
        var result = await schema.ValidateAsync(profile);

        Assert.False(result.IsSuccess);
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains(result.Errors, e => e.Path == "age" && e.Code == "min_value");
        Assert.Contains(result.Errors, e => e.Path == "bio" && e.Code == "max_length");
    }
}
