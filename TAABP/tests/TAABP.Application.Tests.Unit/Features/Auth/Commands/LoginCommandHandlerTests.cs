using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using TAABP.Application.Common;
using TAABP.Application.Common.Errors;
using TAABP.Application.DTOs.Auth;
using TAABP.Application.Features.Auth.Commands.Login;
using TAABP.Application.Interfaces.Repositories;
using TAABP.Application.Interfaces.Services;
using TAABP.Domain.Entities;
using TAABP.Domain.Enums;

namespace TAABP.Application.Tests.Unit.Features.Auth.Commands;

/// <summary>
/// Unit tests for LoginCommandHandler using Moq for all dependencies
/// Tests authentication logic in isolation
/// </summary>
public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<ITokenProvider> _mockTokenProvider;
    private readonly ILogger<LoginCommandHandler> _logger;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockTokenProvider = new Mock<ITokenProvider>();
        _logger = LoggerFactory.Create(b => b.AddConsole())
            .CreateLogger<LoginCommandHandler>();

        _handler = new LoginCommandHandler(
            _mockUserRepository.Object,
            _mockPasswordHasher.Object,
            _mockTokenProvider.Object,
            _logger);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidCredentials_ReturnsSuccessWithToken()
    {
        // Arrange
        var command = new LoginCommand("test@test.com", "Password123");
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@test.com",
            Username = "testuser",
            Role = UserRole.User,
            PasswordSalt = "salt123",
            PasswordHash = "hash123"
        };
        var expectedToken = "jwt_token_12345";

        _mockUserRepository
            .Setup(r => r.GetCredentialsByEmailAsync("test@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((user.PasswordSalt, user.PasswordHash, user));

        _mockPasswordHasher
            .Setup(p => p.VerifyPasswordAsync("Password123", "salt123", "hash123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockTokenProvider
            .Setup(t => t.GenerateTokenAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TokenDto(expectedToken, user.Role.ToString()));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Token.Should().Be(expectedToken);
        result.Value.Role.Should().Be("User");

        _mockUserRepository.Verify(
            r => r.GetCredentialsByEmailAsync("test@test.com", It.IsAny<CancellationToken>()),
            Times.Once);
        _mockPasswordHasher.Verify(
            p => p.VerifyPasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _mockTokenProvider.Verify(
            t => t.GenerateTokenAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData("admin@test.com", "AdminPass", "Admin")]
    [InlineData("user@test.com", "UserPass", "User")]
    public async Task Handle_WithDifferentRoles_ReturnsCorrectRole(string email, string password, string expectedRole)
    {
        // Arrange
        var command = new LoginCommand(email, password);
        var role = Enum.Parse<UserRole>(expectedRole);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            Username = "testuser",
            Role = role,
            PasswordSalt = "salt",
            PasswordHash = "hash"
        };

        _mockUserRepository
            .Setup(r => r.GetCredentialsByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((user.PasswordSalt, user.PasswordHash, user));

        _mockPasswordHasher
            .Setup(p => p.VerifyPasswordAsync(password, "salt", "hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockTokenProvider
            .Setup(t => t.GenerateTokenAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TokenDto("token", expectedRole));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Role.Should().Be(expectedRole);
    }

    #endregion

    #region Failure Cases - Invalid Email

    [Fact]
    public async Task Handle_WithNonExistentEmail_ReturnsInvalidCredentialsError()
    {
        // Arrange
        var command = new LoginCommand("nonexistent@test.com", "Password123");

        _mockUserRepository
            .Setup(r => r.GetCredentialsByEmailAsync("nonexistent@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((null, null, null)); // User not found

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.InvalidCredentials");
        result.Error.Description.Should().Contain("Invalid email or password");

        _mockPasswordHasher.Verify(
            p => p.VerifyPasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region Failure Cases - Invalid Password

    [Fact]
    public async Task Handle_WithInvalidPassword_ReturnsInvalidCredentialsError()
    {
        // Arrange
        var command = new LoginCommand("test@test.com", "WrongPassword");
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@test.com",
            PasswordSalt = "salt123",
            PasswordHash = "hash123"
        };

        _mockUserRepository
            .Setup(r => r.GetCredentialsByEmailAsync("test@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((user.PasswordSalt, user.PasswordHash, user));

        _mockPasswordHasher
            .Setup(p => p.VerifyPasswordAsync("WrongPassword", "salt123", "hash123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.InvalidCredentials");

        _mockTokenProvider.Verify(
            t => t.GenerateTokenAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region Edge Cases

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_WithEmptyEmail_ReturnsInvalidCredentialsError(string email)
    {
        // Arrange
        var command = new LoginCommand(email, "Password123");

        _mockUserRepository
            .Setup(r => r.GetCredentialsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((null, null, null));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.InvalidCredentials");
    }

    [Fact]
    public async Task Handle_WithEmptyPassword_VerifiesButFails()
    {
        // Arrange
        var command = new LoginCommand("test@test.com", "");
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@test.com",
            PasswordSalt = "salt",
            PasswordHash = "hash"
        };

        _mockUserRepository
            .Setup(r => r.GetCredentialsByEmailAsync("test@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((user.PasswordSalt, user.PasswordHash, user));

        _mockPasswordHasher
            .Setup(p => p.VerifyPasswordAsync("", "salt", "hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.InvalidCredentials");
    }

    #endregion
}
