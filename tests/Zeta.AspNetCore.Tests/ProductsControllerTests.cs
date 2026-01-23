using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Zeta.AspNetCore.Tests;

public class ProductsControllerTests : IntegrationTestBase
{
    public ProductsControllerTests(WebApplicationFactory<Program> factory) : base(factory) { }

    #region Create Product

    [Fact]
    public async Task CreateProductSimple_ValidRequest_ReturnsCreated()
    {
        var request = new
        {
            name = "Wireless Mouse",
            description = "Ergonomic wireless mouse with USB receiver",
            sku = "MOUSE-001",
            price = 29.99m,
            stockQuantity = 100,
            tags = new[] { "electronics", "peripherals", "wireless" }
        };

        var response = await Client.PostAsJsonAsync("/api/products/simple", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateProductAsync_UniqueSku_ReturnsCreated()
    {
        var request = new
        {
            name = "Mechanical Keyboard",
            description = "RGB mechanical keyboard with Cherry MX switches",
            sku = "KB-MECH-001",
            price = 149.99m,
            stockQuantity = 50,
            tags = new[] { "electronics", "peripherals", "gaming" }
        };

        var response = await Client.PostAsJsonAsync("/api/products", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateProductAsync_SkuExists_ReturnsBadRequest()
    {
        var request = new
        {
            name = "Another Product",
            description = "This SKU is already taken",
            sku = "SKU-001",
            price = 49.99m,
            stockQuantity = 10,
            tags = new[] { "duplicate" }
        };

        var response = await Client.PostAsJsonAsync("/api/products", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await GetValidationErrors(response);
        Assert.Contains(errors, e => e.Path == "sku" && e.Message!.Contains("already exists"));
    }

    [Fact]
    public async Task CreateProductSimple_MultipleErrors_ReturnsBadRequest()
    {
        var request = new
        {
            name = "X",              // Too short (< 2)
            description = (string?)null,
            sku = "invalid sku!",   // Invalid format
            price = -10m,           // Below minimum
            stockQuantity = -5,     // Below minimum
            tags = Array.Empty<string>()
        };

        var response = await Client.PostAsJsonAsync("/api/products/simple", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await GetValidationErrors(response);
        Assert.Contains(errors, e => e.Path == "name");
        Assert.Contains(errors, e => e.Path == "sku");
        Assert.Contains(errors, e => e.Path == "price");
        Assert.Contains(errors, e => e.Path == "stockQuantity");
    }

    #endregion

    #region Update Price

    [Fact]
    public async Task UpdatePrice_ValidRequest_ReturnsOk()
    {
        var request = new
        {
            price = 24.99m,
            compareAtPrice = 29.99m
        };

        var response = await Client.PatchAsJsonAsync(
            "/api/products/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa/price", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdatePrice_CompareAtPriceLowerThanPrice_ReturnsBadRequest()
    {
        var request = new
        {
            price = 29.99m,
            compareAtPrice = 24.99m
        };

        var response = await Client.PatchAsJsonAsync(
            "/api/products/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa/price", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await GetValidationErrors(response);
        Assert.Contains(errors, e => e.Code == "invalid_compare_price");
    }

    #endregion

    #region Search Products

    [Fact]
    public async Task SearchProducts_ValidRequest_ReturnsOk()
    {
        var response = await Client.GetAsync(
            "/api/products/search?query=mouse&minPrice=10&maxPrice=100&page=1&pageSize=20");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SearchProducts_PageSizeTooLarge_ReturnsBadRequest()
    {
        var response = await Client.GetAsync(
            "/api/products/search?query=keyboard&page=1&pageSize=500");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await GetValidationErrors(response);
        Assert.Contains(errors, e => e.Path == "pageSize");
    }

    [Fact]
    public async Task SearchProducts_MinPriceGreaterThanMaxPrice_ReturnsBadRequest()
    {
        var response = await Client.GetAsync(
            "/api/products/search?minPrice=100&maxPrice=50&page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await GetValidationErrors(response);
        Assert.Contains(errors, e => e.Code == "invalid_price_range");
    }

    #endregion
}
