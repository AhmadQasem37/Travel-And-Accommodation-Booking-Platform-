using MediatR;
using TAABP.Application.Common;

namespace TAABP.Application.Features.Auth.Commands.Register;

public sealed record RegisterCommand(
    string Username,
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber
) : IRequest<Result>;
