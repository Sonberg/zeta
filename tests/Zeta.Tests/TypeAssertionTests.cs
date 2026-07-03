using Microsoft.Extensions.DependencyInjection;
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
    public async Task As_ReturnsNewDerivedSchema()
    {
        var baseSchema = Z.Schema<IAnimal>();
        var dogSchema = baseSchema.As<Dog>();

        // As<Dog>() returns a new ObjectContextlessSchema<Dog>, not mutating the parent
        var result = await dogSchema.ValidateAsync(new Dog(50));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task As_DoesNotMutateParent()
    {
        var schema = Z.Schema<IAnimal>();
        schema.As<Dog>(); // return value not captured

        // Parent schema should not have type assertion — Cat passes
        var result = await schema.ValidateAsync(new Cat(5));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task As_WithIf_TypeMatches_ValidatesFields()
    {
        var schema = Z.Schema<IAnimal>()
            .If(x => x is Dog, c => c.As<Dog>()
                .Property(x => x.WoofVolume, x => x.Min(0).Max(100)));

        var result = await schema.ValidateAsync(new Dog(50));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task As_WithIf_ConditionFalse_SkipsAssertion()
    {
        var schema = Z.Schema<IAnimal>()
            .If(x => x is Dog, c => c.As<Dog>()
                .Property(x => x.WoofVolume, x => x.Min(0).Max(100)));

        var result = await schema.ValidateAsync(new Cat(5));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task As_WithIf_FieldValidationFails_ReportsFieldError()
    {
        var schema = Z.Schema<IAnimal>()
            .If(x => x is Dog, c => c.As<Dog>()
                .Property(x => x.WoofVolume, x => x.Min(0).Max(100)));

        var result = await schema.ValidateAsync(new Dog(150));

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Path == "$.woofVolume" && e.Code == "max_value");
    }

    [Fact]
    public async Task As_ContextAware_DoesNotMutateParent()
    {
        var schema = Z.Schema<IAnimal>()
            .Using<StrictContext>();
        schema.As<Dog>(); // return value not captured

        var ctx = new ValidationRun<StrictContext>(new StrictContext(true));

        // Parent schema should not have type assertion — Cat passes
        var result = await schema.ValidateAsync(new Cat(5), ctx);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task If_WithAs_TypeMismatch_ReportsError()
    {
        var dogSchema = Z.Schema<Dog>()
            .Property(x => x.WoofVolume, x => x.Min(0).Max(100));

        var schema = Z.Schema<IAnimal>()
            .If(x => x is Dog, dogSchema);

        // Dog with valid volume passes
        var dogResult = await schema.ValidateAsync(new Dog(50));
        Assert.True(dogResult.IsSuccess);

        // Dog with invalid volume fails
        var invalidDogResult = await schema.ValidateAsync(new Dog(150));
        Assert.False(invalidDogResult.IsSuccess);
        Assert.Contains(invalidDogResult.Errors, e => e.Code == "max_value");

        // Cat passes (condition not met)
        var catResult = await schema.ValidateAsync(new Cat(5));
        Assert.True(catResult.IsSuccess);
    }

    [Fact]
    public async Task If_WithAs_ContextPromotion_Works()
    {
        var dogSchema = Z.Schema<Dog>()
            .Property(x => x.WoofVolume, x => x.Min(0).Max(100));

        var schema = Z.Schema<IAnimal>()
            .If(x => x is Dog, dogSchema)
            .Using<StrictContext>();

        var ctx = new ValidationRun<StrictContext>(new StrictContext(true));

        // Dog should pass
        var dogResult = await schema.ValidateAsync(new Dog(50), ctx);
        Assert.True(dogResult.IsSuccess);

        // Cat should pass (no Dog predicate match)
        var catResult = await schema.ValidateAsync(new Cat(5), ctx);
        Assert.True(catResult.IsSuccess);
    }

    [Fact]
    public async Task As_ProgramCsExample_WorksCorrectly()
    {
        // Replicate the exact example from Program.cs
        var schema = Z.Schema<IAnimal>()
            .If(x => x is Dog, c => c.As<Dog>()
                .Property(x => x.WoofVolume, x => x.Min(0).Max(100)));

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
        var schema = Z.Schema<IAnimal>()
            .Using<StrictContext>()
            .If(x => x is Dog, c => c.As<Dog>()
                .Property(x => x.WoofVolume, x => x.Min(0).Max(100)));

        var ctx = new ValidationRun<StrictContext>(new StrictContext(true));

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

    // --- Type-narrowed .If() tests ---

    [Fact]
    public async Task IfGeneric_TypeMatches_ValidatesFields()
    {
        var schema = Z.Schema<IAnimal>()
            .If(x => x is Dog, dog => dog.As<Dog>().Property(x => x.WoofVolume, x => x.Min(0).Max(100)));

        var result = await schema.ValidateAsync(new Dog(50));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task IfGeneric_ConditionFalse_Skips()
    {
        var schema = Z.Schema<IAnimal>()
            .If(x => x is Dog, dog => dog.As<Dog>().Property(x => x.WoofVolume, x => x.Min(0).Max(100)));

        var result = await schema.ValidateAsync(new Cat(5));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task IfGeneric_FieldValidationFails_ReportsError()
    {
        var schema = Z.Schema<IAnimal>()
            .If(x => x is Dog, dog => dog.As<Dog>().Property(x => x.WoofVolume, x => x.Min(0).Max(100)));

        var result = await schema.ValidateAsync(new Dog(150));

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Path == "$.woofVolume" && e.Code == "max_value");
    }

    [Fact]
    public async Task IfGeneric_ContextAware_ValidatesFields()
    {
        var schema = Z.Schema<IAnimal>()
            .Using<StrictContext>()
            .If(x => x is Dog, dog => dog.As<Dog>().Property(x => x.WoofVolume, x => x.Min(0).Max(100)));

        var ctx = new ValidationRun<StrictContext>(new StrictContext(true));

        var validDog = await schema.ValidateAsync(new Dog(50), ctx);
        Assert.True(validDog.IsSuccess);

        var invalidDog = await schema.ValidateAsync(new Dog(150), ctx);
        Assert.False(invalidDog.IsSuccess);

        var cat = await schema.ValidateAsync(new Cat(5), ctx);
        Assert.True(cat.IsSuccess);
    }

    [Fact]
    public async Task IfGeneric_ContextAware_UsesPrebuiltSchema()
    {
        var dogSchema = Z.Schema<Dog>()
            .Using<StrictContext>()
            .Property(x => x.WoofVolume, x => x.Min(0).Max(100))
            .Refine((_, ctx) => ctx.IsStrict, "Strict context required for dogs");

        var schema = Z.Schema<IAnimal>()
            .Using<StrictContext>()
            .If(x => x is Dog, dogSchema);

        var strictCtx = new ValidationRun<StrictContext>(new StrictContext(true));
        var lenientCtx = new ValidationRun<StrictContext>(new StrictContext(false));

        var strictDog = await schema.ValidateAsync(new Dog(50), strictCtx);
        Assert.True(strictDog.IsSuccess);

        var lenientDog = await schema.ValidateAsync(new Dog(50), lenientCtx);
        Assert.False(lenientDog.IsSuccess);
        Assert.Contains(lenientDog.Errors, e => e.Message == "Strict context required for dogs");
    }

    [Fact]
    public async Task IfGeneric_MultipleBranches_ValidatesCorrectBranch()
    {
        var schema = Z.Schema<IAnimal>()
            .If(x => x is Dog, dog => dog.As<Dog>().Property(x => x.WoofVolume, x => x.Min(0).Max(100)))
            .If(x => x is Cat, cat => cat.As<Cat>().Property(x => x.ClawSharpness, x => x.Min(1).Max(10)));

        // Valid dog
        var validDog = await schema.ValidateAsync(new Dog(50));
        Assert.True(validDog.IsSuccess);

        // Invalid dog
        var invalidDog = await schema.ValidateAsync(new Dog(150));
        Assert.False(invalidDog.IsSuccess);

        // Valid cat
        var validCat = await schema.ValidateAsync(new Cat(5));
        Assert.True(validCat.IsSuccess);

        // Invalid cat
        var invalidCat = await schema.ValidateAsync(new Cat(20));
        Assert.False(invalidCat.IsSuccess);
        Assert.Contains(invalidCat.Errors, e => e.Path == "$.clawSharpness" && e.Code == "max_value");
    }

    [Fact]
    public async Task IfGeneric_ContextfulDerivedBranch_SelfResolves()
    {
        var services = new ServiceCollection().BuildServiceProvider();

        var dogSchema = Z.Schema<Dog>()
            .Property(x => x.WoofVolume, x => x.Min(0).Max(100))
            .Using<StrictContext>(async (_, _, _) => new StrictContext(true))
            .Refine((_, ctx) => ctx.IsStrict, "Strict context required for dogs");

        var catSchema = Z.Schema<Cat>()
            .Property(x => x.ClawSharpness, x => x.Min(1).Max(10));

        var schema = Z.Schema<IAnimal>()
            .If(x => x is Dog, dogSchema)
            .If(x => x is Cat, catSchema);

        var ctx = new ValidationRun(serviceProvider: services);

        var validDog = await schema.ValidateAsync(new Dog(50), ctx);
        Assert.True(validDog.IsSuccess);

        var invalidCat = await schema.ValidateAsync(new Cat(20), ctx);
        Assert.False(invalidCat.IsSuccess);
        Assert.Contains(invalidCat.Errors, e => e.Path == "$.clawSharpness" && e.Code == "max_value");
    }

    [Fact]
    public async Task IfGeneric_ContextfulDerivedBranch_FactoryReturnsFalse_Fails()
    {
        var services = new ServiceCollection().BuildServiceProvider();

        var dogSchema = Z.Schema<Dog>()
            .Property(x => x.WoofVolume, x => x.Min(0).Max(100))
            .Using<StrictContext>(async (_, _, _) => new StrictContext(false))
            .Refine((_, ctx) => ctx.IsStrict, "Strict context required for dogs");

        var schema = Z.Schema<IAnimal>()
            .If(x => x is Dog, dogSchema);

        var ctx = new ValidationRun(serviceProvider: services);

        var lenientDog = await schema.ValidateAsync(new Dog(50), ctx);
        Assert.False(lenientDog.IsSuccess);
        Assert.Contains(lenientDog.Errors, e => e.Message == "Strict context required for dogs");
    }

}
