namespace Zeta.Sample.Blazor.Models;

public sealed class RegistrationRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Password { get; set; } = string.Empty;
}
