using Microsoft.EntityFrameworkCore;
using TAABP.Application.Interfaces.Repositories;
using TAABP.Domain.Entities;
using TAABP.Infrastructure.Persistence.Context;

namespace TAABP.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(ApplicationDbContext context) : IUserRepository
{
    public async Task<(string? PasswordSalt, string? PasswordHash, User? User)> GetCredentialsByEmailAsync(
        string email, CancellationToken cancellationToken = default)
    {
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        return user is null
            ? (null, null, null)
            : (user.PasswordSalt, user.PasswordHash, user);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .AnyAsync(u => u.Email == email, cancellationToken);
    }

    public void Add(User entity) => context.Users.Add(entity);

    public void Update(User entity) => context.Users.Update(entity);

    public void Remove(User entity) => context.Users.Remove(entity);
}
