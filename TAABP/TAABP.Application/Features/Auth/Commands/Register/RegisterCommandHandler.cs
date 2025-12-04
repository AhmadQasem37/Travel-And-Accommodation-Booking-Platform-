using MediatR;
using Microsoft.Extensions.Logging;
using TAABP.Application.Common;
using TAABP.Application.Common.Errors;
using TAABP.Application.Interfaces;
using TAABP.Application.Interfaces.Repositories;
using TAABP.Application.Interfaces.Services;
using TAABP.Domain.Entities;

namespace TAABP.Application.Features.Auth.Commands.Register;

public sealed class RegisterCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    ISaltGenerator saltGenerator,
    ILogger<RegisterCommandHandler> logger) : IRequestHandler<RegisterCommand, Result>
{
    public async Task<Result> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Registering new user with username {Username}", request.Username);

        if (await userRepository.ExistsByEmailAsync(request.Email, cancellationToken))
        {
            logger.LogWarning("Registration failed: Email {Email} already exists", request.Email);
            return Result.Failure(UserErrors.EmailAlreadyExists(request.Email));
        }

        var salt = await saltGenerator.GenerateSaltAsync(cancellationToken);
        var passwordHash = await passwordHasher.HashPasswordAsync(request.Password, salt, cancellationToken);

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHash,
            PasswordSalt = salt,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber
        };

        userRepository.Add(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully registered user {UserId} with username {Username}",
            user.Id, user.Username);

        return Result.Success();
    }
}
