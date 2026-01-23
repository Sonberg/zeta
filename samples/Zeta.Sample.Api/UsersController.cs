using Microsoft.AspNetCore.Mvc;
using Zeta;
using Zeta.AspNetCore;
using Zeta.Schemas;

namespace Zeta.Sample.Api;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpPost]
    public IActionResult Create(User user)
    {
        return Ok(new
        {
            Message = "User created",
            User = user
        });
    }

    /// <summary>
    /// Endpoint where validation is skipped using [ZetaIgnore].
    /// Even invalid data should pass through.
    /// </summary>
    [HttpPost("ignored")]
    public IActionResult CreateIgnored([ZetaIgnore] User user)
    {
        return Ok(new
        {
            Message = "User created (validation ignored)",
            User = user
        });
    }

    /// <summary>
    /// Endpoint using a specific schema via [ZetaValidate].
    /// StrictUserSchema requires name length >= 5 (stricter than default).
    /// </summary>
    [HttpPost("strict")]
    public IActionResult CreateStrict([ZetaValidate(typeof(StrictUserSchema))] User user)
    {
        return Ok(new
        {
            Message = "User created (strict validation)",
            User = user
        });
    }
}

/// <summary>
/// A stricter schema for User - requires name >= 5 characters.
/// Implements ISchema&lt;User&gt; by delegating to an internal ObjectSchema.
/// </summary>
public class StrictUserSchema : ISchema<User>
{
    private readonly ISchema<User> _inner = Z.Object<User>()
        .Field(u => u.Name, Z.String().MinLength(5)) // Stricter: 5 instead of 3
        .Field(u => u.Email, Z.String().Email());

    public Task<Result<User>> ValidateAsync(User value, ValidationExecutionContext? execution = null)
        => _inner.ValidateAsync(value, execution);

    public Task<Result<User>> ValidateAsync(User value, ValidationContext<object?> context)
        => _inner.ValidateAsync(value, context);
}