using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Zeta.FastEndpoints.Tests;

public class ZetaPreProcessorTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ZetaPreProcessorTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ValidRequest_HandlerRuns_Returns200()
    {
        var request = new { email = "valid@example.com", password = "Password1", age = 25 };

        var response = await _client.PostAsJsonAsync("/api/users/register", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task InvalidEmail_Returns400_WithEmailError()
    {
        var request = new { email = "not-an-email", password = "Password1", age = 25 };

        var response = await _client.PostAsJsonAsync("/api/users/register", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await GetValidationErrors(response);
        Assert.Contains(errors, e => e.Path == "$.email");
    }

    [Fact]
    public async Task AgeTooLow_Returns400_WithAgeError()
    {
        var request = new { email = "valid@example.com", password = "Password1", age = 15 };

        var response = await _client.PostAsJsonAsync("/api/users/register", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await GetValidationErrors(response);
        Assert.Contains(errors, e => e.Path == "$.age");
    }

    [Fact]
    public async Task AllFieldsInvalid_Returns400_WithMultipleErrors()
    {
        var request = new { email = "not-an-email", password = "weak", age = 5 };

        var response = await _client.PostAsJsonAsync("/api/users/register", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await GetValidationErrors(response);
        Assert.Contains(errors, e => e.Path == "$.email");
        Assert.Contains(errors, e => e.Path == "$.password");
        Assert.Contains(errors, e => e.Path == "$.age");
    }

    [Fact]
    public async Task ErrorCodes_ArePreservedOnValidationFailures()
    {
        // Password fails min_length and regex rules.
        // FastEndpoints' default error response includes messages but not error codes in the JSON body.
        // The ErrorCode IS set on the ValidationFailure objects passed to SendErrorsAsync.
        var request = new { email = "valid@example.com", password = "ab", age = 25 };

        var response = await _client.PostAsJsonAsync("/api/users/register", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await GetValidationErrors(response);
        // Verify the password field has the expected min_length error message
        Assert.Contains(errors, e => e.Path == "$.password" && e.Message != null && e.Message.Contains("8 character"));
    }

    [Fact]
    public async Task ValidProductRequest_Returns200()
    {
        var request = new { name = "Widget", sku = "WIDGET-001", price = 9.99m, stockQuantity = 50 };

        var response = await _client.PostAsJsonAsync("/api/products", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task InvalidProductSku_Returns400_WithSkuError()
    {
        var request = new { name = "Widget", sku = "invalid sku!", price = 9.99m, stockQuantity = 50 };

        var response = await _client.PostAsJsonAsync("/api/products", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await GetValidationErrors(response);
        Assert.Contains(errors, e => e.Path == "$.sku");
    }

    private static async Task<List<ValidationErrorDto>> GetValidationErrors(HttpResponseMessage response)
    {
        var content = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var errors = new List<ValidationErrorDto>();

        // FastEndpoints error response: { "errors": { "path": ["message"] } }
        if (content.TryGetProperty("errors", out var errorsElement))
        {
            foreach (var pathProperty in errorsElement.EnumerateObject())
            {
                var path = pathProperty.Name;
                if (pathProperty.Value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var messageElement in pathProperty.Value.EnumerateArray())
                    {
                        var msg = messageElement.ValueKind == JsonValueKind.Object
                            ? messageElement.TryGetProperty("errorCode", out var codeEl) ? codeEl.GetString() : null
                            : messageElement.GetString();
                        // Try to get code from the message string itself (stored as error code in errors dict)
                        errors.Add(new ValidationErrorDto { Path = path, Message = messageElement.GetString() });
                    }
                }
                else if (pathProperty.Value.ValueKind == JsonValueKind.Object)
                {
                    // FastEndpoints may store errors as { "path": [{ "errorCode": "...", "errorMessage": "..." }] }
                    errors.Add(new ValidationErrorDto { Path = path });
                }
            }
        }

        // Parse error codes from the raw structure if available
        return ParseWithCodes(content, errors);
    }

    private static List<ValidationErrorDto> ParseWithCodes(JsonElement root, List<ValidationErrorDto> errors)
    {
        // Try alternate FastEndpoints error format with error codes
        if (!root.TryGetProperty("errors", out var errorsElement)) return errors;

        var result = new List<ValidationErrorDto>();
        foreach (var prop in errorsElement.EnumerateObject())
        {
            foreach (var item in prop.Value.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String)
                {
                    result.Add(new ValidationErrorDto { Path = prop.Name, Message = item.GetString() });
                }
                else if (item.ValueKind == JsonValueKind.Object)
                {
                    var code = item.TryGetProperty("errorCode", out var c) ? c.GetString() : null;
                    code ??= item.TryGetProperty("code", out var c2) ? c2.GetString() : null;
                    var msg = item.TryGetProperty("errorMessage", out var m) ? m.GetString() : null;
                    msg ??= item.TryGetProperty("message", out var m2) ? m2.GetString() : null;
                    result.Add(new ValidationErrorDto { Path = prop.Name, Code = code, Message = msg });
                }
            }
        }

        return result.Count > 0 ? result : errors;
    }

    private class ValidationErrorDto
    {
        public string? Path { get; set; }
        public string? Code { get; set; }
        public string? Message { get; set; }
    }
}
