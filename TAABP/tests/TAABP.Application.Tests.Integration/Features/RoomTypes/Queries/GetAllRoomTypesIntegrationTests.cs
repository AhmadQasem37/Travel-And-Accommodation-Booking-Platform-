using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TAABP.Application.Tests.Integration.Fixtures;
using TAABP.Domain.Entities;
using TAABP.Infrastructure.Persistence.Context;
using TAABP.Infrastructure.Persistence.Repositories;

namespace TAABP.Application.Tests.Integration.Features.RoomTypes.Queries;

public class GetAllRoomTypesIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public GetAllRoomTypesIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task<ApplicationDbContext> CreateFreshContextAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("TestDb_" + Guid.NewGuid())
            .Options;

        var context = new ApplicationDbContext(options);
        await context.Database.EnsureCreatedAsync();
        return context;
    }

    #region Basic Tests

    [Fact]
    public async Task GetAllAsync_WithNoRoomTypes_ReturnsEmptyList()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var repository = new RoomTypeRepository(context);

        // Act
        var result = await repository.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_WithSingleRoomType_ReturnsSingleRoomType()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedSingleRoomTypeAsync(context);
        var repository = new RoomTypeRepository(context);

        // Act
        var result = await repository.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().ContainSingle();
        result[0].Name.Should().Be("Standard");
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleRoomTypes_ReturnsAllRoomTypes()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedMultipleRoomTypesAsync(context);
        var repository = new RoomTypeRepository(context);

        // Act
        var result = await repository.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(5);
    }

    #endregion

    #region Ordering Tests

    [Fact]
    public async Task GetAllAsync_ReturnsRoomTypesOrderedByName()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedUnorderedRoomTypesAsync(context);
        var repository = new RoomTypeRepository(context);

        // Act
        var result = await repository.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().BeInAscendingOrder(r => r.Name);
    }

    [Fact]
    public async Task GetAllAsync_WithAlphabeticalRoomTypes_ReturnsCorrectOrder()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var roomTypes = new[]
        {
            new RoomType { Id = Guid.NewGuid(), Name = "Suite" },
            new RoomType { Id = Guid.NewGuid(), Name = "Deluxe" },
            new RoomType { Id = Guid.NewGuid(), Name = "Standard" }
        };
        context.RoomTypes.AddRange(roomTypes);
        await context.SaveChangesAsync();
        var repository = new RoomTypeRepository(context);

        // Act
        var result = await repository.GetAllAsync(CancellationToken.None);

        // Assert
        result[0].Name.Should().Be("Deluxe");
        result[1].Name.Should().Be("Standard");
        result[2].Name.Should().Be("Suite");
    }

    #endregion

    #region Data Integrity Tests

    [Fact]
    public async Task GetAllAsync_ReturnsCorrectRoomTypeIds()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var roomTypeId = Guid.NewGuid();
        var roomType = new RoomType { Id = roomTypeId, Name = "Test Room Type" };
        context.RoomTypes.Add(roomType);
        await context.SaveChangesAsync();
        var repository = new RoomTypeRepository(context);

        // Act
        var result = await repository.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().ContainSingle();
        result[0].Id.Should().Be(roomTypeId);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsCorrectRoomTypeNames()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var roomType = new RoomType { Id = Guid.NewGuid(), Name = "Deluxe" };
        context.RoomTypes.Add(roomType);
        await context.SaveChangesAsync();
        var repository = new RoomTypeRepository(context);

        // Act
        var result = await repository.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().ContainSingle();
        result[0].Name.Should().Be("Deluxe");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsRoomTypesWithDescriptions()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var roomType = new RoomType
        {
            Id = Guid.NewGuid(),
            Name = "Suite",
            Description = "Luxurious suite with premium amenities"
        };
        context.RoomTypes.Add(roomType);
        await context.SaveChangesAsync();
        var repository = new RoomTypeRepository(context);

        // Act
        var result = await repository.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().ContainSingle();
        result[0].Description.Should().Be("Luxurious suite with premium amenities");
    }

    #endregion

    #region Large Dataset Tests

    [Fact]
    public async Task GetAllAsync_WithManyRoomTypes_ReturnsAllRoomTypes()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedManyRoomTypesAsync(context, 50);
        var repository = new RoomTypeRepository(context);

        // Act
        var result = await repository.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(50);
    }

    [Fact]
    public async Task GetAllAsync_WithManyRoomTypes_MaintainsOrderByName()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedManyRoomTypesAsync(context, 20);
        var repository = new RoomTypeRepository(context);

        // Act
        var result = await repository.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().BeInAscendingOrder(r => r.Name);
    }

    #endregion

    #region AsNoTracking Tests

    [Fact]
    public async Task GetAllAsync_ReturnsUnTrackedEntities()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedSingleRoomTypeAsync(context);
        var repository = new RoomTypeRepository(context);

        // Act
        var result = await repository.GetAllAsync(CancellationToken.None);

        // Assert
        var entry = context.Entry(result[0]);
        entry.State.Should().Be(EntityState.Detached);
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task GetAllAsync_WithValidCancellationToken_Completes()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedSingleRoomTypeAsync(context);
        var repository = new RoomTypeRepository(context);
        var cts = new CancellationTokenSource();

        // Act
        var result = await repository.GetAllAsync(cts.Token);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Common Room Types Tests

    [Theory]
    [InlineData("Standard")]
    [InlineData("Deluxe")]
    [InlineData("Suite")]
    [InlineData("Executive Suite")]
    [InlineData("Presidential Suite")]
    [InlineData("Family Room")]
    [InlineData("Twin Room")]
    [InlineData("King Room")]
    public async Task GetAllAsync_WithCommonRoomType_ReturnsCorrectRoomType(string roomTypeName)
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var roomType = new RoomType { Id = Guid.NewGuid(), Name = roomTypeName };
        context.RoomTypes.Add(roomType);
        await context.SaveChangesAsync();
        var repository = new RoomTypeRepository(context);

        // Act
        var result = await repository.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().ContainSingle(r => r.Name == roomTypeName);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task GetAllAsync_WithSpecialCharactersInName_ReturnsCorrectly()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var roomType = new RoomType { Id = Guid.NewGuid(), Name = "King Room (Non-Smoking)" };
        context.RoomTypes.Add(roomType);
        await context.SaveChangesAsync();
        var repository = new RoomTypeRepository(context);

        // Act
        var result = await repository.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().ContainSingle(r => r.Name == "King Room (Non-Smoking)");
    }

    [Fact]
    public async Task GetAllAsync_WithUnicodeInName_ReturnsCorrectly()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var roomType = new RoomType { Id = Guid.NewGuid(), Name = "Deluxe — Ocean View" };
        context.RoomTypes.Add(roomType);
        await context.SaveChangesAsync();
        var repository = new RoomTypeRepository(context);

        // Act
        var result = await repository.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().ContainSingle(r => r.Name == "Deluxe — Ocean View");
    }

    #endregion

    #region Helper Methods

    private static async Task SeedSingleRoomTypeAsync(ApplicationDbContext context)
    {
        var roomType = new RoomType
        {
            Id = Guid.NewGuid(),
            Name = "Standard",
            Description = "Basic room with essential amenities"
        };
        context.RoomTypes.Add(roomType);
        await context.SaveChangesAsync();
    }

    private static async Task SeedMultipleRoomTypesAsync(ApplicationDbContext context)
    {
        var roomTypes = new[]
        {
            new RoomType { Id = Guid.NewGuid(), Name = "Standard" },
            new RoomType { Id = Guid.NewGuid(), Name = "Deluxe" },
            new RoomType { Id = Guid.NewGuid(), Name = "Suite" },
            new RoomType { Id = Guid.NewGuid(), Name = "Executive Suite" },
            new RoomType { Id = Guid.NewGuid(), Name = "Presidential Suite" }
        };
        context.RoomTypes.AddRange(roomTypes);
        await context.SaveChangesAsync();
    }

    private static async Task SeedUnorderedRoomTypesAsync(ApplicationDbContext context)
    {
        var roomTypes = new[]
        {
            new RoomType { Id = Guid.NewGuid(), Name = "Suite" },
            new RoomType { Id = Guid.NewGuid(), Name = "Deluxe" },
            new RoomType { Id = Guid.NewGuid(), Name = "Standard" },
            new RoomType { Id = Guid.NewGuid(), Name = "Executive Suite" },
            new RoomType { Id = Guid.NewGuid(), Name = "Family Room" }
        };
        context.RoomTypes.AddRange(roomTypes);
        await context.SaveChangesAsync();
    }

    private static async Task SeedManyRoomTypesAsync(ApplicationDbContext context, int count)
    {
        var roomTypes = Enumerable.Range(1, count)
            .Select(i => new RoomType
            {
                Id = Guid.NewGuid(),
                Name = $"RoomType {i:D3}"
            })
            .ToArray();
        context.RoomTypes.AddRange(roomTypes);
        await context.SaveChangesAsync();
    }

    #endregion
}
