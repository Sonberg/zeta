namespace Zeta.Sample.Api.Repository;

public class FakeProductRepository : IProductRepository
{
    private static readonly HashSet<string> ExistingSkus = new(StringComparer.OrdinalIgnoreCase)
    {
        "SKU-001",
        "SKU-002",
        "PROD-ABC"
    };

    private static readonly Dictionary<Guid, decimal> Products = new()
    {
        [Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")] = 29.99m,
        [Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb")] = 49.99m,
        [Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc")] = 99.99m
    };

    public Task<bool> SkuExistsAsync(string sku, CancellationToken ct = default)
        => Task.FromResult(ExistingSkus.Contains(sku));

    public Task<bool> ProductExistsAsync(Guid productId, CancellationToken ct = default)
        => Task.FromResult(Products.ContainsKey(productId));

    public Task<decimal?> GetCurrentPriceAsync(Guid productId, CancellationToken ct = default)
        => Task.FromResult(Products.GetValueOrDefault(productId) as decimal?);
}
