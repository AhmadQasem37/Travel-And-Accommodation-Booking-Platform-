using TAABP.Application.DTOs.Auth;
using TAABP.Domain.Entities;

namespace TAABP.Application.Interfaces.Services;

public interface ITokenProvider
{
    Task<TokenDto> GenerateTokenAsync(User user, CancellationToken cancellationToken = default);
}
