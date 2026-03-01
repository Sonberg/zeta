namespace Zeta.Sample.FastEndpoints.Api.Models;

public record RegisterRequest(
    string Email,
    string Password,
    int Age);

public record CreateProductRequest(
    string Name,
    string Sku,
    decimal Price,
    int StockQuantity);
