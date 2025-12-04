using MediatR;
using Microsoft.Extensions.Logging;
using TAABP.Application.Common;
using TAABP.Application.Common.Errors;
using TAABP.Application.DTOs.Auth;
using TAABP.Application.Interfaces.Repositories;
using TAABP.Application.Interfaces.Services;

namespace TAABP.Application.Features.Auth.Commands.Login;

public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ITokenProvider tokenProvider,
    ILogger<LoginCommandHandler> logger) : IRequestHandler<LoginCommand, Result<TokenDto>>
{
    public async Task<Result<TokenDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Login attempt for email {Email}", request.Email);

        var (salt, hash, user) = await userRepository.GetCredentialsByEmailAsync(request.Email, cancellationToken);

        if (user is null || salt is null || hash is null)
        {
            logger.LogWarning("Login failed: User with email {Email} not found", request.Email);
            return Result.Failure<TokenDto>(UserErrors.InvalidCredentials);
        }

        var isValidPassword = await passwordHasher.VerifyPasswordAsync(
            request.Password,
            salt,
            hash,
            cancellationToken);

        if (!isValidPassword)
        {
            logger.LogWarning("Login failed: Invalid password for email {Email}", request.Email);
            return Result.Failure<TokenDto>(UserErrors.InvalidCredentials);
        }

        var token = await tokenProvider.GenerateTokenAsync(user, cancellationToken);

        logger.LogInformation("User with email {Email} logged in successfully", request.Email);

        return Result.Success(token);
    }
}
