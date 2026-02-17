using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Zeta.AspNetCore.Tests;

public class MinimalApiTests : IntegrationTestBase
{
    public MinimalApiTests(WebApplicationFactory<Program> factory) : base(factory) { }

    #region Users

    [Fact]
    public async Task RegisterSimple_ValidRequest_ReturnsOk()
    {
        var request = new
        {
            email = "minimal@example.com",
            password = "Password123",
            confirmPassword = "Password123",
            name = "Minimal User",
            age = 28
        };

        var response = await Client.PostAsJsonAsync("/api/minimal/users/register", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task RegisterAsync_EmailTaken_ReturnsBadRequest()
    {
        var request = new
        {
            email = "taken@example.com",
            password = "Password123",
            confirmPassword = "Password123",
            name = "Minimal User",
            age = 28
        };

        var response = await Client.PostAsJsonAsync("/api/minimal/users/register/async", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateUser_WithConditionalAddress_ReturnsCreated()
    {
        var request = new
        {
            email = "minimal-user@example.com",
            name = "Minimal User",
            hasAddress = true,
            address = new
            {
                street = "789 Minimal Lane",
                city = "Seattle",
                state = "WA",
                zipCode = "98101",
                country = (string?)null
            }
        };

        var response = await Client.PostAsJsonAsync("/api/minimal/users", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    #endregion

    #region Products

    [Fact]
    public async Task CreateProduct_ValidRequest_ReturnsCreated()
    {
        var request = new
        {
            name = "Minimal Product",
            description = "Created via minimal API",
            sku = "MIN-001",
            price = 19.99m,
            stockQuantity = 25,
            tags = new[] { "minimal", "api" }
        };

        var response = await Client.PostAsJsonAsync("/api/minimal/products", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task UpdatePrice_ValidRequest_ReturnsOk()
    {
        var request = new
        {
            price = 14.99m,
            compareAtPrice = 19.99m
        };

        var response = await Client.PatchAsJsonAsync(
            "/api/minimal/products/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa/price", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SearchProducts_ValidRequest_ReturnsOk()
    {
        var response = await Client.GetAsync("/api/minimal/products/search?query=test&page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion

    #region Orders

    [Fact]
    public async Task CreateOrder_ValidRequest_ReturnsCreated()
    {
        var request = new
        {
            customerId = "22222222-2222-2222-2222-222222222222",
            items = new[]
            {
                new { productId = "cccccccc-cccc-cccc-cccc-cccccccccccc", quantity = 3, notes = "Via minimal API" }
            },
            shippingAddress = new
            {
                street = "321 Minimal Way",
                city = "Portland",
                state = "OR",
                zipCode = "97201",
                country = "USA"
            },
            billingAddress = (object?)null,
            couponCode = "WELCOME20",
            paymentMethod = "paypal"
        };

        var response = await Client.PostAsJsonAsync("/api/minimal/orders", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task ScheduleDelivery_ValidRequest_ReturnsOk()
    {
        var request = new
        {
            orderId = "11111111-0000-0000-0000-000000000002",
            deliveryDate = DateOnly.FromDateTime(DateTime.Today.AddDays(60)).ToString("yyyy-MM-dd"),
            preferredTimeStart = "10:00:00",
            preferredTimeEnd = "14:00:00"
        };

        var response = await Client.PostAsJsonAsync("/api/minimal/orders/delivery", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion

    #region Files

    [Fact]
    public async Task UploadFile_ValidMultipartRequest_ReturnsOk()
    {
        using var content = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes("hello-world"));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        content.Add(fileContent, "file", "test.txt");

        var response = await Client.PostAsync("/api/minimal/files/upload", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UploadFile_EmptyFile_ReturnsBadRequest()
    {
        using var content = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent([]);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        content.Add(fileContent, "file", "empty.txt");

        var response = await Client.PostAsync("/api/minimal/files/upload", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var errors = await GetValidationErrors(response);
        Assert.Contains(errors, e => e.Message == "File cannot be empty");
    }

    #endregion
}
