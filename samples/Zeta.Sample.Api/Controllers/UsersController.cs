using Microsoft.AspNetCore.Mvc;
using Zeta.AspNetCore;
using Zeta.Sample.Api.Models;
using ValidationSchemas = Zeta.Sample.Api.Validation.Schemas;

namespace Zeta.Sample.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IZetaValidator _validator;

    public UsersController(IZetaValidator validator) => _validator = validator;

    /// <summary>
    /// Register user with context-aware validation (async email uniqueness).
    /// Uses IZetaValidator which automatically resolves the context factory from DI.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserRequest request, CancellationToken ct)
    {
        var result = await _validator.ValidateAsync(request, ValidationSchemas.RegisterUser, ct);

        return result.ToActionResult(valid => Ok(new
        {
            Message = "User registered successfully",
            Email = valid.Email,
            Name = valid.Name
        }));
    }

    /// <summary>
    /// Register user with simple validation (no async context).
    /// </summary>
    [HttpPost("register/simple")]
    public async Task<IActionResult> RegisterSimple(RegisterUserRequest request, CancellationToken ct)
    {
        var result = await _validator.ValidateAsync(request, ValidationSchemas.RegisterUserSimple, ct);

        return result.ToActionResult(valid => Ok(new
        {
            Message = "User registered (simple validation)",
            Email = valid.Email,
            Name = valid.Name
        }));
    }

    /// <summary>
    /// Create user with conditional address validation.
    /// Address is only validated when HasAddress is true.
    /// </summary>
    [HttpPost("create")]
    public async Task<IActionResult> Create(CreateUserRequest request, CancellationToken ct)
    {
        var result = await _validator.ValidateAsync(request, ValidationSchemas.CreateUser, ct);

        return result.ToActionResult(valid => CreatedAtAction(
            nameof(Create),
            new
            {
                Message = "User created",
                User = valid
            }));
    }

    /// <summary>
    /// Update user profile with optional field validation.
    /// </summary>
    [HttpPatch("profile")]
    public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request, CancellationToken ct)
    {
        var result = await _validator.ValidateAsync(request, ValidationSchemas.UpdateProfile, ct);

        return result.ToActionResult(valid => Ok(new
        {
            Message = "Profile updated",
            Profile = valid
        }));
    }
}
