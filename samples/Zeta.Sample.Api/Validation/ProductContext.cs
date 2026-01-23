using Zeta.AspNetCore;
using Zeta.Sample.Api.Models;
using Zeta.Sample.Api.Repository;

namespace Zeta.Sample.Api.Validation;

// Context for product creation - checks SKU uniqueness
public record CreateProductContext(bool SkuExists);

public class CreateProductContextFactory : IValidationContextFactory<CreateProductRequest, CreateProductContext>
{
    private readonly IProductRepository _repo;

    public CreateProductContextFactory(IProductRepository repo) => _repo = repo;

    public async Task<CreateProductContext> CreateAsync(
        CreateProductRequest input,
        IServiceProvider services,
        CancellationToken ct)
    {
        var skuExists = await _repo.SkuExistsAsync(input.Sku, ct);
        return new CreateProductContext(skuExists);
    }
}
