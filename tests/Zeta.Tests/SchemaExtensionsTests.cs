namespace Zeta.Tests;

public class SchemaExtensionsTests
{
    // ==================== String Optional Tests ====================

    [Fact]
    public async Task Optional_String_AcceptsUndefinedValue()
    {
        var schema = Z.String().MinLength(3).Optional();
        var result = await schema.ValidateAsync(default(string));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Optional_String_ValidatesWhenPresent()
    {
        var schema = Z.String().MinLength(3).Optional();
        var result = await schema.ValidateAsync("ab");

        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == "min_length");
    }

    [Fact]
    public async Task Optional_String_PassesValidationWhenValueIsValid()
    {
        var schema = Z.String().MinLength(3).Optional();
        var result = await schema.ValidateAsync("hello");

        Assert.True(result.IsSuccess);
    }

    // ==================== Int Optional Tests ====================

    [Fact]
    public async Task Optional_Int_AcceptsUndefinedValue()
    {
        var schema = Z.Int().Min(10).Optional();
        var result = await schema.ValidateAsync(default(int?));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Optional_Int_ValidatesWhenPresent()
    {
        var schema = Z.Int().Min(10).Optional();
        var result = await schema.ValidateAsync(5);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == "min_value");
    }

    [Fact]
    public async Task Optional_Int_PassesValidationWhenValueIsValid()
    {
        var schema = Z.Int().Min(10).Optional();
        var result = await schema.ValidateAsync(15);

        Assert.True(result.IsSuccess);
    }

    // ==================== Double Optional Tests ====================

    [Fact]
    public async Task Optional_Double_AcceptsUndefinedValue()
    {
        var schema = Z.Double().Min(10.0).Optional();
        var result = await schema.ValidateAsync(default(double?));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Optional_Double_ValidatesWhenPresent()
    {
        var schema = Z.Double().Min(10.0).Optional();
        var result = await schema.ValidateAsync(5.0);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == "min_value");
    }

    // ==================== Decimal Optional Tests ====================

    [Fact]
    public async Task Optional_Decimal_AcceptsUndefinedValue()
    {
        var schema = Z.Decimal().Min(10m).Optional();
        var result = await schema.ValidateAsync(default(decimal?));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Optional_Decimal_ValidatesWhenPresent()
    {
        var schema = Z.Decimal().Min(10m).Optional();
        var result = await schema.ValidateAsync(5m);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == "min_value");
    }

    // ==================== Object Optional Tests ====================

    [Fact]
    public async Task Optional_Object_AcceptsUndefinedValue()
    {
        var schema = Z.Object<TestUser>()
            .Field(u => u.Name, Z.String().MinLength(3))
            .Optional();
        
        var result = await schema.ValidateAsync(default(TestUser));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Optional_Object_ValidatesWhenPresent()
    {
        var schema = Z.Object<TestUser>()
            .Field(u => u.Name, Z.String().MinLength(3))
            .Optional();
        
        var result = await schema.ValidateAsync(new TestUser { Name = "ab" });

        Assert.True(result.IsFailure);
    }

    // ==================== Collection Optional Tests ====================

    [Fact]
    public async Task Optional_Collection_AcceptsUndefinedValue()
    {
        var schema = Z.Collection(Z.Int()).MinLength(1).Optional();
        var result = await schema.ValidateAsync(default(List<int>));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Optional_Collection_ValidatesWhenPresent()
    {
        var schema = Z.Collection(Z.Int()).MinLength(1).Optional();
        var result = await schema.ValidateAsync(new List<int>());

        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == "min_length");
    }

    // ==================== DateTime Optional Tests ====================

    [Fact]
    public async Task Optional_DateTime_AcceptsUndefinedValue()
    {
        var schema = Z.DateTime().Optional();
        var result = await schema.ValidateAsync(default(DateTime?));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Optional_DateTime_ValidatesWhenPresent()
    {
        var minDate = new DateTime(2024, 1, 1);
        var schema = Z.DateTime().Min(minDate).Optional();
        var result = await schema.ValidateAsync(new DateTime(2023, 1, 1));

        Assert.True(result.IsFailure);
    }

    // ==================== DateOnly Optional Tests ====================

    [Fact]
    public async Task Optional_DateOnly_AcceptsUndefinedValue()
    {
        var schema = Z.DateOnly().Optional();
        var result = await schema.ValidateAsync(default(DateOnly?));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Optional_DateOnly_ValidatesWhenPresent()
    {
        var minDate = new DateOnly(2024, 1, 1);
        var schema = Z.DateOnly().Min(minDate).Optional();
        var result = await schema.ValidateAsync(new DateOnly(2023, 1, 1));

        Assert.True(result.IsFailure);
    }

    // ==================== TimeOnly Optional Tests ====================

    [Fact]
    public async Task Optional_TimeOnly_AcceptsUndefinedValue()
    {
        var schema = Z.TimeOnly().Optional();
        var result = await schema.ValidateAsync(default(TimeOnly?));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Optional_TimeOnly_ValidatesWhenPresent()
    {
        var minTime = new TimeOnly(12, 0);
        var schema = Z.TimeOnly().Min(minTime).Optional();
        var result = await schema.ValidateAsync(new TimeOnly(10, 0));

        Assert.True(result.IsFailure);
    }

    // ==================== Guid Optional Tests ====================

    [Fact]
    public async Task Optional_Guid_AcceptsUndefinedValue()
    {
        var schema = Z.Guid().Optional();
        var result = await schema.ValidateAsync(default(Guid?));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Optional_Guid_ValidatesWhenPresent()
    {
        var schema = Z.Guid().Optional();
        var validGuid = Guid.NewGuid();
        var result = await schema.ValidateAsync(validGuid);

        Assert.True(result.IsSuccess);
    }

    // ==================== Bool Optional Tests ====================

    [Fact]
    public async Task Optional_Bool_AcceptsUndefinedValue()
    {
        var schema = Z.Bool().Optional();
        var result = await schema.ValidateAsync(default(bool?));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Optional_Bool_ValidatesWhenPresent()
    {
        var schema = Z.Bool().Optional();
        var result = await schema.ValidateAsync(true);

        Assert.True(result.IsSuccess);
    }

    private class TestUser
    {
        public string Name { get; set; } = "";
    }
}
