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
            .WithContext<BanContext>()
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

    // ==================== Implicit Promotion Tests ====================

    public record EmailContext(string BannedDomain);

    [Fact]
    public async Task Field_WithContextAwareSchema_ImplicitlyPromotesObjectSchema()
    {
        // When you pass a context-aware schema to Field() on a contextless ObjectSchema,
        // it should implicitly promote the ObjectSchema to context-aware
        var contextAwareEmailSchema = Z.String()
            .Email()
            .WithContext<EmailContext>()
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
            .WithContext<EmailContext>()
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

    // ==================== Context-Aware When() Tests ====================

    public record LoanContext(bool RequireEmployment, int MinIncomeThreshold);
    public record LoanApplication(int Income, bool IsEmployed, string? EmployerName);

    [Fact]
    public async Task When_ContextAwarePredicate_OnContextPromotedObjectSchema_UsesContextInCondition()
    {
        var schema = Z.Object<LoanApplication>()
            .WithContext<LoanContext>()
            .When(
                (loan, ctx) => ctx.RequireEmployment,
                then => then.Require(l => l.EmployerName, "Employer name is required"));

        // When RequireEmployment is true in context, EmployerName should be required
        var strictContext = new LoanContext(RequireEmployment: true, MinIncomeThreshold: 0);
        var resultMissingEmployer = await schema.ValidateAsync(
            new LoanApplication(50000, true, null), strictContext);
        Assert.False(resultMissingEmployer.IsSuccess);
        Assert.Contains(resultMissingEmployer.Errors, e => e.Path == "employerName" && e.Code == "required");

        // When RequireEmployment is false in context, EmployerName is optional
        var relaxedContext = new LoanContext(RequireEmployment: false, MinIncomeThreshold: 0);
        var resultNoEmployer = await schema.ValidateAsync(
            new LoanApplication(50000, true, null), relaxedContext);
        Assert.True(resultNoEmployer.IsSuccess);
    }

    [Fact]
    public async Task When_ContextAwarePredicate_WithElseBranch_ExecutesCorrectBranch()
    {
        var schema = Z.Object<LoanApplication>()
            .WithContext<LoanContext>()
            .When(
                (loan, ctx) => loan.Income >= ctx.MinIncomeThreshold,
                then => then.Field(l => l.EmployerName, Z.String().MinLength(2)),
                @else => @else.Require(l => l.IsEmployed, "Must be employed if income below threshold"));

        var context = new LoanContext(RequireEmployment: false, MinIncomeThreshold: 50000);

        // Income >= threshold: EmployerName must be at least 2 chars
        var highIncomeResult = await schema.ValidateAsync(
            new LoanApplication(60000, false, "X"), context);
        Assert.False(highIncomeResult.IsSuccess);
        Assert.Contains(highIncomeResult.Errors, e => e.Path == "employerName" && e.Code == "min_length");

        // Income < threshold: IsEmployed check (but IsEmployed is false is OK, it just can't be null)
        var lowIncomeResult = await schema.ValidateAsync(
            new LoanApplication(30000, false, null), context);
        // IsEmployed is a bool (non-nullable), so Require will always pass
        Assert.True(lowIncomeResult.IsSuccess);
    }

    [Fact]
    public async Task When_ContextAwarePredicate_CanAccessBothValueAndContext()
    {
        var schema = Z.Object<LoanApplication>()
            .WithContext<LoanContext>()
            .When(
                (loan, ctx) => loan.IsEmployed && ctx.RequireEmployment,
                then => then.Require(l => l.EmployerName, "Employed applicants must provide employer name"));

        var context = new LoanContext(RequireEmployment: true, MinIncomeThreshold: 0);

        // IsEmployed=true, RequireEmployment=true -> EmployerName required
        var result1 = await schema.ValidateAsync(new LoanApplication(50000, true, null), context);
        Assert.False(result1.IsSuccess);

        // IsEmployed=false, RequireEmployment=true -> condition false, EmployerName not required
        var result2 = await schema.ValidateAsync(new LoanApplication(50000, false, null), context);
        Assert.True(result2.IsSuccess);

        // IsEmployed=true, RequireEmployment=false -> condition false, EmployerName not required
        var relaxedContext = new LoanContext(RequireEmployment: false, MinIncomeThreshold: 0);
        var result3 = await schema.ValidateAsync(new LoanApplication(50000, true, null), relaxedContext);
        Assert.True(result3.IsSuccess);
    }

    [Fact]
    public async Task When_ContextAwarePredicate_OnObjectSchemaWithContext_Works()
    {
        // Test the same functionality directly on ObjectSchema<T, TContext>
        var schema = new Zeta.Schemas.ObjectSchema<LoanApplication, LoanContext>()
            .Field(l => l.Income, Z.Int().Min(0))
            .When(
                (loan, ctx) => ctx.RequireEmployment,
                then => then.Require(l => l.EmployerName));

        var context = new LoanContext(RequireEmployment: true, MinIncomeThreshold: 0);
        var validationContext = new ValidationContext<LoanContext>(context, ValidationExecutionContext.Empty);

        var result = await schema.ValidateAsync(new LoanApplication(50000, true, null), validationContext);
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Path == "employerName" && e.Code == "required");
    }

    // ==================== Single-Parameter WithContext Tests ====================

    [Fact]
    public async Task WithContext_SingleParameter_AllowsContextInference()
    {
        // The new .WithContext<TContext>() syntax infers T from the ObjectSchema<T>
        var schema = Z.Object<User>()
            .WithContext<BanContext>()
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
    public async Task WithContext_SingleParameter_ChainsWithContextAwareFields()
    {
        var schema = Z.Object<User>()
            .WithContext<BanContext>()
            .Field(u => u.Name, Z.String().WithContext<BanContext>()
                .Refine((name, ctx) => name != ctx.BannedName, "Field name is banned"))
            .Field(u => u.Age, Z.Int().Min(0));

        var context = new BanContext("Forbidden");

        var result = await schema.ValidateAsync(new User("Forbidden", 25, null!), context);
        Assert.False(result.IsSuccess);
        Assert.Equal("Field name is banned", result.Errors.Single().Message);
    }

    // ==================== Select() Method Tests ====================

    record UserProfile(string Password, int? Age, double? Score, decimal? Balance, bool RequiresPassword);

    [Fact]
    public async Task Select_Contextless_StringProperty_ValidatesWithInlineBuilder()
    {
        var schema = Z.Object<UserProfile>()
            .When(
                u => u.RequiresPassword,
                then => then.Select(u => u.Password, s => s.MinLength(8).MaxLength(100)));

        // When RequiresPassword is true and password is too short, should fail
        var result = await schema.ValidateAsync(new UserProfile("short", null, null, null, true));
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Path == "password" && e.Code == "min_length");
    }

    [Fact]
    public async Task Select_Contextless_StringProperty_PassesValidation()
    {
        var schema = Z.Object<UserProfile>()
            .When(
                u => u.RequiresPassword,
                then => then.Select(u => u.Password, s => s.MinLength(8).MaxLength(100)));

        // Valid password
        var result = await schema.ValidateAsync(new UserProfile("validpassword123", null, null, null, true));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Select_Contextless_SkipsWhenConditionFalse()
    {
        var schema = Z.Object<UserProfile>()
            .When(
                u => u.RequiresPassword,
                then => then.Select(u => u.Password, s => s.MinLength(8)));

        // When RequiresPassword is false, password validation should be skipped
        var result = await schema.ValidateAsync(new UserProfile("short", null, null, null, false));
        Assert.True(result.IsSuccess);
    }

    record ProductContext(bool EnforceMinPrice);
    record Product(string Name, decimal? Price, int? Quantity, double? Weight);

    [Fact]
    public async Task Select_ContextAware_DecimalProperty_ValidatesWithInlineBuilder()
    {
        var schema = Z.Object<Product>()
            .WithContext<ProductContext>()
            .When(
                (_, ctx) => ctx.EnforceMinPrice,
                then => then.Select(p => p.Price, s => s.Min(10.00m)));

        var context = new ProductContext(EnforceMinPrice: true);

        // Price too low
        var result = await schema.ValidateAsync(new Product("Widget", 5.00m, null, null), context);
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Path == "price" && e.Code == "min_value");
    }

    [Fact]
    public async Task Select_ContextAware_IntProperty_ValidatesWithInlineBuilder()
    {
        var schema = Z.Object<Product>()
            .WithContext<ProductContext>()
            .When(
                p => p.Name != null,
                then => then.Select(p => p.Quantity, s => s.Min(1).Max(1000)));

        var context = new ProductContext(EnforceMinPrice: false);

        // Quantity too high
        var result = await schema.ValidateAsync(new Product("Widget", null, 5000, null), context);
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Path == "quantity" && e.Code == "max_value");
    }

    [Fact]
    public async Task Select_ContextAware_DoubleProperty_ValidatesWithInlineBuilder()
    {
        var schema = Z.Object<Product>()
            .WithContext<ProductContext>()
            .When(
                p => p.Name != null,
                then => then.Select(p => p.Weight, s => s.Min(0.1).Max(100.0)));

        var context = new ProductContext(EnforceMinPrice: false);

        // Weight negative
        var result = await schema.ValidateAsync(new Product("Widget", null, null, -5.0), context);
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Path == "weight" && e.Code == "min_value");
    }

    [Fact]
    public async Task Select_MultipleFieldsChained_ValidatesAll()
    {
        var schema = Z.Object<UserProfile>()
            .When(
                u => u.RequiresPassword,
                then => then
                    .Select(u => u.Password, s => s.MinLength(8))
                    .Select(u => u.Age, s => s.Min(18))
                    .Select(u => u.Score, s => s.Min(0.0).Max(100.0)));

        // All fields invalid
        var result = await schema.ValidateAsync(new UserProfile("short", 15, 150.0, null, true));
        Assert.False(result.IsSuccess);
        Assert.Equal(3, result.Errors.Count);
        Assert.Contains(result.Errors, e => e.Path == "password" && e.Code == "min_length");
        Assert.Contains(result.Errors, e => e.Path == "age" && e.Code == "min_value");
        Assert.Contains(result.Errors, e => e.Path == "score" && e.Code == "max_value");
    }

    [Fact]
    public async Task Select_WithElseBranch_ExecutesCorrectBranch()
    {
        var schema = Z.Object<UserProfile>()
            .When(
                u => u.RequiresPassword,
                then => then.Select(u => u.Password, s => s.MinLength(8)),
                @else => @else.Select(u => u.Password, s => s.MinLength(4)));

        // Then branch: requires 8 chars
        var thenResult = await schema.ValidateAsync(new UserProfile("12345", null, null, null, true));
        Assert.False(thenResult.IsSuccess);

        // Else branch: requires only 4 chars
        var elseResult = await schema.ValidateAsync(new UserProfile("12345", null, null, null, false));
        Assert.True(elseResult.IsSuccess);
    }
}
