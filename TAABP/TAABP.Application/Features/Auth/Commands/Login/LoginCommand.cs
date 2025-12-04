using MediatR;
using TAABP.Application.Common;
using TAABP.Application.DTOs.Auth;

namespace TAABP.Application.Features.Auth.Commands.Login;

public sealed record LoginCommand(
    string Email,
    string Password
) : IRequest<Result<TokenDto>>;
