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
            .Using<BanContext>()
            .Refine((user, ctx) => user.Name != ctx.BannedName, "Name is banned");

        var context = new BanContext("Voldemort");

        var result = await schema.ValidateAsync(new User("Voldemort", 99, null!), context);

        Assert.False(result.IsSuccess);
        Assert.Equal("Name is banned", result.Errors.Single().Message);
    }
    
    // ==================== Implicit Promotion Tests ====================

    public record EmailContext(string BannedDomain);

    [Fact]
    public async Task Field_WithContextAwareSchema_ImplicitlyPromotesObjectSchema()
    {
        // When you pass a context-aware schema to Field() on a contextless ObjectSchema,
        // it should implicitly promote the ObjectSchema to context-aware
        var contextAwareEmailSchema = Z.String()
            .Email()
            .Using<EmailContext>()
            .Refine((email, ctx) => !email.EndsWith($"@{ctx.BannedDomain}"), "Email domain is banned");

        var schema = Z.Object<User>()
            .Field(u => u.Name, contextAwareEmailSchema);

        var context = new EmailContext("banned.com");

        // Valid case - email doesn't end with banned domain
        var validResult = await schema.ValidateAsync(new User("user@allowed.com", 20, null!), context);
        Assert.True(validResult.IsSuccess);

        // Invalid case - email ends with banned domain
        var invalidResult = await schema.ValidateAsync(new User("user@banned.com", 20, null!), context);
        Assert.False(invalidResult.IsSuccess);
        Assert.Contains(invalidResult.Errors, e => e.Path == "name" && e.Message == "Email domain is banned");
    }

    [Fact]
    public async Task Field_WithContextAwareSchema_AllowsFurtherFieldsAndRefinements()
    {
        var contextAwareEmailSchema = Z.String()
            .Email()
            .Using<EmailContext>()
            .Refine((email, ctx) => !email.EndsWith($"@{ctx.BannedDomain}"), "Email domain is banned");

        // After implicit promotion, we should be able to chain more fields and refinements
        var schema = Z.Object<User>()
            .Field(u => u.Name, contextAwareEmailSchema)
            .Field(u => u.Age, Z.Int().Min(18))
            .Refine((user, ctx) => user.Age >= 21 || !user.Name.EndsWith($"@{ctx.BannedDomain}"),
                "Must be 21+ for this domain");

        var context = new EmailContext("restricted.com");

        var result = await schema.ValidateAsync(new User("test@other.com", 18, null!), context);
        Assert.True(result.IsSuccess);
    }
    
    // ==================== Single-Parameter Using Tests ====================

    [Fact]
    public async Task Using_SingleParameter_AllowsContextInference()
    {
        // The new .Using<TContext>() syntax infers T from the ObjectSchema<T>
        var schema = Z.Object<User>()
            .Using<BanContext>()
            .Field(u => u.Name, Z.String().MinLength(2))
            .Refine((user, ctx) => user.Name != ctx.BannedName, "Name is banned");

        var context = new BanContext("BadName");

        var validResult = await schema.ValidateAsync(new User("GoodName", 25, null!), context);
        Assert.True(validResult.IsSuccess);

        var bannedResult = await schema.ValidateAsync(new User("BadName", 25, null!), context);
        Assert.False(bannedResult.IsSuccess);
        Assert.Equal("Name is banned", bannedResult.Errors.Single().Message);
    }

    [Fact]
    public async Task Using_SingleParameter_ChainsWithContextAwareFields()
    {
        var schema = Z.Object<User>()
            .Using<BanContext>()
            .Field(u => u.Name, Z.String().Using<BanContext>()
                .Refine((name, ctx) => name != ctx.BannedName, "Field name is banned"))
            .Field(u => u.Age, Z.Int().Min(0));

        var context = new BanContext("Forbidden");

        var result = await schema.ValidateAsync(new User("Forbidden", 25, null!), context);
        Assert.False(result.IsSuccess);
        Assert.Equal("Field name is banned", result.Errors.Single().Message);
    }

    // ==================== Select() Method Tests ====================

    // record UserProfile(string Password, int? Age, double? Score, decimal? Balance, bool RequiresPassword);
    //
    // [Fact]
    // public async Task Select_Contextless_StringProperty_ValidatesWithInlineBuilder()
    // {
    //     var schema = Z.Object<UserProfile>()
    //         .When(
    //             u => u.RequiresPassword,
    //             then => then.Select(u => u.Password, s => s.MinLength(8).MaxLength(100)));
    //
    //     // When RequiresPassword is true and password is too short, should fail
    //     var result = await schema.ValidateAsync(new UserProfile("short", null, null, null, true));
    //     Assert.False(result.IsSuccess);
    //     Assert.Contains(result.Errors, e => e.Path == "password" && e.Code == "min_length");
    // }
    //
    // [Fact]
    // public async Task Select_Contextless_StringProperty_PassesValidation()
    // {
    //     var schema = Z.Object<UserProfile>()
    //         .When(
    //             u => u.RequiresPassword,
    //             then => then.Select(u => u.Password, s => s.MinLength(8).MaxLength(100)));
    //
    //     // Valid password
    //     var result = await schema.ValidateAsync(new UserProfile("validpassword123", null, null, null, true));
    //     Assert.True(result.IsSuccess);
    // }
    //
    // [Fact]
    // public async Task Select_Contextless_SkipsWhenConditionFalse()
    // {
    //     var schema = Z.Object<UserProfile>()
    //         .When(
    //             u => u.RequiresPassword,
    //             then => then.Select(u => u.Password, s => s.MinLength(8)));
    //
    //     // When RequiresPassword is false, password validation should be skipped
    //     var result = await schema.ValidateAsync(new UserProfile("short", null, null, null, false));
    //     Assert.True(result.IsSuccess);
    // }
    //
    // record ProductContext(bool EnforceMinPrice);
    // record Product(string Name, decimal? Price, int? Quantity, double? Weight);
    //
    // [Fact]
    // public async Task Select_ContextAware_DecimalProperty_ValidatesWithInlineBuilder()
    // {
    //     var schema = Z.Object<Product>()
    //         .Using<ProductContext>()
    //         .When(
    //             (_, ctx) => ctx.EnforceMinPrice,
    //             then => then.Select(p => p.Price, s => s.Min(10.00m)));
    //
    //     var context = new ProductContext(EnforceMinPrice: true);
    //
    //     // Price too low
    //     var result = await schema.ValidateAsync(new Product("Widget", 5.00m, null, null), context);
    //     Assert.False(result.IsSuccess);
    //     Assert.Contains(result.Errors, e => e.Path == "price" && e.Code == "min_value");
    // }
    //
    // [Fact]
    // public async Task Select_ContextAware_IntProperty_ValidatesWithInlineBuilder()
    // {
    //     var schema = Z.Object<Product>()
    //         .Using<ProductContext>()
    //         .When(
    //             p => p.Name != null,
    //             then => then.Select(p => p.Quantity, s => s.Min(1).Max(1000)));
    //
    //     var context = new ProductContext(EnforceMinPrice: false);
    //
    //     // Quantity too high
    //     var result = await schema.ValidateAsync(new Product("Widget", null, 5000, null), context);
    //     Assert.False(result.IsSuccess);
    //     Assert.Contains(result.Errors, e => e.Path == "quantity" && e.Code == "max_value");
    // }
    //
    // [Fact]
    // public async Task Select_ContextAware_DoubleProperty_ValidatesWithInlineBuilder()
    // {
    //     var schema = Z.Object<Product>()
    //         .Using<ProductContext>()
    //         .When(
    //             p => p.Name != null,
    //             then => then.Select(p => p.Weight, s => s.Min(0.1).Max(100.0)));
    //
    //     var context = new ProductContext(EnforceMinPrice: false);
    //
    //     // Weight negative
    //     var result = await schema.ValidateAsync(new Product("Widget", null, null, -5.0), context);
    //     Assert.False(result.IsSuccess);
    //     Assert.Contains(result.Errors, e => e.Path == "weight" && e.Code == "min_value");
    // }
    //
    // [Fact]
    // public async Task Select_MultipleFieldsChained_ValidatesAll()
    // {
    //     var schema = Z.Object<UserProfile>()
    //         .When(
    //             u => u.RequiresPassword,
    //             then => then
    //                 .Select(u => u.Password, s => s.MinLength(8))
    //                 .Select(u => u.Age, s => s.Min(18))
    //                 .Select(u => u.Score, s => s.Min(0.0).Max(100.0)));
    //
    //     // All fields invalid
    //     var result = await schema.ValidateAsync(new UserProfile("short", 15, 150.0, null, true));
    //     Assert.False(result.IsSuccess);
    //     Assert.Equal(3, result.Errors.Count);
    //     Assert.Contains(result.Errors, e => e.Path == "password" && e.Code == "min_length");
    //     Assert.Contains(result.Errors, e => e.Path == "age" && e.Code == "min_value");
    //     Assert.Contains(result.Errors, e => e.Path == "score" && e.Code == "max_value");
    // }
    //
    // [Fact]
    // public async Task Select_WithElseBranch_ExecutesCorrectBranch()
    // {
    //     var schema = Z.Object<UserProfile>()
    //         .When(
    //             u => u.RequiresPassword,
    //             then => then.Select(u => u.Password, s => s.MinLength(8)),
    //             @else => @else.Select(u => u.Password, s => s.MinLength(4)));
    //
    //     // Then branch: requires 8 chars
    //     var thenResult = await schema.ValidateAsync(new UserProfile("12345", null, null, null, true));
    //     Assert.False(thenResult.IsSuccess);
    //
    //     // Else branch: requires only 4 chars
    //     var elseResult = await schema.ValidateAsync(new UserProfile("12345", null, null, null, false));
    //     Assert.True(elseResult.IsSuccess);
    // }
}
