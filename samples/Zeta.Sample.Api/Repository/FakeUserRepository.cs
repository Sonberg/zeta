public class FakeUserRepository : IUserRepository
{
    public Task<bool> EmailExistsAsync(string email) => Task.FromResult(email == "taken@example.com");
    public Task<bool> IsMaintenanceModeAsync() => Task.FromResult(false);
}