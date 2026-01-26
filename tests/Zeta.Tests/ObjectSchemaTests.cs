using Zeta.Core;

namespace Zeta.Tests;

public class ObjectSchemaTests
{
    record Address(string City, string Zip);
    record User(string Name, int Age, Address Address);

    [Fact]
    public async Task Field_PropagatesValidation()
    {
        var schema = Z.Object<User>()
            .Field(u => u.Name, Z.String().MinLength(3));

        var valid = await schema.ValidateAsync(new User("Joe", 20, new Address("City", "12345")));
        Assert.True(valid.IsSuccess);

        var invalid = await schema.ValidateAsync(new User("Jo", 20, new Address("City", "12345")));
        Assert.False(invalid.IsSuccess);
        Assert.Contains(invalid.Errors, e => e.Path == "name" && e.Code == "min_length");
    }

    [Fact]
    public async Task NestedField_PropagatesPath()
    {
        var addressSchema = Z.Object<Address>()
            .Field(a => a.Zip, Z.String().MinLength(5));

        var userSchema = Z.Object<User>()
            .Field(u => u.Address, addressSchema);

        var user = new User("Joe", 20, new Address("City", "123")); // Invalid Zip

        var result = await userSchema.ValidateAsync(user);

        Assert.False(result.IsSuccess);
        var error = result.Errors.Single();
        Assert.Equal("address.zip", error.Path);
    }

    public record BanContext(string BannedName);

    [Fact]
    public async Task Context_PropagatesToFields()
    {
        var schema = Z.Object<User>()
            .Field(u => u.Name, Z.String())
            .WithContext<User, BanContext>()
            .Refine((user, ctx) => user.Name != ctx.BannedName, "Name is banned");

        var context = new BanContext("Voldemort");

        var result = await schema.ValidateAsync(new User("Voldemort", 99, null!), context);

        Assert.False(result.IsSuccess);
        Assert.Equal("Name is banned", result.Errors.Single().Message);
    }

    record Order(bool IsCompany, string? OrgNumber, string? Ssn);

    [Fact]
    public async Task When_ThenBranch_ValidatesWhenConditionTrue()
    {
        var schema = Z.Object<Order>()
            .When(
                o => o.IsCompany,
                then => then.Require(o => o.OrgNumber));

        // When IsCompany is true and OrgNumber is null, should fail
        var result = await schema.ValidateAsync(new Order(true, null, "123456789"));
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Path == "orgNumber" && e.Code == "required");
    }

    [Fact]
    public async Task When_ThenBranch_SkipsWhenConditionFalse()
    {
        var schema = Z.Object<Order>()
            .When(
                o => o.IsCompany,
                then => then.Require(o => o.OrgNumber));

        // When IsCompany is false, OrgNumber validation should be skipped
        var result = await schema.ValidateAsync(new Order(false, null, "123456789"));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task When_ElseBranch_ValidatesWhenConditionFalse()
    {
        var schema = Z.Object<Order>()
            .When(
                o => o.IsCompany,
                then => then.Require(o => o.OrgNumber),
                @else => @else.Require(o => o.Ssn));

        // When IsCompany is false and Ssn is null, should fail
        var result = await schema.ValidateAsync(new Order(false, null, null));
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Path == "ssn" && e.Code == "required");
    }

    [Fact]
    public async Task When_ThenBranch_PassesWhenValueProvided()
    {
        var schema = Z.Object<Order>()
            .When(
                o => o.IsCompany,
                then => then.Require(o => o.OrgNumber),
                @else => @else.Require(o => o.Ssn));

        // When IsCompany is true and OrgNumber is provided, should pass
        var result = await schema.ValidateAsync(new Order(true, "ORG123", null));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task When_ElseBranch_PassesWhenValueProvided()
    {
        var schema = Z.Object<Order>()
            .When(
                o => o.IsCompany,
                then => then.Require(o => o.OrgNumber),
                @else => @else.Require(o => o.Ssn));

        // When IsCompany is false and Ssn is provided, should pass
        var result = await schema.ValidateAsync(new Order(false, null, "123456789"));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task When_WithFieldSchema_ValidatesConditionally()
    {
        var schema = Z.Object<Order>()
            .When(
                o => o.IsCompany,
                then => then.Field(o => o.OrgNumber, Z.String().MinLength(5)));

        // When IsCompany is true and OrgNumber is too short, should fail
        var result = await schema.ValidateAsync(new Order(true, "ORG", "123456789"));
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Path == "orgNumber" && e.Code == "min_length");
    }

    record Registration(bool IsCompany, string? OrgNumber, string? VatNumber, string? Ssn);

    [Fact]
    public async Task When_MultipleRequirements_ValidatesAll()
    {
        var schema = Z.Object<Registration>()
            .When(
                r => r.IsCompany,
                then => then
                    .Require(r => r.OrgNumber)
                    .Require(r => r.VatNumber),
                @else => @else.Require(r => r.Ssn));

        // When IsCompany is true and both are null, should have 2 errors
        var result = await schema.ValidateAsync(new Registration(true, null, null, null));
        Assert.False(result.IsSuccess);
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains(result.Errors, e => e.Path == "orgNumber");
        Assert.Contains(result.Errors, e => e.Path == "vatNumber");
    }
}
