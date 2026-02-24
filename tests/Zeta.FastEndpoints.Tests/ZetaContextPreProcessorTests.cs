using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Zeta.FastEndpoints.Tests;

/// <summary>
/// Tests for context-aware validation via the unified <see cref="ZetaPreProcessor{TRequest}"/>.
/// Context-aware schemas with a factory delegate implement <c>ISchema&lt;T&gt;</c> directly,
/// so no separate pre-processor type is needed.
/// </summary>
public class ZetaContextPreProcessorTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ZetaContextPreProcessorTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ValidRequest_UniqueEmail_Returns200()
    {
        var request = new { email = "newuser@example.com", password = "Password1", age = 25 };

        var response = await _client.PostAsJsonAsync("/api/users/register-ctx", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task DuplicateEmail_Returns400_WithEmailTakenError()
    {
        // "taken@example.com" is pre-seeded as existing in FakeUserRepository
        var request = new { email = "taken@example.com", password = "Password1", age = 25 };

        var response = await _client.PostAsJsonAsync("/api/users/register-ctx", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.True(content.TryGetProperty("errors", out _));
    }

    [Fact]
    public async Task InvalidEmail_ContextSchema_Returns400_WithEmailError()
    {
        var request = new { email = "not-an-email", password = "Password1", age = 25 };

        var response = await _client.PostAsJsonAsync("/api/users/register-ctx", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.True(content.TryGetProperty("errors", out var errorsElement));
        var hasEmailError = errorsElement.EnumerateObject().Any(p => p.Name == "$.email");
        Assert.True(hasEmailError);
    }

    [Fact]
    public async Task ContextFactory_ResolvesContext_ValidatesWithContext()
    {
        // Context is built from the request via the factory (checks email existence).
        // A unique email should pass context validation.
        var request = new { email = "unique@example.com", password = "Password1", age = 30 };

        var response = await _client.PostAsJsonAsync("/api/users/register-ctx", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
