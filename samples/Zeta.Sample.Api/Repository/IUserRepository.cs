public interface IUserRepository
{
    Task<bool> EmailExistsAsync(string email);
    Task<bool> IsMaintenanceModeAsync();
}