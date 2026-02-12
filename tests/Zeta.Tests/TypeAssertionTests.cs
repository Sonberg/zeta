using Zeta.Core;

namespace Zeta.Tests;

public class TypeAssertionTests
{
    // Test models
    public interface IAnimal;
    public record Dog(int? WoofVolume) : IAnimal;
    public record Cat(int? ClawSharpness) : IAnimal;
    public record StrictContext(bool IsStrict);

    [Fact]
    public async Task As_TypeMatches_Succeeds()
    {
        var schema = Z.Object<IAnimal>();
        schema.As<Dog>();

        var result = await schema.ValidateAsync(new Dog(50));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task As_TypeMismatch_ReturnsTypeMismatchError()
    {
        var schema = Z.Object<IAnimal>();
        schema.As<Dog>();

        var result = await schema.ValidateAsync(new Cat(5));

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "type_mismatch");
    }

    [Fact]
    public async Task As_WithIf_TypeMatches_ValidatesFields()
    {
        var schema = Z.Object<IAnimal>()
            .If(x => x is Dog, c => c.As<Dog>()
                .Field(x => x.WoofVolume, x => x.Min(0).Max(100)));

        var result = await schema.ValidateAsync(new Dog(50));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task As_WithIf_ConditionFalse_SkipsAssertion()
    {
        var schema = Z.Object<IAnimal>()
            .If(x => x is Dog, c => c.As<Dog>()
                .Field(x => x.WoofVolume, x => x.Min(0).Max(100)));

        var result = await schema.ValidateAsync(new Cat(5));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task As_WithIf_FieldValidationFails_ReportsFieldError()
    {
        var schema = Z.Object<IAnimal>()
            .If(x => x is Dog, c => c.As<Dog>()
                .Field(x => x.WoofVolume, x => x.Min(0).Max(100)));

        var result = await schema.ValidateAsync(new Dog(150));

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Path == "woofVolume" && e.Code == "max_value");
    }

    [Fact]
    public async Task As_ContextAware_TypeMatches_Succeeds()
    {
        var schema = Z.Object<IAnimal>()
            .WithContext<StrictContext>();
        schema.As<Dog>();

        var ctx = new ValidationContext<StrictContext>(new StrictContext(true));
        var result = await schema.ValidateAsync(new Dog(50), ctx);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task As_ContextAware_TypeMismatch_ReturnsError()
    {
        var schema = Z.Object<IAnimal>()
            .WithContext<StrictContext>();
        schema.As<Dog>();

        var ctx = new ValidationContext<StrictContext>(new StrictContext(true));
        var result = await schema.ValidateAsync(new Cat(5), ctx);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "type_mismatch");
    }

    [Fact]
    public async Task As_ContextPromotion_TransfersTypeAssertion()
    {
        var contextless = Z.Object<IAnimal>();
        contextless.As<Dog>();
        var schema = contextless.WithContext<StrictContext>();

        var ctx = new ValidationContext<StrictContext>(new StrictContext(true));

        // Dog should pass
        var dogResult = await schema.ValidateAsync(new Dog(50), ctx);
        Assert.True(dogResult.IsSuccess);

        // Cat should fail with type_mismatch
        var catResult = await schema.ValidateAsync(new Cat(5), ctx);
        Assert.False(catResult.IsSuccess);
        Assert.Contains(catResult.Errors, e => e.Code == "type_mismatch");
    }

    [Fact]
    public async Task As_ErrorMessageFormat_ContainsTypeNames()
    {
        var schema = Z.Object<IAnimal>();
        schema.As<Dog>();

        var result = await schema.ValidateAsync(new Cat(5));

        Assert.False(result.IsSuccess);
        var error = Assert.Single(result.Errors);
        Assert.Equal("type_mismatch", error.Code);
        Assert.Contains("Dog", error.Message);
        Assert.Contains("Cat", error.Message);
    }

    [Fact]
    public async Task As_ProgramCsExample_WorksCorrectly()
    {
        // Replicate the exact example from Program.cs
        var schema = Z.Object<IAnimal>()
            .If(x => x is Dog, c => c.As<Dog>()
                .Field(x => x.WoofVolume, x => x.Min(0).Max(100)));

        // Dog with valid volume
        var validDog = await schema.ValidateAsync(new Dog(50));
        Assert.True(validDog.IsSuccess);

        // Dog with invalid volume
        var invalidDog = await schema.ValidateAsync(new Dog(150));
        Assert.False(invalidDog.IsSuccess);

        // Cat passes (condition not met)
        var cat = await schema.ValidateAsync(new Cat(5));
        Assert.True(cat.IsSuccess);
    }

    [Fact]
    public async Task As_WithIf_ContextAware_ValidatesFieldsWithContext()
    {
        var schema = Z.Object<IAnimal>()
            .WithContext<StrictContext>()
            .If(x => x is Dog, c => c.As<Dog>()
                .Field(x => x.WoofVolume, x => x.Min(0).Max(100)));

        var ctx = new ValidationContext<StrictContext>(new StrictContext(true));

        // Dog with valid volume
        var validDog = await schema.ValidateAsync(new Dog(50), ctx);
        Assert.True(validDog.IsSuccess);

        // Dog with invalid volume
        var invalidDog = await schema.ValidateAsync(new Dog(150), ctx);
        Assert.False(invalidDog.IsSuccess);

        // Cat passes
        var cat = await schema.ValidateAsync(new Cat(5), ctx);
        Assert.True(cat.IsSuccess);
    }
}
