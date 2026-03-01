namespace Zeta.Sample.FastEndpoints.Api.Repository;

public interface IUserRepository
{
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
}
