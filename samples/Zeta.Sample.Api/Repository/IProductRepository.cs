namespace Zeta.Sample.Api.Repository;

public interface IProductRepository
{
    Task<bool> SkuExistsAsync(string sku, CancellationToken ct = default);
    Task<bool> ProductExistsAsync(Guid productId, CancellationToken ct = default);
    Task<decimal?> GetCurrentPriceAsync(Guid productId, CancellationToken ct = default);
}
