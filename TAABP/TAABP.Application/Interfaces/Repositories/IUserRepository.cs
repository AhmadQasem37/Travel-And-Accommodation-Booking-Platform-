using TAABP.Domain.Entities;

namespace TAABP.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for User aggregate root.
/// Methods will be added as features require them.
/// </summary>
public interface IUserRepository
{
    // For Login - get credentials (salt, hash) for verification
    Task<(string? PasswordSalt, string? PasswordHash, User? User)> GetCredentialsByEmailAsync(
        string email, CancellationToken cancellationToken = default);

    // Existence checks
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    // Write operations
    void Add(User entity);
    void Update(User entity);
    void Remove(User entity);
}
