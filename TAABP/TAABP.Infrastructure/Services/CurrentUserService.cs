using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TAABP.Application.Interfaces.Services;

namespace TAABP.Infrastructure.Services;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid? UserId
    {
        get
        {
            var userId = httpContextAccessor.HttpContext?.User?.FindFirstValue(JwtRegisteredClaimNames.Sub);
            return string.IsNullOrEmpty(userId) ? null : Guid.Parse(userId);
        }
    }

    public string? Username => httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);

    public string? Role => httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);

    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
