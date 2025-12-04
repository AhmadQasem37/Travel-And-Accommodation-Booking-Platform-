using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using TAABP.Application.Common;
using TAABP.Application.Common.Errors;
using TAABP.Application.Features.Auth.Commands.Register;
using TAABP.Application.Interfaces;
using TAABP.Application.Interfaces.Repositories;
using TAABP.Application.Interfaces.Services;
using TAABP.Domain.Entities;

namespace TAABP.Application.Tests.Unit.Features.Auth.Commands;

/// <summary>
/// Unit tests for RegisterCommandHandler using Moq for all dependencies
/// Tests registration logic in isolation
/// </summary>
public class RegisterCommandHandlerTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<ISaltGenerator> _mockSaltGenerator;
    private readonly ILogger<RegisterCommandHandler> _logger;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockSaltGenerator = new Mock<ISaltGenerator>();
        _logger = LoggerFactory.Create(b => b.AddConsole())
            .CreateLogger<RegisterCommandHandler>();

        _handler = new RegisterCommandHandler(
            _mockUserRepository.Object,
            _mockUnitOfWork.Object,
            _mockPasswordHasher.Object,
            _mockSaltGenerator.Object,
            _logger);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidCommand_ReturnsSuccess()
    {
        // Arrange
        var command = new RegisterCommand("testuser", "test@test.com", "Pass123", "John", "Doe", "+1234567890");

        _mockUserRepository
            .Setup(r => r.ExistsByEmailAsync("test@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockSaltGenerator
            .Setup(s => s.GenerateSaltAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("salt123");

        _mockPasswordHasher
            .Setup(p => p.HashPasswordAsync("Pass123", "salt123", It.IsAny<CancellationToken>()))
            .ReturnsAsync("hash123");

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().Be(Error.None);

        _mockUserRepository.Verify(r => r.Add(It.IsAny<User>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("user1@test.com", "user1")]
    [InlineData("user2@test.com", "user2")]
    [InlineData("user3@test.com", "user3")]
    public async Task Handle_WithDifferentEmails_ReturnsSuccess(string email, string username)
    {
        // Arrange
        var command = new RegisterCommand(username, email, "Pass123", "John", "Doe", null);

        _mockUserRepository
            .Setup(r => r.ExistsByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockSaltGenerator
            .Setup(s => s.GenerateSaltAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("salt");

        _mockPasswordHasher
            .Setup(p => p.HashPasswordAsync("Pass123", "salt", It.IsAny<CancellationToken>()))
            .ReturnsAsync("hash");

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Failure Cases - Duplicate Email

    [Fact]
    public async Task Handle_WithExistingEmail_ReturnsEmailAlreadyExistsError()
    {
        // Arrange
        var command = new RegisterCommand("testuser", "duplicate@test.com", "Pass123", "John", "Doe", null);

        _mockUserRepository
            .Setup(r => r.ExistsByEmailAsync("duplicate@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true); // Email already exists

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.EmailAlreadyExists");
        result.Error.Description.Should().Contain("duplicate@test.com");

        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockUserRepository.Verify(r => r.Add(It.IsAny<User>()), Times.Never);
    }

    #endregion

    #region Edge Cases

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_WithEmptyEmail_AllowsItToPassHandler(string email)
    {
        // Arrange
        // Note: Email validation is delegated to RegisterCommandValidator, not the handler
        // The handler will attempt to process empty emails (should be caught by validator before reaching handler)
        var command = new RegisterCommand("testuser", email, "Pass123", "John", "Doe", null);

        _mockUserRepository
            .Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockSaltGenerator
            .Setup(s => s.GenerateSaltAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("salt");

        _mockPasswordHasher
            .Setup(p => p.HashPasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("hash");

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act - Handler doesn't validate, just processes
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - Handler accepts it (validation should happen at validator level)
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_VerifiesSaltGenerationCalledOnce()
    {
        // Arrange
        var command = new RegisterCommand("testuser", "test@test.com", "Pass123", "John", "Doe", null);

        _mockUserRepository
            .Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockSaltGenerator
            .Setup(s => s.GenerateSaltAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("salt123");

        _mockPasswordHasher
            .Setup(p => p.HashPasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("hash123");

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockSaltGenerator.Verify(s => s.GenerateSaltAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_VerifiesPasswordHashedWithCorrectSalt()
    {
        // Arrange
        var command = new RegisterCommand("testuser", "test@test.com", "Pass123", "John", "Doe", null);
        var generatedSalt = "generated_salt_123";

        _mockUserRepository
            .Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockSaltGenerator
            .Setup(s => s.GenerateSaltAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(generatedSalt);

        _mockPasswordHasher
            .Setup(p => p.HashPasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("hash123");

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockPasswordHasher.Verify(
            p => p.HashPasswordAsync("Pass123", generatedSalt, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion
}
