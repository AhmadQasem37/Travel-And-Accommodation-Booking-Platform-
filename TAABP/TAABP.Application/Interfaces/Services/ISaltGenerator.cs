namespace TAABP.Application.Interfaces.Services;

public interface ISaltGenerator
{
    Task<string> GenerateSaltAsync(CancellationToken cancellationToken = default);
}
