using Zeta.Schemas;

namespace Zeta.Tests;

public class CollectionContextBridgeTests
{
    private record CreateOrderContext(int MaxQuantity);

    private record OrderItem(Guid ProductId, int Quantity);

    private record CreateOrderRequest(Guid CustomerId, OrderItem[] Items);

    private record CreateOrderRequestWithList(Guid CustomerId, List<OrderItem> Items);

    [Fact]
    public async Task ContextAwareObjectWithArrayField_UsingEach_ShouldCompile()
    {
        // This is the exact example from the RFC/problem statement
        var itemSchema = Z.Object<OrderItem>()
            .Field(x => x.ProductId, Z.Guid())
            .Field(x => x.Quantity, Z.Int().Min(1).Max(100));

        var schema = Z.Object<CreateOrderRequest>()
            .WithContext<CreateOrderContext>()
            .Field(x => x.CustomerId, x => x.NotEmpty())
            .Field(x => x.Items, x => x.Each(itemSchema));

        var context = new CreateOrderContext(100);
        var order = new CreateOrderRequest(
            Guid.NewGuid(),
            [
                new OrderItem(Guid.NewGuid(), 5),
                new OrderItem(Guid.NewGuid(), 10)
            ]);

        var result = await schema.ValidateAsync(order, context);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ContextAwareObjectWithListField_UsingEach_ShouldCompile()
    {
        var itemSchema = Z.Object<OrderItem>()
            .Field(x => x.ProductId, Z.Guid())
            .Field(x => x.Quantity, Z.Int().Min(1).Max(100));

        var schema = Z.Object<CreateOrderRequestWithList>()
            .WithContext<CreateOrderContext>()
            .Field(x => x.CustomerId, x => x.NotEmpty())
            .Field(x => x.Items, x => x.Each(itemSchema));

        var context = new CreateOrderContext(100);
        var order = new CreateOrderRequestWithList(
            Guid.NewGuid(),
            [
                new OrderItem(Guid.NewGuid(), 5),
                new OrderItem(Guid.NewGuid(), 10)
            ]);

        var result = await schema.ValidateAsync(order, context);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ContextAwareObjectWithArrayField_InvalidElement_ReturnsError()
    {
        var itemSchema = Z.Object<OrderItem>()
            .Field(x => x.ProductId, Z.Guid())
            .Field(x => x.Quantity, Z.Int().Min(1).Max(100));

        var schema = Z.Object<CreateOrderRequest>()
            .WithContext<CreateOrderContext>()
            .Field(x => x.CustomerId, x => x.NotEmpty())
            .Field(x => x.Items, x => x.Each(itemSchema));

        var context = new CreateOrderContext(100);
        var order = new CreateOrderRequest(
            Guid.NewGuid(),
            [
                new OrderItem(Guid.NewGuid(), 5),
                new OrderItem(Guid.NewGuid(), 150) // Invalid: exceeds max
            ]);

        var result = await schema.ValidateAsync(order, context);
        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Equal("items[1].quantity", result.Errors[0].Path);
        Assert.Equal("max_value", result.Errors[0].Code);
    }

    [Fact]
    public async Task ContextAwareObjectWithPrimitiveArray_UsingEach_ShouldCompile()
    {
        var schema = Z.Object<TaggedItem>()
            .WithContext<CreateOrderContext>()
            .Field(x => x.Tags, x => x.Each(t => t.MinLength(3).MaxLength(20)));

        var context = new CreateOrderContext(100);
        var item = new TaggedItem(["coding", "music", "sports"]);

        var result = await schema.ValidateAsync(item, context);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ContextAwareObjectWithPrimitiveArray_InvalidElement_ReturnsError()
    {
        var schema = Z.Object<TaggedItem>()
            .WithContext<CreateOrderContext>()
            .Field(x => x.Tags, x => x.Each(t => t.MinLength(3)));

        var context = new CreateOrderContext(100);
        var item = new TaggedItem(["coding", "ab"]); // "ab" is too short

        var result = await schema.ValidateAsync(item, context);
        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Equal("tags[1]", result.Errors[0].Path);
        Assert.Equal("min_length", result.Errors[0].Code);
    }

    [Fact]
    public async Task ContextAwareObjectWithPrimitiveList_UsingEach_ShouldCompile()
    {
        var schema = Z.Object<TaggedItemWithList>()
            .WithContext<CreateOrderContext>()
            .Field(x => x.Tags, x => x.Each(t => t.MinLength(3).MaxLength(20)));

        var context = new CreateOrderContext(100);
        var item = new TaggedItemWithList(["coding", "music", "sports"]);

        var result = await schema.ValidateAsync(item, context);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ContextAwareObjectWithIntArray_UsingEach_ShouldCompile()
    {
        var schema = Z.Object<NumberContainer>()
            .WithContext<CreateOrderContext>()
            .Field(x => x.Numbers, x => x.Each(n => n.Min(0).Max(100)));

        var context = new CreateOrderContext(100);
        var container = new NumberContainer([1, 50, 100]);

        var result = await schema.ValidateAsync(container, context);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ContextAwareObjectWithIntArray_InvalidElement_ReturnsError()
    {
        var schema = Z.Object<NumberContainer>()
            .WithContext<CreateOrderContext>()
            .Field(x => x.Numbers, x => x.Each(n => n.Min(0).Max(100)));

        var context = new CreateOrderContext(100);
        var container = new NumberContainer([1, 150]); // 150 exceeds max

        var result = await schema.ValidateAsync(container, context);
        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Equal("numbers[1]", result.Errors[0].Path);
        Assert.Equal("max_value", result.Errors[0].Code);
    }

    [Fact]
    public async Task ContextAwareObjectWithCollectionValidation_ShouldWork()
    {
        var itemSchema = Z.Object<OrderItem>()
            .Field(x => x.ProductId, Z.Guid())
            .Field(x => x.Quantity, Z.Int().Min(1).Max(100));

        var schema = Z.Object<CreateOrderRequest>()
            .WithContext<CreateOrderContext>()
            .Field(x => x.CustomerId, x => x.NotEmpty())
            .Field(x => x.Items, x => x
                .Each(itemSchema)
                .MinLength(1)
                .MaxLength(10));

        var context = new CreateOrderContext(100);
        var order = new CreateOrderRequest(
            Guid.NewGuid(),
            [
                new OrderItem(Guid.NewGuid(), 5)
            ]);

        var result = await schema.ValidateAsync(order, context);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ContextAwareObjectWithEmptyArray_FailsMinLength()
    {
        var itemSchema = Z.Object<OrderItem>()
            .Field(x => x.ProductId, Z.Guid())
            .Field(x => x.Quantity, Z.Int().Min(1));

        var schema = Z.Object<CreateOrderRequest>()
            .WithContext<CreateOrderContext>()
            .Field(x => x.Items, x => x
                .Each(itemSchema)
                .MinLength(1));

        var context = new CreateOrderContext(100);
        var order = new CreateOrderRequest(Guid.NewGuid(), []);

        var result = await schema.ValidateAsync(order, context);
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "min_length" && e.Path == "items");
    }

    [Fact]
    public async Task ExplicitWithContext_StillWorks()
    {
        // Users should still be able to call .WithContext<TContext>() explicitly
        var itemSchema = Z.Object<OrderItem>()
            .Field(x => x.ProductId, Z.Guid())
            .Field(x => x.Quantity, Z.Int().Min(1).Max(100));

        var schema = Z.Object<CreateOrderRequest>()
            .WithContext<CreateOrderContext>()
            .Field(x => x.Items, x => x
                .Each(itemSchema)
                .WithContext<CreateOrderContext>());

        var context = new CreateOrderContext(100);
        var order = new CreateOrderRequest(
            Guid.NewGuid(),
            [new OrderItem(Guid.NewGuid(), 5)]);

        var result = await schema.ValidateAsync(order, context);
        Assert.True(result.IsSuccess);
    }

    private record TaggedItem(string[] Tags);

    private record TaggedItemWithList(List<string> Tags);

    private record NumberContainer(int[] Numbers);
}