using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Zeta.AspNetCore.Tests;

public class ExtensionsTests
{
    [Fact]
    public void ToActionResult_Success_UsesDefaultOkResult()
    {
        var result = Result<int>.Success(123);

        var actionResult = result.ToActionResult();

        var ok = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal(123, ok.Value);
    }

    [Fact]
    public void ToActionResult_Failure_GroupsErrorsByPath()
    {
        var result = Result<int>.Failure(
        [
            new ValidationError(ValidationPath.Parse("$.name"), "invalid", "Name is invalid"),
            new ValidationError(ValidationPath.Parse("$.name"), "required", "Name is required")
        ]);

        var actionResult = result.ToActionResult();

        var badRequest = Assert.IsType<BadRequestObjectResult>(actionResult);
        var details = Assert.IsType<ValidationProblemDetails>(badRequest.Value);
        Assert.True(details.Errors.TryGetValue("$.name", out var errors));
        Assert.Equal(2, errors.Length);
    }

    [Fact]
    public void ToResult_Success_UsesDefaultOkResult()
    {
        var result = Result<string>.Success("ok");

        var httpResult = result.ToResult();
        Assert.NotNull(httpResult);
    }

    [Fact]
    public void ToResult_Failure_ReturnsValidationProblem()
    {
        var result = Result<int>.Failure(
        [
            new ValidationError(ValidationPath.Parse("$.value"), "invalid", "bad")
        ]);

        var httpResult = result.ToResult();
        Assert.NotNull(httpResult);
    }

    [Fact]
    public void ToActionResult_WithIActionResultCallback_Success_InvokesCallback()
    {
        var result = Result<int>.Success(123);

        var actionResult = result.ToActionResult(value => new CreatedResult("/items/1", value));

        var created = Assert.IsType<CreatedResult>(actionResult);
        Assert.Equal(123, created.Value);
    }

    [Fact]
    public void ToActionResult_WithIActionResultCallback_Failure_ReturnsBadRequest()
    {
        var result = Result<int>.Failure(
        [
            new ValidationError(ValidationPath.Parse("$.value"), "invalid", "bad")
        ]);

        var actionResult = result.ToActionResult(value => new CreatedResult("/items/1", value));

        var badRequest = Assert.IsType<BadRequestObjectResult>(actionResult);
        Assert.IsType<ValidationProblemDetails>(badRequest.Value);
    }

    [Fact]
    public void ToActionResultOfT_WithCallback_Success_InvokesCallback()
    {
        var result = Result<int>.Success(123);

        var actionResult = result.ToActionResult<int, string>(value => $"value-{value}");

        Assert.Equal("value-123", actionResult.Value);
    }

    [Fact]
    public void ToActionResultOfT_WithNullCallback_Success_UsesDefaultOkResult()
    {
        var result = Result<int>.Success(123);

        var actionResult = result.ToActionResult<int, int>(null);

        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(123, ok.Value);
    }

    [Fact]
    public void ToActionResultOfT_WithCallback_Failure_ReturnsBadRequest()
    {
        var result = Result<int>.Failure(
        [
            new ValidationError(ValidationPath.Parse("$.value"), "invalid", "bad")
        ]);

        var actionResult = result.ToActionResult<int, string>(value => $"value-{value}");

        var badRequest = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        Assert.IsType<ValidationProblemDetails>(badRequest.Value);
    }

    private record TestContext(bool Flag);

    [Fact]
    public void ToActionResultOfTContext_WithCallback_Success_InvokesCallback()
    {
        var result = Result<int, TestContext>.Success(123, new TestContext(true));

        var actionResult = result.ToActionResult<int, TestContext, string>(value => $"value-{value}");

        Assert.Equal("value-123", actionResult.Value);
    }

    [Fact]
    public void ToActionResultOfTContext_WithCallback_Failure_ReturnsBadRequest()
    {
        var result = Result<int, TestContext>.Failure(
        [
            new ValidationError(ValidationPath.Parse("$.value"), "invalid", "bad")
        ]);

        var actionResult = result.ToActionResult<int, TestContext, string>(value => $"value-{value}");

        var badRequest = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        Assert.IsType<ValidationProblemDetails>(badRequest.Value);
    }

    [Fact]
    public void ToResult_WithIResultCallback_Success_InvokesCallback()
    {
        var result = Result<int>.Success(123);

        var httpResult = result.ToResult(value => Microsoft.AspNetCore.Http.Results.Created($"/items/{value}", value));

        Assert.NotNull(httpResult);
    }

    [Fact]
    public void ToResult_WithIResultCallback_Failure_ReturnsValidationProblem()
    {
        var result = Result<int>.Failure(
        [
            new ValidationError(ValidationPath.Parse("$.value"), "invalid", "bad")
        ]);

        var httpResult = result.ToResult(value => Microsoft.AspNetCore.Http.Results.Created($"/items/{value}", value));

        Assert.NotNull(httpResult);
    }
}
