namespace Zeta.Tests;

public class OptionalSchemaTests
{
    // ==================== Optional is Alias for Nullable ====================

    [Fact]
    public async Task OptionalString_NullValue_ReturnsSuccess()
    {
        var schema = Z.String().MinLength(3).Optional();
        var result = await schema.ValidateAsync(null);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task OptionalString_ValidValue_ReturnsSuccess()
    {
        var schema = Z.String().MinLength(3).Optional();
        var result = await schema.ValidateAsync("abc");

        Assert.True(result.IsSuccess);
        Assert.Equal("abc", result.Value);
    }

    [Fact]
    public async Task OptionalString_InvalidValue_ReturnsFailure()
    {
        var schema = Z.String().MinLength(3).Optional();
        var result = await schema.ValidateAsync("ab");

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "min_length");
    }

    [Fact]
    public async Task OptionalInt_NullValue_ReturnsSuccess()
    {
        var schema = Z.Int().Min(0).Optional();
        var result = await schema.ValidateAsync(null);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task OptionalInt_ValidValue_ReturnsSuccess()
    {
        var schema = Z.Int().Min(0).Optional();
        var result = await schema.ValidateAsync(42);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public async Task OptionalDouble_NullValue_ReturnsSuccess()
    {
        var schema = Z.Double().Positive().Optional();
        var result = await schema.ValidateAsync(null);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task OptionalDecimal_NullValue_ReturnsSuccess()
    {
        var schema = Z.Decimal().Min(0m).Optional();
        var result = await schema.ValidateAsync(null);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task OptionalGuid_NullValue_ReturnsSuccess()
    {
        var schema = Z.Guid().Optional();
        var result = await schema.ValidateAsync(null);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task OptionalBool_NullValue_ReturnsSuccess()
    {
        var schema = Z.Bool().Optional();
        var result = await schema.ValidateAsync(null);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task OptionalDateTime_NullValue_ReturnsSuccess()
    {
        var schema = Z.DateTime().Optional();
        var result = await schema.ValidateAsync(null);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task OptionalDateOnly_NullValue_ReturnsSuccess()
    {
        var schema = Z.DateOnly().Optional();
        var result = await schema.ValidateAsync(null);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task OptionalTimeOnly_NullValue_ReturnsSuccess()
    {
        var schema = Z.TimeOnly().Optional();
        var result = await schema.ValidateAsync(null);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task OptionalArray_NullValue_ReturnsSuccess()
    {
        var schema = Z.Collection<int>().Optional();
        var result = await schema.ValidateAsync(null);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task OptionalList_NullValue_ReturnsSuccess()
    {
        var schema = Z.Collection<string>().Optional();
        var result = await schema.ValidateAsync(null);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    record Person(string Name, int Age);

    [Fact]
    public async Task OptionalObject_NullValue_ReturnsSuccess()
    {
        var schema = Z.Object<Person>()
            .Field(p => p.Name, Z.String().MinLength(1))
            .Field(p => p.Age, Z.Int().Min(0))
            .Optional();

        var result = await schema.ValidateAsync(null);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task OptionalObject_ValidValue_ReturnsSuccess()
    {
        var schema = Z.Object<Person>()
            .Field(p => p.Name, Z.String().MinLength(1))
            .Field(p => p.Age, Z.Int().Min(0))
            .Optional();

        var person = new Person("John", 30);
        var result = await schema.ValidateAsync(person);

        Assert.True(result.IsSuccess);
        Assert.Equal(person, result.Value);
    }

    // ==================== Optional in Object Fields (PATCH semantics) ====================

    record PatchRequest(string? Name, int? Age);

    [Fact]
    public async Task OptionalFieldsInObject_AllNull_ReturnsSuccess()
    {
        // Use case: PATCH request where all fields are optional
        var schema = Z.Object<PatchRequest>()
            .Field(r => r.Name, Z.String().MinLength(1).Optional())
            .Field(r => r.Age, Z.Int().Min(0).Optional());

        var request = new PatchRequest(null, null);
        var result = await schema.ValidateAsync(request);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task OptionalFieldsInObject_PartialUpdate_ReturnsSuccess()
    {
        var schema = Z.Object<PatchRequest>()
            .Field(r => r.Name, Z.String().MinLength(1).Optional())
            .Field(r => r.Age, Z.Int().Min(0).Optional());

        var request = new PatchRequest("John", null);
        var result = await schema.ValidateAsync(request);

        Assert.True(result.IsSuccess);
    }
}
