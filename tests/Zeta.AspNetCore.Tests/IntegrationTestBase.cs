using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Zeta.AspNetCore.Tests;

public class IntegrationTestBase : IClassFixture<WebApplicationFactory<Program>>
{
    protected readonly HttpClient Client;
    protected static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public IntegrationTestBase(WebApplicationFactory<Program> factory)
    {
        Client = factory.CreateClient();
    }

    /// <summary>
    /// Parses ValidationProblemDetails format: { "errors": { "path": ["message1", "message2"] } }
    /// </summary>
    protected static async Task<List<ValidationErrorDto>> GetValidationErrors(HttpResponseMessage response)
    {
        var content = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var errors = new List<ValidationErrorDto>();

        if (content.TryGetProperty("errors", out var errorsElement))
        {
            foreach (var pathProperty in errorsElement.EnumerateObject())
            {
                var path = pathProperty.Name;
                foreach (var messageElement in pathProperty.Value.EnumerateArray())
                {
                    var message = messageElement.GetString();
                    errors.Add(new ValidationErrorDto
                    {
                        Path = path,
                        Code = InferCodeFromMessage(message),
                        Message = message
                    });
                }
            }
        }

        return errors;
    }

    /// <summary>
    /// Infers error code from common validation messages
    /// </summary>
    private static string? InferCodeFromMessage(string? message)
    {
        if (string.IsNullOrEmpty(message)) return null;

        return message.ToLowerInvariant() switch
        {
            var m when m.Contains("passwords do not match") => "password_mismatch",
            var m when m.Contains("minimum length") || m.Contains("at least") && m.Contains("character") => "min_length",
            var m when m.Contains("maximum length") => "max_length",
            var m when m.Contains("compare-at price must be greater") => "invalid_compare_price",
            var m when m.Contains("maximum price must be greater than or equal to minimum") => "invalid_price_range",
            var m when m.Contains("end time must be after start time") => "invalid_time_range",
            _ => null
        };
    }

    protected class ValidationErrorDto
    {
        public string? Path { get; set; }
        public string? Code { get; set; }
        public string? Message { get; set; }
    }
}
