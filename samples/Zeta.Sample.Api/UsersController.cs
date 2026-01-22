using Microsoft.AspNetCore.Mvc;

namespace Zeta.Sample.Api;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpPost]
    public IActionResult Create(User user)
    {
        return Ok(new { Message = "User created", User = user });
    }
}
