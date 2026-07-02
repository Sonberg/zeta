using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Zeta.FastEndpoints.Tests;

public class ZetaEndpointExtensionsTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public async Task AutoDiscovered_ValidRequest_Returns200()
    {
        var request = new { productId = "ABC-123", quantity = 5 };

        var response = await _client.PostAsJsonAsync("/api/orders", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AutoDiscovered_InvalidQuantity_Returns400()
    {
        var request = new { productId = "ABC-123", quantity = 0 };

        var response = await _client.PostAsJsonAsync("/api/orders", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await GetValidationErrors(response);
        Assert.Contains(errors, e => e.Path == "$.quantity");
    }

    [Fact]
    public async Task AutoDiscovered_EmptyProductId_Returns400WithPath()
    {
        var request = new { productId = "", quantity = 1 };

        var response = await _client.PostAsJsonAsync("/api/orders", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await GetValidationErrors(response);
        Assert.Contains(errors, e => e.Path == "$.productId");
    }

    private static async Task<List<ValidationErrorDto>> GetValidationErrors(HttpResponseMessage response)
    {
        var content = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var result = new List<ValidationErrorDto>();

        if (!content.TryGetProperty("errors", out var errorsElement)) return result;

        foreach (var prop in errorsElement.EnumerateObject())
        {
            foreach (var item in prop.Value.EnumerateArray())
            {
                result.Add(new ValidationErrorDto { Path = prop.Name, Message = item.GetString() });
            }
        }

        return result;
    }

    private class ValidationErrorDto
    {
        public string? Path { get; set; }
        public string? Message { get; set; }
    }
}
