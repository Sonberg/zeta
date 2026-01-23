using Zeta;

public record UserContext(bool EmailExists, bool IsMaintenanceMode);

public class UserContextFactory : IValidationContextFactory<User, UserContext>
{
    private readonly IUserRepository _repo;
    public UserContextFactory(IUserRepository repo) => _repo = repo;

    public async Task<UserContext> CreateAsync(User input, IServiceProvider services, CancellationToken ct)
    {
        return new UserContext(
            EmailExists: await _repo.EmailExistsAsync(input.Email),
            IsMaintenanceMode: await _repo.IsMaintenanceModeAsync()
        );
    }
}