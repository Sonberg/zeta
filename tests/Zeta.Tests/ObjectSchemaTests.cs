using Zeta.Schemas;

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
        var schema = Z.Object<User, BanContext>()
            .Field(u => u.Name, Z.String<BanContext>()
                .Refine((val, ctx) => val != ctx.BannedName, "Name is banned"));

        var context = new ValidationContext<BanContext>(
            new BanContext("Voldemort"), 
            ValidationExecutionContext.Empty);

        var result = await schema.ValidateAsync(new User("Voldemort", 99, null!), context);
        
        Assert.False(result.IsSuccess);
        Assert.Equal("Name is banned", result.Errors.Single().Message);
    }
}
