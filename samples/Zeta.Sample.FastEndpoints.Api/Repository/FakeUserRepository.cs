namespace Zeta.Sample.FastEndpoints.Api.Repository;

public class FakeUserRepository : IUserRepository
{
    // Simulate a pre-existing email for testing context-aware validation
    private static readonly HashSet<string> _existingEmails =
    [
        "taken@example.com",
        "duplicate@example.com"
    ];

    public Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
        => Task.FromResult(_existingEmails.Contains(email.ToLowerInvariant()));
}
