using System.Security.Cryptography;
using TAABP.Application.Interfaces.Services;

namespace TAABP.Infrastructure.Services;

public sealed class SaltGenerator : ISaltGenerator
{
    private const int SaltSize = 16;

    public Task<string> GenerateSaltAsync(CancellationToken cancellationToken = default)
    {
        var saltBytes = RandomNumberGenerator.GetBytes(SaltSize);
        return Task.FromResult(Convert.ToBase64String(saltBytes));
    }
}
