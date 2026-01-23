using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Zeta.AspNetCore.Tests;

public class ValidationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ValidationIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AsyncValidation_ValidRequest_ReturnsOk()
    {
        var client = _factory.CreateClient();
        var user = new User("JohnDoe", "valid@example.com");

        var response = await client.PostAsJsonAsync("/async/users", user);

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.Equal("User created", content?.Message);
        Assert.Equal("JohnDoe", content?.User?.Name);
    }

    [Fact]
    public async Task AsyncValidation_InvalidEmail_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var user = new User("JohnDoe", "invalid-email");

        var response = await client.PostAsJsonAsync("/async/users", user);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problems = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        
        Assert.NotNull(problems);
        Assert.Contains("email", problems.Errors.Keys);
        Assert.Contains("Invalid email format", problems.Errors["email"]);
    }

    [Fact]
    public async Task AsyncValidation_TakenEmail_ReturnsBadRequest_FromContext()
    {
        var client = _factory.CreateClient();
        var user = new User("JohnDoe", "taken@example.com");

        var response = await client.PostAsJsonAsync("/async/users", user);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problems = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(problems);
        Assert.Contains("email", problems.Errors.Keys);
        Assert.Contains("Email already exists", problems.Errors["email"]);
    }

    [Fact]
    public async Task SyncValidation_ValidRequest_ReturnsOk()
    {
        var client = _factory.CreateClient();
        var user = new User("John", "sync@example.com");

        var response = await client.PostAsJsonAsync("/sync/users", user);

        response.EnsureSuccessStatusCode();
    }
    
    [Fact]
    public async Task SyncValidation_InvalidRequest_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var user = new User("J", "sync@example.com"); // Name too short

        var response = await client.PostAsJsonAsync("/sync/users", user);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Controller_ImplicitValidation_InvalidRequest_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var user = new User("J", "controller@example.com"); // Name too short (min 3)

        var response = await client.PostAsJsonAsync("/api/users", user);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problems = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problems);
        Assert.Contains("name", problems.Errors.Keys);
    }

    [Fact]
    public async Task Controller_ZetaIgnore_SkipsValidation()
    {
        var client = _factory.CreateClient();
        // Invalid user that would normally fail validation (name too short, bad email)
        var user = new User("J", "not-an-email");

        var response = await client.PostAsJsonAsync("/api/users/ignored", user);

        // Should succeed because [ZetaIgnore] skips validation
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<IgnoredUserResponse>();
        Assert.Equal("User created (validation ignored)", content?.Message);
    }

    [Fact]
    public async Task Controller_ZetaValidate_UsesSpecifiedSchema_Valid()
    {
        var client = _factory.CreateClient();
        // User that passes StrictUserSchema (name >= 5 chars)
        var user = new User("JohnDoe", "valid@example.com");

        var response = await client.PostAsJsonAsync("/api/users/strict", user);

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Controller_ZetaValidate_UsesSpecifiedSchema_Invalid()
    {
        var client = _factory.CreateClient();
        // User that passes default schema (name >= 3) but fails StrictUserSchema (name >= 5)
        var user = new User("Joe", "valid@example.com"); // 3 chars - fails strict

        var response = await client.PostAsJsonAsync("/api/users/strict", user);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problems = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problems);
        Assert.Contains("name", problems.Errors.Keys);
    }

    record User(string Name, string Email);
    record UserResponse(string Message, User User);
    record IgnoredUserResponse(string Message, User User);
    record ValidationProblemDetails(string Type, string Title, int Status, Dictionary<string, string[]> Errors);
}
