using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Zeta.AspNetCore.Tests;

public class UsersControllerTests : IntegrationTestBase
{
    public UsersControllerTests(WebApplicationFactory<Program> factory) : base(factory) { }

    #region Registration - Simple

    [Fact]
    public async Task RegisterSimple_ValidRequest_ReturnsOk()
    {
        var request = new
        {
            email = "john@example.com",
            password = "Password123",
            confirmPassword = "Password123",
            name = "John Doe",
            age = 25
        };

        var response = await Client.PostAsJsonAsync("/api/users/register/simple", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("john@example.com", content.GetProperty("email").GetString());
    }

    [Fact]
    public async Task RegisterSimple_PasswordMismatch_ReturnsBadRequest()
    {
        var request = new
        {
            email = "john@example.com",
            password = "Password123",
            confirmPassword = "DifferentPassword",
            name = "John Doe",
            age = 25
        };

        var response = await Client.PostAsJsonAsync("/api/users/register/simple", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await GetValidationErrors(response);
        Assert.Contains(errors, e => e.Code == "password_mismatch");
    }

    [Fact]
    public async Task RegisterSimple_WeakPassword_ReturnsBadRequest()
    {
        var request = new
        {
            email = "john@example.com",
            password = "weak",
            confirmPassword = "weak",
            name = "John Doe",
            age = 25
        };

        var response = await Client.PostAsJsonAsync("/api/users/register/simple", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await GetValidationErrors(response);
        Assert.Contains(errors, e => e.Path == "password" && e.Code == "min_length");
    }

    #endregion

    #region Registration - Async

    [Fact]
    public async Task RegisterAsync_UniqueEmail_ReturnsOk()
    {
        var request = new
        {
            email = "unique@example.com",
            password = "Password123",
            confirmPassword = "Password123",
            name = "Jane Doe",
            age = 30
        };

        var response = await Client.PostAsJsonAsync("/api/users/register", request);

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
            name = "Jane Doe",
            age = 30
        };

        var response = await Client.PostAsJsonAsync("/api/users/register", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await GetValidationErrors(response);
        Assert.Contains(errors, e => e.Path == "email" && e.Message!.Contains("already registered"));
    }

    #endregion

    #region Create with Address

    [Fact]
    public async Task CreateUser_WithValidAddress_ReturnsCreated()
    {
        var request = new
        {
            email = "user@example.com",
            name = "Bob Smith",
            hasAddress = true,
            address = new
            {
                street = "123 Main Street",
                city = "New York",
                state = "NY",
                zipCode = "10001",
                country = "USA"
            }
        };

        var response = await Client.PostAsJsonAsync("/api/users/create", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateUser_WithoutAddress_ReturnsCreated()
    {
        var request = new
        {
            email = "user@example.com",
            name = "Bob Smith",
            hasAddress = false,
            address = (object?)null
        };

        var response = await Client.PostAsJsonAsync("/api/users/create", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateUser_HasAddressTrueButInvalidAddress_ReturnsBadRequest()
    {
        var request = new
        {
            email = "user@example.com",
            name = "Bob Smith",
            hasAddress = true,
            address = new
            {
                street = "123",        // Too short (< 5)
                city = "X",            // Too short (< 2)
                state = "NEW YORK",    // Wrong length (not 2)
                zipCode = "invalid",   // Doesn't match regex
                country = (string?)null
            }
        };

        var response = await Client.PostAsJsonAsync("/api/users/create", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await GetValidationErrors(response);
        Assert.Contains(errors, e => e.Path == "address.street");
        Assert.Contains(errors, e => e.Path == "address.city");
        Assert.Contains(errors, e => e.Path == "address.state");
        Assert.Contains(errors, e => e.Path == "address.zipCode");
    }

    #endregion

    #region Profile Update

    [Fact]
    public async Task UpdateProfile_ValidRequest_ReturnsOk()
    {
        var request = new
        {
            name = "Robert Smith",
            phoneNumber = "+15551234567",
            dateOfBirth = "1990-05-15"
        };

        var response = await Client.PatchAsJsonAsync("/api/users/profile", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProfile_InvalidPhoneNumber_ReturnsBadRequest()
    {
        var request = new
        {
            name = "Robert Smith",
            phoneNumber = "not-a-phone",
            dateOfBirth = "1990-05-15"
        };

        var response = await Client.PatchAsJsonAsync("/api/users/profile", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await GetValidationErrors(response);
        Assert.Contains(errors, e => e.Path == "phoneNumber");
    }

    #endregion
}
