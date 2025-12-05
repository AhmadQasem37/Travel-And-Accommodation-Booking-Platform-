using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TAABP.Application.Tests.Integration.Fixtures;
using TAABP.Domain.Entities;
using TAABP.Infrastructure.Persistence.Context;
using TAABP.Infrastructure.Persistence.Repositories;

namespace TAABP.Application.Tests.Integration.Features.Amenities.Queries;

public class GetAllAmenitiesIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public GetAllAmenitiesIntegrationTests(DatabaseFixture fixture)
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
    public async Task GetAllAsync_WithNoAmenities_ReturnsEmptyList()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var repository = new AmenityRepository(context);

        // Act
        var result = await repository.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_WithSingleAmenity_ReturnsSingleAmenity()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedSingleAmenityAsync(context);
        var repository = new AmenityRepository(context);

        // Act
        var result = await repository.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().ContainSingle();
        result[0].Name.Should().Be("Free WiFi");
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleAmenities_ReturnsAllAmenities()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedMultipleAmenitiesAsync(context);
        var repository = new AmenityRepository(context);

        // Act
        var result = await repository.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(5);
    }

    #endregion

    #region Ordering Tests

    [Fact]
    public async Task GetAllAsync_ReturnsAmenitiesOrderedByName()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedUnorderedAmenitiesAsync(context);
        var repository = new AmenityRepository(context);

        // Act
        var result = await repository.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().BeInAscendingOrder(a => a.Name);
    }

    [Fact]
    public async Task GetAllAsync_WithAlphabeticalAmenities_ReturnsCorrectOrder()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var amenities = new[]
        {
            new Amenity { Id = Guid.NewGuid(), Name = "Zumba Classes" },
            new Amenity { Id = Guid.NewGuid(), Name = "Airport Shuttle" },
            new Amenity { Id = Guid.NewGuid(), Name = "Meeting Rooms" }
        };
        context.Amenities.AddRange(amenities);
        await context.SaveChangesAsync();
        var repository = new AmenityRepository(context);

        // Act
        var result = await repository.GetAllAsync(CancellationToken.None);

        // Assert
        result[0].Name.Should().Be("Airport Shuttle");
        result[1].Name.Should().Be("Meeting Rooms");
        result[2].Name.Should().Be("Zumba Classes");
    }

    #endregion

    #region Data Integrity Tests

    [Fact]
    public async Task GetAllAsync_ReturnsCorrectAmenityIds()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var amenityId = Guid.NewGuid();
        var amenity = new Amenity { Id = amenityId, Name = "Test Amenity" };
        context.Amenities.Add(amenity);
        await context.SaveChangesAsync();
        var repository = new AmenityRepository(context);

        // Act
        var result = await repository.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().ContainSingle();
        result[0].Id.Should().Be(amenityId);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsCorrectAmenityNames()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var amenity = new Amenity { Id = Guid.NewGuid(), Name = "Swimming Pool" };
        context.Amenities.Add(amenity);
        await context.SaveChangesAsync();
        var repository = new AmenityRepository(context);

        // Act
        var result = await repository.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().ContainSingle();
        result[0].Name.Should().Be("Swimming Pool");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAmenitiesWithDescriptions()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var amenity = new Amenity
        {
            Id = Guid.NewGuid(),
            Name = "Gym",
            Description = "24/7 fitness center"
        };
        context.Amenities.Add(amenity);
        await context.SaveChangesAsync();
        var repository = new AmenityRepository(context);

        // Act
        var result = await repository.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().ContainSingle();
        result[0].Description.Should().Be("24/7 fitness center");
    }

    #endregion

    #region Large Dataset Tests

    [Fact]
    public async Task GetAllAsync_WithManyAmenities_ReturnsAllAmenities()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedManyAmenitiesAsync(context, 50);
        var repository = new AmenityRepository(context);

        // Act
        var result = await repository.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(50);
    }

    [Fact]
    public async Task GetAllAsync_WithManyAmenities_MaintainsOrderByName()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedManyAmenitiesAsync(context, 20);
        var repository = new AmenityRepository(context);

        // Act
        var result = await repository.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().BeInAscendingOrder(a => a.Name);
    }

    #endregion

    #region AsNoTracking Tests

    [Fact]
    public async Task GetAllAsync_ReturnsUnTrackedEntities()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedSingleAmenityAsync(context);
        var repository = new AmenityRepository(context);

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
        await SeedSingleAmenityAsync(context);
        var repository = new AmenityRepository(context);
        var cts = new CancellationTokenSource();

        // Act
        var result = await repository.GetAllAsync(cts.Token);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Common Hotel Amenities Tests

    [Theory]
    [InlineData("Free WiFi")]
    [InlineData("Swimming Pool")]
    [InlineData("Gym")]
    [InlineData("Spa")]
    [InlineData("Restaurant")]
    [InlineData("Room Service")]
    [InlineData("Parking")]
    [InlineData("Business Center")]
    public async Task GetAllAsync_WithCommonAmenity_ReturnsCorrectAmenity(string amenityName)
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var amenity = new Amenity { Id = Guid.NewGuid(), Name = amenityName };
        context.Amenities.Add(amenity);
        await context.SaveChangesAsync();
        var repository = new AmenityRepository(context);

        // Act
        var result = await repository.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().ContainSingle(a => a.Name == amenityName);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task GetAllAsync_WithSpecialCharactersInName_ReturnsCorrectly()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var amenity = new Amenity { Id = Guid.NewGuid(), Name = "24/7 Room Service" };
        context.Amenities.Add(amenity);
        await context.SaveChangesAsync();
        var repository = new AmenityRepository(context);

        // Act
        var result = await repository.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().ContainSingle(a => a.Name == "24/7 Room Service");
    }

    [Fact]
    public async Task GetAllAsync_WithUnicodeInName_ReturnsCorrectly()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var amenity = new Amenity { Id = Guid.NewGuid(), Name = "Café & Lounge" };
        context.Amenities.Add(amenity);
        await context.SaveChangesAsync();
        var repository = new AmenityRepository(context);

        // Act
        var result = await repository.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().ContainSingle(a => a.Name == "Café & Lounge");
    }

    #endregion

    #region Helper Methods

    private static async Task SeedSingleAmenityAsync(ApplicationDbContext context)
    {
        var amenity = new Amenity
        {
            Id = Guid.NewGuid(),
            Name = "Free WiFi",
            Description = "High-speed internet access"
        };
        context.Amenities.Add(amenity);
        await context.SaveChangesAsync();
    }

    private static async Task SeedMultipleAmenitiesAsync(ApplicationDbContext context)
    {
        var amenities = new[]
        {
            new Amenity { Id = Guid.NewGuid(), Name = "Free WiFi" },
            new Amenity { Id = Guid.NewGuid(), Name = "Swimming Pool" },
            new Amenity { Id = Guid.NewGuid(), Name = "Gym" },
            new Amenity { Id = Guid.NewGuid(), Name = "Spa" },
            new Amenity { Id = Guid.NewGuid(), Name = "Restaurant" }
        };
        context.Amenities.AddRange(amenities);
        await context.SaveChangesAsync();
    }

    private static async Task SeedUnorderedAmenitiesAsync(ApplicationDbContext context)
    {
        var amenities = new[]
        {
            new Amenity { Id = Guid.NewGuid(), Name = "Spa" },
            new Amenity { Id = Guid.NewGuid(), Name = "Airport Shuttle" },
            new Amenity { Id = Guid.NewGuid(), Name = "Restaurant" },
            new Amenity { Id = Guid.NewGuid(), Name = "Business Center" },
            new Amenity { Id = Guid.NewGuid(), Name = "Free WiFi" }
        };
        context.Amenities.AddRange(amenities);
        await context.SaveChangesAsync();
    }

    private static async Task SeedManyAmenitiesAsync(ApplicationDbContext context, int count)
    {
        var amenities = Enumerable.Range(1, count)
            .Select(i => new Amenity
            {
                Id = Guid.NewGuid(),
                Name = $"Amenity {i:D3}"
            })
            .ToArray();
        context.Amenities.AddRange(amenities);
        await context.SaveChangesAsync();
    }

    #endregion
}
