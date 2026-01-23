using Microsoft.AspNetCore.Mvc;
using Zeta;
using Zeta.AspNetCore;
using Zeta.Sample.Api.Models;
using Zeta.Schemas;

namespace Zeta.Sample.Api;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IZetaValidator _validator;

    // Define schemas as static fields for reuse
    private static readonly ISchema<User> DefaultSchema = Z.Object<User>()
        .Field(u => u.Name, Z.String().MinLength(3))
        .Field(u => u.Email, Z.String().Email());

    private static readonly ISchema<User> StrictSchema = Z.Object<User>()
        .Field(u => u.Name, Z.String().MinLength(5)) // Stricter: 5 instead of 3
        .Field(u => u.Email, Z.String().Email());

    public UsersController(IZetaValidator validator)
    {
        _validator = validator;
    }

    [HttpPost]
    public async Task<IActionResult> Create(User user)
    {
        var result = await _validator.ValidateAsync(user, DefaultSchema);

        return result.ToActionResult(valid => Ok(new
        {
            Message = "User created",
            User = valid
        }));
    }

    /// <summary>
    /// Endpoint where validation is skipped.
    /// Even invalid data passes through.
    /// </summary>
    [HttpPost("ignored")]
    public IActionResult CreateIgnored(User user)
    {
        // No validation - just process the data
        return Ok(new
        {
            Message = "User created (validation ignored)",
            User = user
        });
    }

    /// <summary>
    /// Endpoint using stricter validation.
    /// StrictSchema requires name length >= 5 (stricter than default).
    /// </summary>
    [HttpPost("strict")]
    public async Task<IActionResult> CreateStrict(User user)
    {
        var result = await _validator.ValidateAsync(user, StrictSchema);

        return result.ToActionResult(valid => Ok(new
        {
            Message = "User created (strict validation)",
            User = valid
        }));
    }
}
