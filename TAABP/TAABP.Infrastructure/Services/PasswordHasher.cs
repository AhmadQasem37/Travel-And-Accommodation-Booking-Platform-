using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using TAABP.Application.Interfaces.Services;

namespace TAABP.Infrastructure.Services;

public sealed class PasswordHasher : IPasswordHasher
{
    private const int DegreeOfParallelism = 4;
    private const int MemorySize = 65536; // 64 MB
    private const int Iterations = 4;
    private const int HashLength = 32;

    public async Task<string> HashPasswordAsync(string password, string salt, CancellationToken cancellationToken = default)
    {
        var saltBytes = Convert.FromBase64String(salt);
        var passwordBytes = Encoding.UTF8.GetBytes(password);

        using var argon2 = new Argon2id(passwordBytes)
        {
            Salt = saltBytes,
            DegreeOfParallelism = DegreeOfParallelism,
            MemorySize = MemorySize,
            Iterations = Iterations
        };

        var hashBytes = await argon2.GetBytesAsync(HashLength);
        return Convert.ToBase64String(hashBytes);
    }

    public async Task<bool> VerifyPasswordAsync(string password, string salt, string hash, CancellationToken cancellationToken = default)
    {
        var newHash = await HashPasswordAsync(password, salt, cancellationToken);
        return CryptographicOperations.FixedTimeEquals(
            Convert.FromBase64String(newHash),
            Convert.FromBase64String(hash));
    }
}
