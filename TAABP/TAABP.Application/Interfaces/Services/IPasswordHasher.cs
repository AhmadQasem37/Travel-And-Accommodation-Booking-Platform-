namespace TAABP.Application.Interfaces.Services;

public interface IPasswordHasher
{
    Task<string> HashPasswordAsync(string password, string salt, CancellationToken cancellationToken = default);
    Task<bool> VerifyPasswordAsync(string password, string salt, string hash, CancellationToken cancellationToken = default);
}
