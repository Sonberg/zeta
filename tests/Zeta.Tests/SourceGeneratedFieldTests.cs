using Zeta.Core;

namespace Zeta.Tests;

public class SourceGeneratedFieldTests
{
    private record TestUserAddress
    {
        public string Street { get; init; } = "";
        public string City { get; init; } = "";
        public string ZipCode { get; init; } = "";
    }

    private record TestUser
    {
        public string Name { get; init; } = "";
        public int Age { get; init; }
        public decimal Balance { get; init; }
        public double Rating { get; init; }
        public bool IsActive { get; init; }
        public Guid Id { get; init; }
        public DateTime CreatedAt { get; init; }
        public List<string> Roles { get; init; } = [];
        public TestUserAddress Address { get; init; } = new();
    }

    [Fact]
    public async Task Field_StringWithFluentBuilder_Works()
    {
        var schema = Z.Object<TestUser>()
            .Field(u => u.Name, s => s.MinLength(3).MaxLength(50));

        var validUser = new TestUser
        {
            Name = "John Doe"
        };
        var result = await schema.ValidateAsync(validUser);
        Assert.True(result.IsSuccess);

        var invalidUser = new TestUser
        {
            Name = "Jo"
        };
        var result2 = await schema.ValidateAsync(invalidUser);
        Assert.False(result2.IsSuccess);
        Assert.Contains(result2.Errors, e => e.Code == "min_length");
    }

    [Fact]
    public async Task Field_IntWithFluentBuilder_Works()
    {
        var schema = Z.Object<TestUser>()
            .Field(u => u.Age, s => s.Min(18).Max(100));

        var validUser = new TestUser
        {
            Age = 25
        };
        var result = await schema.ValidateAsync(validUser);
        Assert.True(result.IsSuccess);

        var invalidUser = new TestUser
        {
            Age = 150
        };
        var result2 = await schema.ValidateAsync(invalidUser);
        Assert.False(result2.IsSuccess);
        Assert.Contains(result2.Errors, e => e.Code == "max_value");
    }

    [Fact]
    public async Task Field_DecimalWithFluentBuilder_Works()
    {
        var schema = Z.Object<TestUser>()
            .Field(u => u.Balance, s => s.Positive().Precision(2));

        var validUser = new TestUser
        {
            Balance = 99.99m
        };
        var result = await schema.ValidateAsync(validUser);
        Assert.True(result.IsSuccess);

        var invalidUser = new TestUser
        {
            Balance = -10m
        };
        var result2 = await schema.ValidateAsync(invalidUser);
        Assert.False(result2.IsSuccess);
        Assert.Contains(result2.Errors, e => e.Code == "positive");
    }

    [Fact]
    public async Task Field_DoubleWithFluentBuilder_Works()
    {
        var schema = Z.Object<TestUser>()
            .Field(u => u.Rating, s => s.Min(0.0).Max(5.0));

        var validUser = new TestUser
        {
            Rating = 4.5
        };
        var result = await schema.ValidateAsync(validUser);
        Assert.True(result.IsSuccess);

        var invalidUser = new TestUser
        {
            Rating = 10.0
        };
        var result2 = await schema.ValidateAsync(invalidUser);
        Assert.False(result2.IsSuccess);
        Assert.Contains(result2.Errors, e => e.Code == "max_value");
    }

    [Fact]
    public async Task Field_ObjectWithFluentBuilder_Works()
    {
        var schema = Z.Object<TestUser>()
            .Field(u => u.Address, u => u
                .Field(x => x.Street, ss => ss.MinLength(5))
                .Field(x => x.City, ss => ss.MinLength(2))
                .Field(x => x.ZipCode, ss => ss.MinLength(4).MaxLength(10)));

        var validUser = new TestUser
        {
            Address = new TestUserAddress
            {
                City = "Stockholm",
                Street = "Main Street 123",
                ZipCode = "12345"
            }
        };

        var result = await schema.ValidateAsync(validUser);

        Assert.True(result.IsSuccess);

        var invalidUser = new TestUser
        {
            Address = new TestUserAddress()
        };

        var result2 = await schema.ValidateAsync(invalidUser);
        Assert.False(result2.IsSuccess);
        Assert.NotEmpty(result2.Errors);
    }

    [Fact]
    public async Task Field_CollectionWithFluentBuilder_Works()
    {
        var schema = Z.Object<TestUser>()
            .Field(u => u.Roles, u => u.MinLength(1));

        var validUser = new TestUser
        {
            Roles = ["Admin"]
        };

        var result = await schema.ValidateAsync(validUser);

        Assert.True(result.IsSuccess);

        var invalidUser = new TestUser
        {
            Roles = []
        };

        var result2 = await schema.ValidateAsync(invalidUser);

        Assert.False(result2.IsSuccess);
        Assert.NotEmpty(result2.Errors);
    }

    [Fact]
    public async Task Field_MultipleFieldsWithFluentBuilders_Works()
    {
        var schema = Z.Object<TestUser>()
            .Field(u => u.Name, s => s.MinLength(2))
            .Field(u => u.Age, s => s.Min(18))
            .Field(u => u.Balance, s => s.Positive())
            .Field(u => u.IsActive, s => s);

        var validUser = new TestUser
        {
            Name = "John",
            Age = 30,
            Balance = 100.50m,
            IsActive = true
        };

        var result = await schema.ValidateAsync(validUser);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Field_WithContextPromotion_Works()
    {
        var schema = Z.Object<TestUser>()
            .Field(u => u.Name, s => s.MinLength(3))
            .WithContext<TestContext>()
            .Refine((user, ctx) => user.Name != ctx.MagicWord, "Name cannot be magic word");

        var context = Z.Context(new TestContext
        {
            MagicWord = "Forbidden"
        });

        var validUser = new TestUser
        {
            Name = "John"
        };
        var result = await schema.ValidateAsync(validUser, context);
        Assert.True(result.IsSuccess);

        var invalidUser = new TestUser
        {
            Name = "Forbidden"
        };
        var result2 = await schema.ValidateAsync(invalidUser, context);
        Assert.False(result2.IsSuccess);
    }

    private class TestContext
    {
        public string MagicWord { get; init; } = "";
    }
}