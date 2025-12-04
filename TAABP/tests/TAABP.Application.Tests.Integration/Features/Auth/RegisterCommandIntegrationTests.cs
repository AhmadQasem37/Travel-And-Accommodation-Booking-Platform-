using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TAABP.Application.Common;
using TAABP.Application.Common.Errors;
using TAABP.Application.Features.Auth.Commands.Register;
using TAABP.Application.Interfaces;
using TAABP.Infrastructure.Persistence.Repositories;
using TAABP.Infrastructure.Services;
using TAABP.Application.Tests.Integration.Fixtures;
using Microsoft.Extensions.Logging;
using TAABP.Domain.Entities;

namespace TAABP.Application.Tests.Integration.Features.Auth;

/// <summary>
/// Integration tests for RegisterCommandHandler
/// Tests the full flow: Handler -> Repository -> In-Memory Database
/// Uses DatabaseFixture for setup and teardown
/// </summary>
public class RegisterCommandIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _logger = LoggerFactory.Create(b => b.AddConsole())
            .CreateLogger<RegisterCommandHandler>();
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidCommand_CreatesUserInDatabase()
    {
        // Arrange
        var command = new RegisterCommand(
            "testuser",
            "test@test.com",
            "Password123",
            "John",
            "Doe",
            "+1234567890");

        // Real implementations
        var userRepository = new UserRepository(_fixture.Context);
        var unitOfWork = new UnitOfWork(_fixture.Context);
        var passwordHasher = new PasswordHasher();
        var saltGenerator = new SaltGenerator();

        var handler = new RegisterCommandHandler(
            userRepository,
            unitOfWork,
            passwordHasher,
            saltGenerator,
            _logger);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert - Handler result
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().Be(Error.None);

        // Assert - Database persistence
        var savedUser = await _fixture.Context.Users
            .FirstOrDefaultAsync(u => u.Email == "test@test.com");

        savedUser.Should().NotBeNull();
        savedUser!.Username.Should().Be("testuser");
        savedUser.Email.Should().Be("test@test.com");
        savedUser.FirstName.Should().Be("John");
        savedUser.LastName.Should().Be("Doe");
        savedUser.PhoneNumber.Should().Be("+1234567890");

        // Assert - Password hashed correctly
        savedUser.PasswordHash.Should().NotBeEmpty();
        savedUser.PasswordSalt.Should().NotBeEmpty();
        savedUser.PasswordHash.Should().NotBe("Password123");
    }

    [Theory]
    [InlineData("user1@test.com", "User One")]
    [InlineData("user2@test.com", "User Two")]
    [InlineData("user3@test.com", "User Three")]
    public async Task Handle_WithDifferentEmails_CreatesMultipleUsers(string email, string firstName)
    {
        // Arrange
        var command = new RegisterCommand(
            "testuser",
            email,
            "Password123",
            firstName,
            "TestLast",
            null);

        var userRepository = new UserRepository(_fixture.Context);
        var unitOfWork = new UnitOfWork(_fixture.Context);
        var passwordHasher = new PasswordHasher();
        var saltGenerator = new SaltGenerator();

        var handler = new RegisterCommandHandler(
            userRepository,
            unitOfWork,
            passwordHasher,
            saltGenerator,
            _logger);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var savedUser = await _fixture.Context.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        savedUser.Should().NotBeNull();
        savedUser!.FirstName.Should().Be(firstName);
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WithDuplicateEmail_ReturnsEmailAlreadyExistsError()
    {
        // Arrange - Create first user
        var existingUser = new User
        {
            Username = "existinguser",
            Email = "duplicate@test.com",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            FirstName = "Existing",
            LastName = "User"
        };
        _fixture.Context.Users.Add(existingUser);
        await _fixture.Context.SaveChangesAsync();

        // Arrange - Try to register with same email
        var command = new RegisterCommand(
            "newuser",
            "duplicate@test.com",
            "Password123",
            "New",
            "User",
            null);

        var userRepository = new UserRepository(_fixture.Context);
        var unitOfWork = new UnitOfWork(_fixture.Context);
        var passwordHasher = new PasswordHasher();
        var saltGenerator = new SaltGenerator();

        var handler = new RegisterCommandHandler(
            userRepository,
            unitOfWork,
            passwordHasher,
            saltGenerator,
            _logger);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.EmailAlreadyExists");
        result.Error.Description.Should().Contain("duplicate@test.com");

        // Verify only one user with that email exists
        var userCount = await _fixture.Context.Users
            .CountAsync(u => u.Email == "duplicate@test.com");

        userCount.Should().Be(1);
    }

    #endregion

    #region Transaction Tests

    [Fact]
    public async Task Handle_WithValidCommand_CanRetrieveUserImmediately()
    {
        // Arrange
        var command = new RegisterCommand(
            "retrievetest",
            "retrieve@test.com",
            "Password123",
            "Retrieve",
            "Test",
            null);

        var userRepository = new UserRepository(_fixture.Context);
        var unitOfWork = new UnitOfWork(_fixture.Context);
        var passwordHasher = new PasswordHasher();
        var saltGenerator = new SaltGenerator();

        var handler = new RegisterCommandHandler(
            userRepository,
            unitOfWork,
            passwordHasher,
            saltGenerator,
            _logger);

        // Act
        var registerResult = await handler.Handle(command, CancellationToken.None);

        // Assert
        registerResult.IsSuccess.Should().BeTrue();

        // Immediately retrieve without new context - query directly from context
        var retrievedUser = _fixture.Context.Users.FirstOrDefault(u => u.Email == "retrieve@test.com");

        retrievedUser.Should().NotBeNull();
        retrievedUser!.Username.Should().Be("retrievetest");
    }

    #endregion
}

/// <summary>
/// Collection definition for sharing database fixture across multiple test classes
/// </summary>
[CollectionDefinition("Database collection")]
public class AuthIntegrationCollection : ICollectionFixture<DatabaseFixture>
{
    // This has no code, just defines the collection
}
