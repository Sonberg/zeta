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
}
