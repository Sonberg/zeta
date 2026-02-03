using Zeta.Core;

namespace Zeta.Tests;

public class IfConditionalTests
{
    // ==================== Value Schema If Tests ====================

    [Fact]
    public async Task If_ValueSchema_ConditionTrue_AppliesRules()
    {
        var schema = Z.Int()
            .If(v => v >= 18, s => s.Max(100));

        var result = await schema.ValidateAsync(150);
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "max_value");
    }

    [Fact]
    public async Task If_ValueSchema_ConditionFalse_SkipsRules()
    {
        var schema = Z.Int()
            .If(v => v >= 18, s => s.Max(100));

        // v < 18, so Max(100) is not applied; 150 is allowed
        var result = await schema.ValidateAsync(10);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task If_ValueSchema_ChainedConditions_AppliesMatching()
    {
        var schema = Z.Int()
            .If(v => v >= 18, s => s.Max(100))
            .If(v => v < 18, s => s.Min(0).Max(17));

        // Adult: 150 should fail Max(100)
        var adultResult = await schema.ValidateAsync(150);
        Assert.False(adultResult.IsSuccess);
        Assert.Contains(adultResult.Errors, e => e.Code == "max_value");

        // Minor: -5 should fail Min(0)
        var minorResult = await schema.ValidateAsync(-5);
        Assert.False(minorResult.IsSuccess);
        Assert.Contains(minorResult.Errors, e => e.Code == "min_value");

        // Minor: 10 should pass both
        var validMinor = await schema.ValidateAsync(10);
        Assert.True(validMinor.IsSuccess);

        // Adult: 50 should pass both
        var validAdult = await schema.ValidateAsync(50);
        Assert.True(validAdult.IsSuccess);
    }

    [Fact]
    public async Task If_ValueSchema_MultipleRulesInBranch()
    {
        var schema = Z.Int()
            .If(v => v >= 0, s => s.Min(10).Max(100));

        // Positive but below min: fails Min(10)
        var result = await schema.ValidateAsync(5);
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "min_value");

        // Negative: condition false, no rules applied
        var negResult = await schema.ValidateAsync(-1);
        Assert.True(negResult.IsSuccess);
    }

    [Fact]
    public async Task If_StringSchema_ConditionTrue_AppliesRules()
    {
        var schema = Z.String()
            .If(s => s.Length > 0, s => s.MinLength(3));

        var result = await schema.ValidateAsync("ab");
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "min_length");
    }

    [Fact]
    public async Task If_StringSchema_ConditionFalse_SkipsRules()
    {
        var schema = Z.String()
            .If(s => s.Length > 5, s => s.MinLength(10));

        // Length is 3, condition (>5) false, MinLength(10) skipped
        var result = await schema.ValidateAsync("abc");
        Assert.True(result.IsSuccess);
    }

    // ==================== Context-Aware Value Schema If Tests ====================

    public record UserContext(bool IsSpecialUser);

    [Fact]
    public async Task If_ContextAware_AccessesContext()
    {
        var schema = Z.String()
            .WithContext<UserContext>()
            .If(
                (value, ctx) => ctx.IsSpecialUser,
                s => s.MinLength(10));

        var specialContext = new ValidationContext<UserContext>(new UserContext(IsSpecialUser: true));
        var normalContext = new ValidationContext<UserContext>(new UserContext(IsSpecialUser: false));

        // Special user: "short" fails MinLength(10)
        var specialResult = await schema.ValidateAsync("short", specialContext);
        Assert.False(specialResult.IsSuccess);

        // Normal user: "short" passes (condition false)
        var normalResult = await schema.ValidateAsync("short", normalContext);
        Assert.True(normalResult.IsSuccess);
    }

    [Fact]
    public async Task If_ContextAware_ChainedConditions()
    {
        var schema = Z.String()
            .WithContext<UserContext>()
            .If(
                (value, ctx) => ctx.IsSpecialUser,
                s => s.MinLength(10))
            .If(
                (value, ctx) => !ctx.IsSpecialUser,
                s => s.MinLength(5));

        var specialContext = new ValidationContext<UserContext>(new UserContext(IsSpecialUser: true));
        var normalContext = new ValidationContext<UserContext>(new UserContext(IsSpecialUser: false));

        // Special user, 7 chars: fails MinLength(10), passes MinLength(5) not applied
        var result1 = await schema.ValidateAsync("abcdefg", specialContext);
        Assert.False(result1.IsSuccess);

        // Normal user, 3 chars: passes MinLength(10) not applied, fails MinLength(5)
        var result2 = await schema.ValidateAsync("abc", normalContext);
        Assert.False(result2.IsSuccess);

        // Normal user, 7 chars: passes MinLength(5)
        var result3 = await schema.ValidateAsync("abcdefg", normalContext);
        Assert.True(result3.IsSuccess);
    }

    // ==================== Object Schema If Tests ====================

    record Order(bool IsCompany, string? OrgNumber, string? Ssn);

    [Fact]
    public async Task If_ObjectSchema_RequiresFieldWhenConditionTrue()
    {
        var schema = Z.Object<Order>()
            .If(
                o => o.IsCompany,
                then => then.Require(o => o.OrgNumber));

        // IsCompany true, OrgNumber null -> fail
        var result = await schema.ValidateAsync(new Order(true, null, "123456789"));
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Path == "orgNumber" && e.Code == "required");
    }

    [Fact]
    public async Task If_ObjectSchema_SkipsFieldWhenConditionFalse()
    {
        var schema = Z.Object<Order>()
            .If(
                o => o.IsCompany,
                then => then.Require(o => o.OrgNumber));

        // IsCompany false -> OrgNumber validation skipped
        var result = await schema.ValidateAsync(new Order(false, null, "123456789"));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task If_ObjectSchema_ChainedIfReplacesElse()
    {
        var schema = Z.Object<Order>()
            .If(o => o.IsCompany, then => then.Require(o => o.OrgNumber))
            .If(o => !o.IsCompany, then => then.Require(o => o.Ssn));

        // IsCompany true, OrgNumber null -> fail on OrgNumber
        var result1 = await schema.ValidateAsync(new Order(true, null, "123"));
        Assert.False(result1.IsSuccess);
        Assert.Contains(result1.Errors, e => e.Path == "orgNumber");

        // IsCompany false, Ssn null -> fail on Ssn
        var result2 = await schema.ValidateAsync(new Order(false, "ORG123", null));
        Assert.False(result2.IsSuccess);
        Assert.Contains(result2.Errors, e => e.Path == "ssn");

        // IsCompany true, OrgNumber provided -> pass
        var result3 = await schema.ValidateAsync(new Order(true, "ORG123", null));
        Assert.True(result3.IsSuccess);

        // IsCompany false, Ssn provided -> pass
        var result4 = await schema.ValidateAsync(new Order(false, null, "123456789"));
        Assert.True(result4.IsSuccess);
    }

    [Fact]
    public async Task If_ObjectSchema_WithFieldSchema()
    {
        var schema = Z.Object<Order>()
            .If(
                o => o.IsCompany,
                then => then.Field(o => o.OrgNumber, Z.String().MinLength(5)));

        // IsCompany true, OrgNumber too short -> fail
        var result = await schema.ValidateAsync(new Order(true, "ORG", "123"));
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Path == "orgNumber" && e.Code == "min_length");
    }

    record Registration(bool IsCompany, string? OrgNumber, string? VatNumber, string? Ssn);

    [Fact]
    public async Task If_ObjectSchema_MultipleRequirements()
    {
        var schema = Z.Object<Registration>()
            .If(
                r => r.IsCompany,
                then => then
                    .Require(r => r.OrgNumber)
                    .Require(r => r.VatNumber));

        // IsCompany true, both null -> 2 errors
        var result = await schema.ValidateAsync(new Registration(true, null, null, null));
        Assert.False(result.IsSuccess);
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains(result.Errors, e => e.Path == "orgNumber");
        Assert.Contains(result.Errors, e => e.Path == "vatNumber");
    }

    // ==================== Context-Aware Object Schema If Tests ====================

    public record LoanContext(bool RequireEmployment);
    public record LoanApplication(int Income, bool IsEmployed, string? EmployerName);

    [Fact]
    public async Task If_ObjectSchema_ContextAware_UsesContext()
    {
        var schema = Z.Object<LoanApplication>()
            .WithContext<LoanContext>()
            .If(
                (loan, ctx) => ctx.RequireEmployment,
                then => then.Require(l => l.EmployerName));

        var strictContext = new ValidationContext<LoanContext>(new LoanContext(RequireEmployment: true));
        var relaxedContext = new ValidationContext<LoanContext>(new LoanContext(RequireEmployment: false));

        // RequireEmployment true, no employer -> fail
        var result1 = await schema.ValidateAsync(new LoanApplication(50000, true, null), strictContext);
        Assert.False(result1.IsSuccess);
        Assert.Contains(result1.Errors, e => e.Path == "employerName" && e.Code == "required");

        // RequireEmployment false -> pass
        var result2 = await schema.ValidateAsync(new LoanApplication(50000, true, null), relaxedContext);
        Assert.True(result2.IsSuccess);
    }

    // ==================== Collection Schema If Tests ====================

    [Fact]
    public async Task If_CollectionSchema_AppliesConditionally()
    {
        var schema = Z.Collection<string>()
            .If(
                items => items.Count > 0,
                s => s.MinLength(2));

        // Non-empty collection with only 1 item -> MinLength(2) fails
        var result = await schema.ValidateAsync(new[] { "one" });
        Assert.False(result.IsSuccess);

        // Empty collection -> condition false, MinLength(2) skipped
        var emptyResult = await schema.ValidateAsync(Array.Empty<string>());
        Assert.True(emptyResult.IsSuccess);
    }

    // ==================== Select() in If Tests ====================

    record UserProfile(string Password, int? Age, double? Score, decimal? Balance, bool RequiresPassword);

    [Fact]
    public async Task If_ObjectSchema_Select_StringProperty()
    {
        var schema = Z.Object<UserProfile>()
            .If(
                u => u.RequiresPassword,
                then => then.Select(u => u.Password, s => s.MinLength(8).MaxLength(100)));

        var result = await schema.ValidateAsync(new UserProfile("short", null, null, null, true));
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Path == "password" && e.Code == "min_length");
    }

    [Fact]
    public async Task If_ObjectSchema_Select_SkipsWhenConditionFalse()
    {
        var schema = Z.Object<UserProfile>()
            .If(
                u => u.RequiresPassword,
                then => then.Select(u => u.Password, s => s.MinLength(8)));

        // RequiresPassword false -> validation skipped
        var result = await schema.ValidateAsync(new UserProfile("short", null, null, null, false));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task If_ObjectSchema_Select_MultipleFields()
    {
        var schema = Z.Object<UserProfile>()
            .If(
                u => u.RequiresPassword,
                then => then
                    .Select(u => u.Password, s => s.MinLength(8))
                    .Select(u => u.Age, s => s.Min(18))
                    .Select(u => u.Score, s => s.Min(0.0).Max(100.0)));

        var result = await schema.ValidateAsync(new UserProfile("short", 15, 150.0, null, true));
        Assert.False(result.IsSuccess);
        Assert.Equal(3, result.Errors.Count);
        Assert.Contains(result.Errors, e => e.Path == "password" && e.Code == "min_length");
        Assert.Contains(result.Errors, e => e.Path == "age" && e.Code == "min_value");
        Assert.Contains(result.Errors, e => e.Path == "score" && e.Code == "max_value");
    }
}
