namespace Zeta.Sample.Api.Repository;

public class FakeUserRepository : IUserRepository
{
    private static readonly HashSet<string> ExistingEmails = new(StringComparer.OrdinalIgnoreCase)
    {
        "taken@example.com",
        "admin@example.com",
        "test@example.com"
    };

    private static readonly HashSet<Guid> ExistingUsers = new()
    {
        Guid.Parse("11111111-1111-1111-1111-111111111111"),
        Guid.Parse("22222222-2222-2222-2222-222222222222")
    };

    public Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
        => Task.FromResult(ExistingEmails.Contains(email));

    public Task<bool> UserExistsAsync(Guid userId, CancellationToken ct = default)
        => Task.FromResult(ExistingUsers.Contains(userId));
}
