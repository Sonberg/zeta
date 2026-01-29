#!/usr/bin/env dotnet-script
#r "nuget: Zeta, *"

using Zeta;
using Zeta.Schemas;

// Define models
public record OrderItem(Guid ProductId, int Quantity);
public record CreateOrderRequest(Guid CustomerId, OrderItem[] Items);

// NEW: Use .Each() with inline object builders!
var schema = Z.Object<CreateOrderRequest>()
    .Field(x => x.CustomerId, Z.Guid())
    .Field(x => x.Items, Z.Collection<OrderItem>()
        .Each(item => item
            .Field(i => i.ProductId, Z.Guid())
            .Field(i => i.Quantity, Z.Int().Min(1).Max(100)))
        .MinLength(1));

// Test validation
var validOrder = new CreateOrderRequest(
    Guid.NewGuid(),
    [
        new OrderItem(Guid.NewGuid(), 5),
        new OrderItem(Guid.NewGuid(), 10)
    ]);

var invalidOrder = new CreateOrderRequest(
    Guid.NewGuid(),
    [
        new OrderItem(Guid.NewGuid(), 0),  // Invalid: quantity < 1
        new OrderItem(Guid.NewGuid(), 150) // Invalid: quantity > 100
    ]);

var result1 = await schema.ValidateAsync(validOrder);
Console.WriteLine($"Valid order: {result1.IsSuccess}"); // True

var result2 = await schema.ValidateAsync(invalidOrder);
Console.WriteLine($"Invalid order: {result2.IsSuccess}"); // False
Console.WriteLine($"Errors: {string.Join(", ", result2.Errors.Select(e => e.Path))}");
// Output: items[0].quantity, items[1].quantity
