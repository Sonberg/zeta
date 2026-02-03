using Zeta.Core;

namespace Zeta.Tests;

public class SwitchConditionalTests
{
    record Animal(string Type, string Name, int BarkVolume, int ClawSharpness);

    [Fact]
    public async Task Switch_FirstMatchingCase_AppliesSchema()
    {
        var schema = Z.Object<Animal>()
            .Switch(s => s
                .Case(
                    a => a.Type == "Dog",
                    dog => dog.Field(a => a.BarkVolume, Z.Int().Min(0).Max(100)))
                .Case(
                    a => a.Type == "Cat",
                    cat => cat.Field(a => a.ClawSharpness, Z.Int().Min(0).Max(10))));

        // Dog with BarkVolume > 100 -> fails
        var dogResult = await schema.ValidateAsync(new Animal("Dog", "Rex", 150, 5));
        Assert.False(dogResult.IsSuccess);
        Assert.Contains(dogResult.Errors, e => e.Path == "barkVolume" && e.Code == "max_value");

        // Cat with ClawSharpness > 10 -> fails
        var catResult = await schema.ValidateAsync(new Animal("Cat", "Whiskers", 0, 15));
        Assert.False(catResult.IsSuccess);
        Assert.Contains(catResult.Errors, e => e.Path == "clawSharpness" && e.Code == "max_value");
    }

    [Fact]
    public async Task Switch_NoMatchingCase_AppliesDefault()
    {
        var schema = Z.Object<Animal>()
            .Switch(s => s
                .Case(
                    a => a.Type == "Dog",
                    dog => dog.Field(a => a.BarkVolume, Z.Int().Min(0).Max(100)))
                .Default(
                    other => other.Field(a => a.Name, Z.String().MinLength(1))));

        // Unknown type with empty name -> Default applies, MinLength(1) fails
        var result = await schema.ValidateAsync(new Animal("Fish", "", 0, 0));
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Path == "name" && e.Code == "min_length");
    }

    [Fact]
    public async Task Switch_NoMatchNoDefault_ReturnsSuccess()
    {
        var schema = Z.Object<Animal>()
            .Switch(s => s
                .Case(
                    a => a.Type == "Dog",
                    dog => dog.Field(a => a.BarkVolume, Z.Int().Min(0).Max(100))));

        // Unknown type, no default -> no validation applied, success
        var result = await schema.ValidateAsync(new Animal("Fish", "Nemo", 999, 999));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Switch_ValidCase_PassesValidation()
    {
        var schema = Z.Object<Animal>()
            .Switch(s => s
                .Case(
                    a => a.Type == "Dog",
                    dog => dog.Field(a => a.BarkVolume, Z.Int().Min(0).Max(100)))
                .Case(
                    a => a.Type == "Cat",
                    cat => cat.Field(a => a.ClawSharpness, Z.Int().Min(0).Max(10))));

        // Valid dog
        var result = await schema.ValidateAsync(new Animal("Dog", "Rex", 50, 0));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Switch_OnlyFirstMatchExecutes()
    {
        var schema = Z.Object<Animal>()
            .Switch(s => s
                .Case(
                    a => a.Type == "Dog",
                    dog => dog.Field(a => a.BarkVolume, Z.Int().Min(0).Max(100)))
                .Case(
                    a => a.Type == "Dog", // duplicate condition
                    dog2 => dog2.Field(a => a.Name, Z.String().MinLength(50))));

        // Dog: first case matches, second case not evaluated
        // BarkVolume=50 is valid, Name="Rex" would fail MinLength(50) but isn't checked
        var result = await schema.ValidateAsync(new Animal("Dog", "Rex", 50, 0));
        Assert.True(result.IsSuccess);
    }

    // ==================== Context-Aware Switch Tests ====================

    public record AnimalContext(int MaxBark);

    [Fact]
    public async Task Switch_ContextAware_UsesContextInConditions()
    {
        var schema = Z.Object<Animal>()
            .WithContext<AnimalContext>()
            .Switch(s => s
                .Case(
                    (a, ctx) => a.Type == "Dog",
                    dog => dog.Field(a => a.BarkVolume, Z.Int().Min(0)))
                .Default(
                    other => other.Field(a => a.Name, Z.String().MinLength(1))));

        var context = new ValidationContext<AnimalContext>(new AnimalContext(MaxBark: 50));

        // Dog with negative BarkVolume -> fails Min(0)
        var result = await schema.ValidateAsync(new Animal("Dog", "Rex", -5, 0), context);
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Path == "barkVolume" && e.Code == "min_value");

        // Cat -> Default applies, valid name
        var catResult = await schema.ValidateAsync(new Animal("Cat", "Whiskers", 0, 5), context);
        Assert.True(catResult.IsSuccess);
    }

    // ==================== Switch Combined with Other Schema Methods ====================

    [Fact]
    public async Task Switch_CombinedWithFields_ValidatesFieldsAndSwitch()
    {
        var schema = Z.Object<Animal>()
            .Field(a => a.Name, Z.String().MinLength(1))
            .Switch(s => s
                .Case(
                    a => a.Type == "Dog",
                    dog => dog.Field(a => a.BarkVolume, Z.Int().Min(0).Max(100))));

        // Invalid name AND invalid BarkVolume -> both errors
        var result = await schema.ValidateAsync(new Animal("Dog", "", 150, 0));
        Assert.False(result.IsSuccess);
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains(result.Errors, e => e.Path == "name");
        Assert.Contains(result.Errors, e => e.Path == "barkVolume");
    }
}
