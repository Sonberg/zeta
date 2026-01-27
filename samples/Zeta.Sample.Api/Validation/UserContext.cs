using Zeta.Sample.Api.Models;
using Zeta.Sample.Api.Repository;

namespace Zeta.Sample.Api.Validation;

// Context for user registration - loaded async before validation
public record RegisterUserContext(bool EmailExists);

public class RegisterUserContextFactory : IValidationContextFactory<RegisterUserRequest, RegisterUserContext>
{
    private readonly IUserRepository _repo;

    public RegisterUserContextFactory(IUserRepository repo) => _repo = repo;

    public async Task<RegisterUserContext> CreateAsync(
        RegisterUserRequest input,
        CancellationToken ct)
    {
        var emailExists = await _repo.EmailExistsAsync(input.Email, ct);
        return new RegisterUserContext(emailExists);
    }
}
