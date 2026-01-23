namespace Zeta.Sample.Api.Models;

public record CreateProductRequest(
    string Name,
    string? Description,
    string Sku,
    decimal Price,
    int StockQuantity,
    string[] Tags);

public record UpdatePriceRequest(
    decimal Price,
    decimal? CompareAtPrice);

public record ProductSearchRequest(
    string? Query,
    decimal? MinPrice,
    decimal? MaxPrice,
    int Page,
    int PageSize);
