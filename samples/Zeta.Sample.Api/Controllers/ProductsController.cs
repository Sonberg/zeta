using Microsoft.AspNetCore.Mvc;
using Zeta.AspNetCore;
using Zeta.Sample.Api.Models;
using Zeta.Sample.Api.Validation;

using ValidationSchemas = Zeta.Sample.Api.Validation.Schemas;

namespace Zeta.Sample.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IZetaValidator _validator;

    public ProductsController(IZetaValidator validator) => _validator = validator;

    /// <summary>
    /// Create product with context-aware validation (async SKU uniqueness).
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(CreateProductRequest request, CancellationToken ct)
    {
        var result = await _validator.ValidateAsync(request, ValidationSchemas.CreateProduct, ct);

        return result.ToActionResult(valid => CreatedAtAction(
            nameof(Create),
            new
            {
                Message = "Product created",
                Product = valid
            }));
    }

    /// <summary>
    /// Create product with simple validation (no async SKU check).
    /// </summary>
    [HttpPost("simple")]
    public async Task<IActionResult> CreateSimple(CreateProductRequest request, CancellationToken ct)
    {
        var result = await _validator.ValidateAsync(request, ValidationSchemas.CreateProductSimple, ct);

        return result.ToActionResult(valid => CreatedAtAction(
            nameof(CreateSimple),
            new
            {
                Message = "Product created (simple validation)",
                Product = valid
            }));
    }

    /// <summary>
    /// Update product price with cross-field validation.
    /// </summary>
    [HttpPatch("{id:guid}/price")]
    public async Task<IActionResult> UpdatePrice(Guid id, UpdatePriceRequest request, CancellationToken ct)
    {
        var result = await _validator.ValidateAsync(request, ValidationSchemas.UpdatePrice, ct);

        return result.ToActionResult(valid => Ok(new
        {
            Message = "Price updated",
            ProductId = id,
            Price = valid.Price,
            CompareAtPrice = valid.CompareAtPrice
        }));
    }

    /// <summary>
    /// Search products with pagination validation.
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] ProductSearchRequest request, CancellationToken ct)
    {
        var result = await _validator.ValidateAsync(request, ValidationSchemas.ProductSearch, ct);

        return result.ToActionResult(valid => Ok(new
        {
            Message = "Search results",
            Query = valid.Query,
            Pagination = new { valid.Page, valid.PageSize },
            PriceRange = new { valid.MinPrice, valid.MaxPrice },
            Results = Array.Empty<object>() // Placeholder
        }));
    }
}
