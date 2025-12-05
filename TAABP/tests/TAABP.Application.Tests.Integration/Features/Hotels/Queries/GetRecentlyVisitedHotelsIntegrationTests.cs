using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TAABP.Application.Tests.Integration.Fixtures;
using TAABP.Domain.Entities;
using TAABP.Infrastructure.Persistence.Context;
using TAABP.Infrastructure.Persistence.Repositories;

namespace TAABP.Application.Tests.Integration.Features.Hotels.Queries;

public class GetRecentlyVisitedHotelsIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public GetRecentlyVisitedHotelsIntegrationTests(DatabaseFixture fixture)
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
    public async Task GetByUserIdAsync_WithNoVisits_ReturnsEmptyList()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var userId = Guid.NewGuid();
        var repository = new RecentlyVisitedHotelRepository(context);

        // Act
        var result = await repository.GetByUserIdAsync(userId, 5, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByUserIdAsync_WithVisits_ReturnsUserVisits()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var userId = Guid.NewGuid();
        await SeedRecentlyVisitedHotelsAsync(context, userId, 3);
        var repository = new RecentlyVisitedHotelRepository(context);

        // Act
        var result = await repository.GetByUserIdAsync(userId, 5, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetByUserIdAsync_WithOtherUserVisits_ReturnsOnlyUserVisits()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        await SeedRecentlyVisitedHotelsAsync(context, userId, 2);
        await SeedRecentlyVisitedHotelsAsync(context, otherUserId, 5);
        var repository = new RecentlyVisitedHotelRepository(context);

        // Act
        var result = await repository.GetByUserIdAsync(userId, 10, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Count/Limit Tests

    [Fact]
    public async Task GetByUserIdAsync_WithCountOf5_ReturnsMax5Visits()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var userId = Guid.NewGuid();
        await SeedRecentlyVisitedHotelsAsync(context, userId, 10);
        var repository = new RecentlyVisitedHotelRepository(context);

        // Act
        var result = await repository.GetByUserIdAsync(userId, 5, CancellationToken.None);

        // Assert
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetByUserIdAsync_WithCountOf3_ReturnsMax3Visits()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var userId = Guid.NewGuid();
        await SeedRecentlyVisitedHotelsAsync(context, userId, 10);
        var repository = new RecentlyVisitedHotelRepository(context);

        // Act
        var result = await repository.GetByUserIdAsync(userId, 3, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetByUserIdAsync_WhenFewerVisitsThanCount_ReturnsAllAvailable()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var userId = Guid.NewGuid();
        await SeedRecentlyVisitedHotelsAsync(context, userId, 3);
        var repository = new RecentlyVisitedHotelRepository(context);

        // Act
        var result = await repository.GetByUserIdAsync(userId, 10, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(20)]
    public async Task GetByUserIdAsync_WithVariousCounts_RespectsLimit(int count)
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var userId = Guid.NewGuid();
        await SeedRecentlyVisitedHotelsAsync(context, userId, 25);
        var repository = new RecentlyVisitedHotelRepository(context);

        // Act
        var result = await repository.GetByUserIdAsync(userId, count, CancellationToken.None);

        // Assert
        result.Should().HaveCount(count);
    }

    #endregion

    #region Sorting Tests

    [Fact]
    public async Task GetByUserIdAsync_ReturnsVisitsOrderedByVisitedAtDescending()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var userId = Guid.NewGuid();
        await SeedVisitsWithVaryingTimesAsync(context, userId);
        var repository = new RecentlyVisitedHotelRepository(context);

        // Act
        var result = await repository.GetByUserIdAsync(userId, 10, CancellationToken.None);

        // Assert
        result.Should().BeInDescendingOrder(v => v.VisitedAt);
    }

    [Fact]
    public async Task GetByUserIdAsync_MostRecentVisitFirst()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var userId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var city = await SeedCityAsync(context, "Dubai");
        var hotel1 = await SeedHotelAsync(context, city.Id, "Old Visit Hotel");
        var hotel2 = await SeedHotelAsync(context, city.Id, "Recent Visit Hotel");

        context.RecentlyVisitedHotels.Add(new RecentlyVisitedHotel
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            HotelId = hotel1.Id,
            VisitedAt = now.AddDays(-5) // Old visit
        });
        context.RecentlyVisitedHotels.Add(new RecentlyVisitedHotel
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            HotelId = hotel2.Id,
            VisitedAt = now // Recent visit
        });
        await context.SaveChangesAsync();

        var repository = new RecentlyVisitedHotelRepository(context);

        // Act
        var result = await repository.GetByUserIdAsync(userId, 10, CancellationToken.None);

        // Assert
        result.First().HotelName.Should().Be("Recent Visit Hotel");
    }

    #endregion

    #region Data Mapping Tests

    [Fact]
    public async Task GetByUserIdAsync_MapsHotelIdCorrectly()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var userId = Guid.NewGuid();
        var city = await SeedCityAsync(context, "Test City");
        var hotel = await SeedHotelAsync(context, city.Id, "Test Hotel");
        await SeedVisitAsync(context, userId, hotel.Id);
        var repository = new RecentlyVisitedHotelRepository(context);

        // Act
        var result = await repository.GetByUserIdAsync(userId, 5, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().HotelId.Should().Be(hotel.Id);
    }

    [Fact]
    public async Task GetByUserIdAsync_MapsHotelNameCorrectly()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var userId = Guid.NewGuid();
        var city = await SeedCityAsync(context, "Test City");
        var hotel = await SeedHotelAsync(context, city.Id, "Grand Palace Hotel");
        await SeedVisitAsync(context, userId, hotel.Id);
        var repository = new RecentlyVisitedHotelRepository(context);

        // Act
        var result = await repository.GetByUserIdAsync(userId, 5, CancellationToken.None);

        // Assert
        result.First().HotelName.Should().Be("Grand Palace Hotel");
    }

    [Fact]
    public async Task GetByUserIdAsync_MapsCityNameCorrectly()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var userId = Guid.NewGuid();
        var city = await SeedCityAsync(context, "Dubai");
        var hotel = await SeedHotelAsync(context, city.Id, "Test Hotel");
        await SeedVisitAsync(context, userId, hotel.Id);
        var repository = new RecentlyVisitedHotelRepository(context);

        // Act
        var result = await repository.GetByUserIdAsync(userId, 5, CancellationToken.None);

        // Assert
        result.First().CityName.Should().Be("Dubai");
    }

    [Fact]
    public async Task GetByUserIdAsync_MapsStarRatingCorrectly()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var userId = Guid.NewGuid();
        var city = await SeedCityAsync(context, "Test City");
        var hotel = await SeedHotelAsync(context, city.Id, "5 Star Hotel", starRating: 5);
        await SeedVisitAsync(context, userId, hotel.Id);
        var repository = new RecentlyVisitedHotelRepository(context);

        // Act
        var result = await repository.GetByUserIdAsync(userId, 5, CancellationToken.None);

        // Assert
        result.First().StarRating.Should().Be(5);
    }

    [Fact]
    public async Task GetByUserIdAsync_MapsThumbnailUrlCorrectly()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var userId = Guid.NewGuid();
        var city = await SeedCityAsync(context, "Test City");
        var hotel = await SeedHotelAsync(context, city.Id, "Test Hotel",
            thumbnailUrl: "https://example.com/thumb.jpg");
        await SeedVisitAsync(context, userId, hotel.Id);
        var repository = new RecentlyVisitedHotelRepository(context);

        // Act
        var result = await repository.GetByUserIdAsync(userId, 5, CancellationToken.None);

        // Assert
        result.First().ThumbnailUrl.Should().Be("https://example.com/thumb.jpg");
    }

    [Fact]
    public async Task GetByUserIdAsync_MapsNullThumbnailUrlCorrectly()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var userId = Guid.NewGuid();
        var city = await SeedCityAsync(context, "Test City");
        var hotel = await SeedHotelAsync(context, city.Id, "Test Hotel", thumbnailUrl: null);
        await SeedVisitAsync(context, userId, hotel.Id);
        var repository = new RecentlyVisitedHotelRepository(context);

        // Act
        var result = await repository.GetByUserIdAsync(userId, 5, CancellationToken.None);

        // Assert
        result.First().ThumbnailUrl.Should().BeNull();
    }

    [Fact]
    public async Task GetByUserIdAsync_MapsPriceCorrectly()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var userId = Guid.NewGuid();
        var city = await SeedCityAsync(context, "Test City");
        var hotel = await SeedHotelAsync(context, city.Id, "Test Hotel", minRoomPrice: 350.50m);
        await SeedVisitAsync(context, userId, hotel.Id);
        var repository = new RecentlyVisitedHotelRepository(context);

        // Act
        var result = await repository.GetByUserIdAsync(userId, 5, CancellationToken.None);

        // Assert
        result.First().PriceStartingFrom.Should().Be(350.50m);
    }

    [Fact]
    public async Task GetByUserIdAsync_MapsVisitedAtCorrectly()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var userId = Guid.NewGuid();
        var visitedAt = new DateTime(2024, 6, 15, 10, 30, 0, DateTimeKind.Utc);
        var city = await SeedCityAsync(context, "Test City");
        var hotel = await SeedHotelAsync(context, city.Id, "Test Hotel");

        context.RecentlyVisitedHotels.Add(new RecentlyVisitedHotel
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            HotelId = hotel.Id,
            VisitedAt = visitedAt
        });
        await context.SaveChangesAsync();

        var repository = new RecentlyVisitedHotelRepository(context);

        // Act
        var result = await repository.GetByUserIdAsync(userId, 5, CancellationToken.None);

        // Assert
        result.First().VisitedAt.Should().Be(visitedAt);
    }

    #endregion

    #region User Isolation Tests

    [Fact]
    public async Task GetByUserIdAsync_DoesNotReturnOtherUsersVisits()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var city = await SeedCityAsync(context, "Test City");
        var hotel1 = await SeedHotelAsync(context, city.Id, "User Hotel");
        var hotel2 = await SeedHotelAsync(context, city.Id, "Other User Hotel");

        await SeedVisitAsync(context, userId, hotel1.Id);
        await SeedVisitAsync(context, otherUserId, hotel2.Id);

        var repository = new RecentlyVisitedHotelRepository(context);

        // Act
        var result = await repository.GetByUserIdAsync(userId, 10, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().HotelName.Should().Be("User Hotel");
    }

    [Fact]
    public async Task GetByUserIdAsync_WithNonExistentUser_ReturnsEmptyList()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var existingUserId = Guid.NewGuid();
        var nonExistentUserId = Guid.NewGuid();
        await SeedRecentlyVisitedHotelsAsync(context, existingUserId, 5);
        var repository = new RecentlyVisitedHotelRepository(context);

        // Act
        var result = await repository.GetByUserIdAsync(nonExistentUserId, 10, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task GetByUserIdAsync_WithSameHotelVisitedMultipleTimes_ReturnsAllVisits()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var userId = Guid.NewGuid();
        var city = await SeedCityAsync(context, "Test City");
        var hotel = await SeedHotelAsync(context, city.Id, "Repeated Hotel");

        // Visit the same hotel 3 times
        for (int i = 0; i < 3; i++)
        {
            context.RecentlyVisitedHotels.Add(new RecentlyVisitedHotel
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                HotelId = hotel.Id,
                VisitedAt = DateTime.UtcNow.AddHours(-i)
            });
        }
        await context.SaveChangesAsync();

        var repository = new RecentlyVisitedHotelRepository(context);

        // Act
        var result = await repository.GetByUserIdAsync(userId, 10, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
        result.Should().OnlyContain(r => r.HotelName == "Repeated Hotel");
    }

    [Fact]
    public async Task GetByUserIdAsync_WithDifferentCities_ReturnsMixedCities()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var userId = Guid.NewGuid();
        var dubai = await SeedCityAsync(context, "Dubai");
        var abuDhabi = await SeedCityAsync(context, "Abu Dhabi");
        var hotel1 = await SeedHotelAsync(context, dubai.Id, "Dubai Hotel");
        var hotel2 = await SeedHotelAsync(context, abuDhabi.Id, "Abu Dhabi Hotel");

        await SeedVisitAsync(context, userId, hotel1.Id);
        await SeedVisitAsync(context, userId, hotel2.Id);

        var repository = new RecentlyVisitedHotelRepository(context);

        // Act
        var result = await repository.GetByUserIdAsync(userId, 10, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Select(r => r.CityName).Should().Contain("Dubai");
        result.Select(r => r.CityName).Should().Contain("Abu Dhabi");
    }

    #endregion

    #region Cancellation Tests

    [Fact]
    public async Task GetByUserIdAsync_WithCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var userId = Guid.NewGuid();
        await SeedRecentlyVisitedHotelsAsync(context, userId, 3);
        var repository = new RecentlyVisitedHotelRepository(context);
        var cts = new CancellationTokenSource();

        // Act
        var result = await repository.GetByUserIdAsync(userId, 5, cts.Token);

        // Assert
        result.Should().HaveCount(3);
    }

    #endregion

    #region Helper Methods

    private async Task<City> SeedCityAsync(ApplicationDbContext context, string name)
    {
        var city = new City
        {
            Id = Guid.NewGuid(),
            Name = name,
            Country = "UAE",
            PostOffice = "12345"
        };
        context.Cities.Add(city);
        await context.SaveChangesAsync();
        return city;
    }

    private async Task<Hotel> SeedHotelAsync(
        ApplicationDbContext context,
        Guid cityId,
        string name,
        int starRating = 4,
        string? thumbnailUrl = null,
        decimal minRoomPrice = 200m)
    {
        var hotel = new Hotel
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = "Test description",
            StarRating = starRating,
            CityId = cityId,
            Address = "123 Test St",
            Latitude = 0m,
            Longitude = 0m,
            ThumbnailUrl = thumbnailUrl,
            MinRoomPrice = minRoomPrice,
            DiscountPercentage = 0
        };
        context.Hotels.Add(hotel);
        await context.SaveChangesAsync();
        return hotel;
    }

    private async Task SeedVisitAsync(ApplicationDbContext context, Guid userId, Guid hotelId)
    {
        context.RecentlyVisitedHotels.Add(new RecentlyVisitedHotel
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            HotelId = hotelId,
            VisitedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();
    }

    private async Task SeedRecentlyVisitedHotelsAsync(ApplicationDbContext context, Guid userId, int count)
    {
        var city = await SeedCityAsync(context, "Test City " + Guid.NewGuid());

        for (int i = 0; i < count; i++)
        {
            var hotel = new Hotel
            {
                Id = Guid.NewGuid(),
                Name = $"Hotel {i + 1}",
                Description = "Test description",
                StarRating = (i % 5) + 1,
                CityId = city.Id,
                Address = $"{i + 1} Test St",
                Latitude = 0m,
                Longitude = 0m,
                MinRoomPrice = 100m + (i * 50),
                DiscountPercentage = 0
            };
            context.Hotels.Add(hotel);

            context.RecentlyVisitedHotels.Add(new RecentlyVisitedHotel
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                HotelId = hotel.Id,
                VisitedAt = DateTime.UtcNow.AddHours(-i)
            });
        }

        await context.SaveChangesAsync();
    }

    private async Task SeedVisitsWithVaryingTimesAsync(ApplicationDbContext context, Guid userId)
    {
        var city = await SeedCityAsync(context, "Test City");
        var now = DateTime.UtcNow;

        var hotels = new[]
        {
            new Hotel
            {
                Id = Guid.NewGuid(),
                Name = "Recent Hotel",
                Description = "Most recent",
                StarRating = 5,
                CityId = city.Id,
                Address = "1 Test St",
                Latitude = 0m,
                Longitude = 0m,
                MinRoomPrice = 500m,
                DiscountPercentage = 0
            },
            new Hotel
            {
                Id = Guid.NewGuid(),
                Name = "Middle Hotel",
                Description = "Middle visit",
                StarRating = 4,
                CityId = city.Id,
                Address = "2 Test St",
                Latitude = 0m,
                Longitude = 0m,
                MinRoomPrice = 300m,
                DiscountPercentage = 0
            },
            new Hotel
            {
                Id = Guid.NewGuid(),
                Name = "Old Hotel",
                Description = "Oldest visit",
                StarRating = 3,
                CityId = city.Id,
                Address = "3 Test St",
                Latitude = 0m,
                Longitude = 0m,
                MinRoomPrice = 100m,
                DiscountPercentage = 0
            }
        };

        context.Hotels.AddRange(hotels);

        context.RecentlyVisitedHotels.AddRange(new[]
        {
            new RecentlyVisitedHotel
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                HotelId = hotels[0].Id,
                VisitedAt = now // Most recent
            },
            new RecentlyVisitedHotel
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                HotelId = hotels[1].Id,
                VisitedAt = now.AddDays(-3) // Middle
            },
            new RecentlyVisitedHotel
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                HotelId = hotels[2].Id,
                VisitedAt = now.AddDays(-7) // Oldest
            }
        });

        await context.SaveChangesAsync();
    }

    #endregion
}
