using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TAABP.Application.Tests.Integration.Fixtures;
using TAABP.Domain.Entities;
using TAABP.Infrastructure.Persistence.Context;
using TAABP.Infrastructure.Persistence.Repositories;

namespace TAABP.Application.Tests.Integration.Features.Cities.Queries;

public class GetTrendingDestinationsIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public GetTrendingDestinationsIntegrationTests(DatabaseFixture fixture)
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
    public async Task GetTrendingDestinationsAsync_WithNoCities_ReturnsEmptyList()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var repository = new CityRepository(context);

        // Act
        var result = await repository.GetTrendingDestinationsAsync(5, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTrendingDestinationsAsync_WithCitiesButNoVisits_ReturnsEmptyList()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedCitiesWithHotelsNoVisitsAsync(context);
        var repository = new CityRepository(context);

        // Act
        var result = await repository.GetTrendingDestinationsAsync(5, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTrendingDestinationsAsync_WithVisits_ReturnsCitiesWithVisits()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedCitiesWithVisitsAsync(context);
        var repository = new CityRepository(context);

        // Act
        var result = await repository.GetTrendingDestinationsAsync(10, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(d => d.VisitCount > 0);
    }

    #endregion

    #region Count/Limit Tests

    [Fact]
    public async Task GetTrendingDestinationsAsync_WithCountOf5_ReturnsMax5Destinations()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedMultipleCitiesWithVisitsAsync(context, 10);
        var repository = new CityRepository(context);

        // Act
        var result = await repository.GetTrendingDestinationsAsync(5, CancellationToken.None);

        // Assert
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetTrendingDestinationsAsync_WithCountOf3_ReturnsMax3Destinations()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedMultipleCitiesWithVisitsAsync(context, 10);
        var repository = new CityRepository(context);

        // Act
        var result = await repository.GetTrendingDestinationsAsync(3, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetTrendingDestinationsAsync_WhenFewerCitiesThanCount_ReturnsAllAvailable()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedMultipleCitiesWithVisitsAsync(context, 3);
        var repository = new CityRepository(context);

        // Act
        var result = await repository.GetTrendingDestinationsAsync(10, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(20)]
    public async Task GetTrendingDestinationsAsync_WithVariousCounts_RespectsLimit(int count)
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedMultipleCitiesWithVisitsAsync(context, 25);
        var repository = new CityRepository(context);

        // Act
        var result = await repository.GetTrendingDestinationsAsync(count, CancellationToken.None);

        // Assert
        result.Should().HaveCount(count);
    }

    #endregion

    #region Sorting Tests

    [Fact]
    public async Task GetTrendingDestinationsAsync_ReturnsDestinationsOrderedByVisitCountDescending()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedCitiesWithVaryingVisitCountsAsync(context);
        var repository = new CityRepository(context);

        // Act
        var result = await repository.GetTrendingDestinationsAsync(10, CancellationToken.None);

        // Assert
        result.Should().BeInDescendingOrder(d => d.VisitCount);
    }

    [Fact]
    public async Task GetTrendingDestinationsAsync_MostVisitedCityFirst()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedCitiesWithVaryingVisitCountsAsync(context);
        var repository = new CityRepository(context);

        // Act
        var result = await repository.GetTrendingDestinationsAsync(5, CancellationToken.None);

        // Assert
        result.First().CityName.Should().Be("Most Visited City"); // 100 visits
    }

    #endregion

    #region Data Mapping Tests

    [Fact]
    public async Task GetTrendingDestinationsAsync_MapsCityIdCorrectly()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var cityId = await SeedSingleCityWithVisitsAsync(context);
        var repository = new CityRepository(context);

        // Act
        var result = await repository.GetTrendingDestinationsAsync(5, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().CityId.Should().Be(cityId);
    }

    [Fact]
    public async Task GetTrendingDestinationsAsync_MapsCityNameCorrectly()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedSingleCityWithVisitsAsync(context, cityName: "Dubai");
        var repository = new CityRepository(context);

        // Act
        var result = await repository.GetTrendingDestinationsAsync(5, CancellationToken.None);

        // Assert
        result.First().CityName.Should().Be("Dubai");
    }

    [Fact]
    public async Task GetTrendingDestinationsAsync_MapsCountryCorrectly()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedSingleCityWithVisitsAsync(context, country: "UAE");
        var repository = new CityRepository(context);

        // Act
        var result = await repository.GetTrendingDestinationsAsync(5, CancellationToken.None);

        // Assert
        result.First().Country.Should().Be("UAE");
    }

    [Fact]
    public async Task GetTrendingDestinationsAsync_MapsThumbnailUrlCorrectly()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedSingleCityWithVisitsAsync(context, thumbnailUrl: "https://example.com/dubai.jpg");
        var repository = new CityRepository(context);

        // Act
        var result = await repository.GetTrendingDestinationsAsync(5, CancellationToken.None);

        // Assert
        result.First().ThumbnailUrl.Should().Be("https://example.com/dubai.jpg");
    }

    [Fact]
    public async Task GetTrendingDestinationsAsync_MapsNullThumbnailUrlCorrectly()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedSingleCityWithVisitsAsync(context, thumbnailUrl: null);
        var repository = new CityRepository(context);

        // Act
        var result = await repository.GetTrendingDestinationsAsync(5, CancellationToken.None);

        // Assert
        result.First().ThumbnailUrl.Should().BeNull();
    }

    [Fact]
    public async Task GetTrendingDestinationsAsync_MapsVisitCountCorrectly()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedSingleCityWithVisitsAsync(context, visitCount: 150);
        var repository = new CityRepository(context);

        // Act
        var result = await repository.GetTrendingDestinationsAsync(5, CancellationToken.None);

        // Assert
        result.First().VisitCount.Should().Be(150);
    }

    #endregion

    #region Visit Count Aggregation Tests

    [Fact]
    public async Task GetTrendingDestinationsAsync_AggregatesVisitsAcrossMultipleHotels()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = new City
        {
            Id = Guid.NewGuid(),
            Name = "Dubai",
            Country = "UAE",
            PostOffice = "00000"
        };
        context.Cities.Add(city);

        // Add multiple hotels in same city
        var hotel1 = new Hotel
        {
            Id = Guid.NewGuid(),
            Name = "Hotel 1",
            CityId = city.Id,
            Address = "Address 1",
            StarRating = 5,
            MinRoomPrice = 200m
        };
        var hotel2 = new Hotel
        {
            Id = Guid.NewGuid(),
            Name = "Hotel 2",
            CityId = city.Id,
            Address = "Address 2",
            StarRating = 4,
            MinRoomPrice = 150m
        };
        context.Hotels.AddRange(hotel1, hotel2);

        // Add visits to different hotels
        var userId = Guid.NewGuid();
        for (int i = 0; i < 5; i++)
        {
            context.RecentlyVisitedHotels.Add(new RecentlyVisitedHotel
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                HotelId = hotel1.Id,
                VisitedAt = DateTime.UtcNow.AddHours(-i)
            });
        }
        for (int i = 0; i < 3; i++)
        {
            context.RecentlyVisitedHotels.Add(new RecentlyVisitedHotel
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                HotelId = hotel2.Id,
                VisitedAt = DateTime.UtcNow.AddHours(-i)
            });
        }
        await context.SaveChangesAsync();

        var repository = new CityRepository(context);

        // Act
        var result = await repository.GetTrendingDestinationsAsync(5, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().VisitCount.Should().Be(8); // 5 + 3 visits
    }

    [Fact]
    public async Task GetTrendingDestinationsAsync_CountsVisitsFromMultipleUsers()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = new City
        {
            Id = Guid.NewGuid(),
            Name = "Dubai",
            Country = "UAE",
            PostOffice = "00000"
        };
        context.Cities.Add(city);

        var hotel = new Hotel
        {
            Id = Guid.NewGuid(),
            Name = "Hotel",
            CityId = city.Id,
            Address = "Address",
            StarRating = 5,
            MinRoomPrice = 200m
        };
        context.Hotels.Add(hotel);

        // Add visits from different users
        for (int i = 0; i < 10; i++)
        {
            context.RecentlyVisitedHotels.Add(new RecentlyVisitedHotel
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(), // Different user each time
                HotelId = hotel.Id,
                VisitedAt = DateTime.UtcNow.AddHours(-i)
            });
        }
        await context.SaveChangesAsync();

        var repository = new CityRepository(context);

        // Act
        var result = await repository.GetTrendingDestinationsAsync(5, CancellationToken.None);

        // Assert
        result.First().VisitCount.Should().Be(10);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task GetTrendingDestinationsAsync_ExcludesCitiesWithZeroVisits()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();

        // City with visits
        var cityWithVisits = new City
        {
            Id = Guid.NewGuid(),
            Name = "Popular City",
            Country = "UAE",
            PostOffice = "00000"
        };

        // City without visits
        var cityWithoutVisits = new City
        {
            Id = Guid.NewGuid(),
            Name = "Empty City",
            Country = "UAE",
            PostOffice = "11111"
        };

        context.Cities.AddRange(cityWithVisits, cityWithoutVisits);

        var hotel1 = new Hotel
        {
            Id = Guid.NewGuid(),
            Name = "Hotel in Popular City",
            CityId = cityWithVisits.Id,
            Address = "Address",
            StarRating = 5,
            MinRoomPrice = 200m
        };
        var hotel2 = new Hotel
        {
            Id = Guid.NewGuid(),
            Name = "Hotel in Empty City",
            CityId = cityWithoutVisits.Id,
            Address = "Address",
            StarRating = 4,
            MinRoomPrice = 150m
        };
        context.Hotels.AddRange(hotel1, hotel2);

        context.RecentlyVisitedHotels.Add(new RecentlyVisitedHotel
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            HotelId = hotel1.Id,
            VisitedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        var repository = new CityRepository(context);

        // Act
        var result = await repository.GetTrendingDestinationsAsync(10, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().CityName.Should().Be("Popular City");
    }

    [Fact]
    public async Task GetTrendingDestinationsAsync_WithDifferentCountries_ReturnsMixedCountries()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();

        var uaeCity = await SeedCityWithVisitsAsync(context, "Dubai", "UAE", 50);
        var franceCity = await SeedCityWithVisitsAsync(context, "Paris", "France", 30);

        var repository = new CityRepository(context);

        // Act
        var result = await repository.GetTrendingDestinationsAsync(10, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Select(r => r.Country).Should().Contain("UAE");
        result.Select(r => r.Country).Should().Contain("France");
    }

    #endregion

    #region Cancellation Tests

    [Fact]
    public async Task GetTrendingDestinationsAsync_WithCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedMultipleCitiesWithVisitsAsync(context, 3);
        var repository = new CityRepository(context);
        var cts = new CancellationTokenSource();

        // Act
        var result = await repository.GetTrendingDestinationsAsync(5, cts.Token);

        // Assert
        result.Should().HaveCount(3);
    }

    #endregion

    #region Helper Methods

    private async Task SeedCitiesWithHotelsNoVisitsAsync(ApplicationDbContext context)
    {
        var city = new City
        {
            Id = Guid.NewGuid(),
            Name = "Empty City",
            Country = "UAE",
            PostOffice = "00000"
        };
        context.Cities.Add(city);

        var hotel = new Hotel
        {
            Id = Guid.NewGuid(),
            Name = "Empty Hotel",
            CityId = city.Id,
            Address = "Address",
            StarRating = 4,
            MinRoomPrice = 100m
        };
        context.Hotels.Add(hotel);

        await context.SaveChangesAsync();
    }

    private async Task SeedCitiesWithVisitsAsync(ApplicationDbContext context)
    {
        var city = new City
        {
            Id = Guid.NewGuid(),
            Name = "Dubai",
            Country = "UAE",
            PostOffice = "00000"
        };
        context.Cities.Add(city);

        var hotel = new Hotel
        {
            Id = Guid.NewGuid(),
            Name = "Dubai Hotel",
            CityId = city.Id,
            Address = "Address",
            StarRating = 5,
            MinRoomPrice = 200m
        };
        context.Hotels.Add(hotel);

        context.RecentlyVisitedHotels.Add(new RecentlyVisitedHotel
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            HotelId = hotel.Id,
            VisitedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
    }

    private async Task SeedMultipleCitiesWithVisitsAsync(ApplicationDbContext context, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var city = new City
            {
                Id = Guid.NewGuid(),
                Name = $"City {i + 1}",
                Country = "Country",
                PostOffice = $"{i:D5}"
            };
            context.Cities.Add(city);

            var hotel = new Hotel
            {
                Id = Guid.NewGuid(),
                Name = $"Hotel {i + 1}",
                CityId = city.Id,
                Address = "Address",
                StarRating = (i % 5) + 1,
                MinRoomPrice = 100m + (i * 50)
            };
            context.Hotels.Add(hotel);

            // Add varying number of visits
            for (int j = 0; j <= i; j++)
            {
                context.RecentlyVisitedHotels.Add(new RecentlyVisitedHotel
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    HotelId = hotel.Id,
                    VisitedAt = DateTime.UtcNow.AddHours(-j)
                });
            }
        }

        await context.SaveChangesAsync();
    }

    private async Task SeedCitiesWithVaryingVisitCountsAsync(ApplicationDbContext context)
    {
        var cities = new[]
        {
            ("Most Visited City", 100),
            ("Second City", 50),
            ("Third City", 25)
        };

        foreach (var (cityName, visitCount) in cities)
        {
            var city = new City
            {
                Id = Guid.NewGuid(),
                Name = cityName,
                Country = "Country",
                PostOffice = "00000"
            };
            context.Cities.Add(city);

            var hotel = new Hotel
            {
                Id = Guid.NewGuid(),
                Name = $"Hotel in {cityName}",
                CityId = city.Id,
                Address = "Address",
                StarRating = 5,
                MinRoomPrice = 200m
            };
            context.Hotels.Add(hotel);

            for (int i = 0; i < visitCount; i++)
            {
                context.RecentlyVisitedHotels.Add(new RecentlyVisitedHotel
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    HotelId = hotel.Id,
                    VisitedAt = DateTime.UtcNow.AddHours(-i)
                });
            }
        }

        await context.SaveChangesAsync();
    }

    private async Task<Guid> SeedSingleCityWithVisitsAsync(
        ApplicationDbContext context,
        string cityName = "Test City",
        string country = "Test Country",
        string? thumbnailUrl = null,
        int visitCount = 10)
    {
        var city = new City
        {
            Id = Guid.NewGuid(),
            Name = cityName,
            Country = country,
            PostOffice = "00000",
            ThumbnailUrl = thumbnailUrl
        };
        context.Cities.Add(city);

        var hotel = new Hotel
        {
            Id = Guid.NewGuid(),
            Name = "Hotel",
            CityId = city.Id,
            Address = "Address",
            StarRating = 4,
            MinRoomPrice = 200m
        };
        context.Hotels.Add(hotel);

        for (int i = 0; i < visitCount; i++)
        {
            context.RecentlyVisitedHotels.Add(new RecentlyVisitedHotel
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                HotelId = hotel.Id,
                VisitedAt = DateTime.UtcNow.AddHours(-i)
            });
        }

        await context.SaveChangesAsync();
        return city.Id;
    }

    private async Task<Guid> SeedCityWithVisitsAsync(
        ApplicationDbContext context,
        string cityName,
        string country,
        int visitCount)
    {
        var city = new City
        {
            Id = Guid.NewGuid(),
            Name = cityName,
            Country = country,
            PostOffice = "00000"
        };
        context.Cities.Add(city);

        var hotel = new Hotel
        {
            Id = Guid.NewGuid(),
            Name = $"Hotel in {cityName}",
            CityId = city.Id,
            Address = "Address",
            StarRating = 4,
            MinRoomPrice = 200m
        };
        context.Hotels.Add(hotel);

        for (int i = 0; i < visitCount; i++)
        {
            context.RecentlyVisitedHotels.Add(new RecentlyVisitedHotel
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                HotelId = hotel.Id,
                VisitedAt = DateTime.UtcNow.AddHours(-i)
            });
        }

        await context.SaveChangesAsync();
        return city.Id;
    }

    #endregion
}
