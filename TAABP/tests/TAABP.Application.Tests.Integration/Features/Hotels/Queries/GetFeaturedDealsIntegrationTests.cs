using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TAABP.Application.Tests.Integration.Fixtures;
using TAABP.Domain.Entities;
using TAABP.Infrastructure.Persistence.Context;
using TAABP.Infrastructure.Persistence.Repositories;

namespace TAABP.Application.Tests.Integration.Features.Hotels.Queries;

public class GetFeaturedDealsIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public GetFeaturedDealsIntegrationTests(DatabaseFixture fixture)
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
    public async Task GetFeaturedDealsAsync_WithNoHotels_ReturnsEmptyList()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetFeaturedDealsAsync(5, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFeaturedDealsAsync_WithNoDiscountedHotels_ReturnsEmptyList()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedHotelsWithoutDiscountsAsync(context);
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetFeaturedDealsAsync(5, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFeaturedDealsAsync_WithDiscountedHotels_ReturnsOnlyDiscountedHotels()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedMixedHotelsAsync(context);
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetFeaturedDealsAsync(10, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(d => d.DiscountPercentage > 0);
    }

    #endregion

    #region Count/Limit Tests

    [Fact]
    public async Task GetFeaturedDealsAsync_WithCountOf5_ReturnsMax5Deals()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedMultipleDiscountedHotelsAsync(context, 10);
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetFeaturedDealsAsync(5, CancellationToken.None);

        // Assert
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetFeaturedDealsAsync_WithCountOf3_ReturnsMax3Deals()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedMultipleDiscountedHotelsAsync(context, 10);
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetFeaturedDealsAsync(3, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetFeaturedDealsAsync_WhenFewerHotelsThanCount_ReturnsAllAvailable()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedMultipleDiscountedHotelsAsync(context, 3);
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetFeaturedDealsAsync(10, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(20)]
    public async Task GetFeaturedDealsAsync_WithVariousCounts_RespectsLimit(int count)
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedMultipleDiscountedHotelsAsync(context, 25);
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetFeaturedDealsAsync(count, CancellationToken.None);

        // Assert
        result.Should().HaveCount(count);
    }

    #endregion

    #region Sorting Tests

    [Fact]
    public async Task GetFeaturedDealsAsync_ReturnsDealsOrderedByDiscountDescending()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedHotelsWithVaryingDiscountsAsync(context);
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetFeaturedDealsAsync(10, CancellationToken.None);

        // Assert
        result.Should().BeInDescendingOrder(d => d.DiscountPercentage);
    }

    [Fact]
    public async Task GetFeaturedDealsAsync_WithMultipleHotels_HighestDiscountFirst()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedHotelsWithVaryingDiscountsAsync(context);
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetFeaturedDealsAsync(5, CancellationToken.None);

        // Assert
        result.First().DiscountPercentage.Should().Be(30); // Highest discount
    }

    #endregion

    #region Data Mapping Tests

    [Fact]
    public async Task GetFeaturedDealsAsync_MapsHotelIdCorrectly()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var hotelId = await SeedSingleDiscountedHotelAsync(context);
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetFeaturedDealsAsync(5, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().HotelId.Should().Be(hotelId);
    }

    [Fact]
    public async Task GetFeaturedDealsAsync_MapsHotelNameCorrectly()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedSingleDiscountedHotelAsync(context, "Test Luxury Hotel");
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetFeaturedDealsAsync(5, CancellationToken.None);

        // Assert
        result.First().HotelName.Should().Be("Test Luxury Hotel");
    }

    [Fact]
    public async Task GetFeaturedDealsAsync_MapsCityNameCorrectly()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedSingleDiscountedHotelAsync(context, cityName: "Dubai");
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetFeaturedDealsAsync(5, CancellationToken.None);

        // Assert
        result.First().CityName.Should().Be("Dubai");
    }

    [Fact]
    public async Task GetFeaturedDealsAsync_MapsCountryCorrectly()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedSingleDiscountedHotelAsync(context, country: "United Arab Emirates");
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetFeaturedDealsAsync(5, CancellationToken.None);

        // Assert
        result.First().Country.Should().Be("United Arab Emirates");
    }

    [Fact]
    public async Task GetFeaturedDealsAsync_MapsStarRatingCorrectly()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedSingleDiscountedHotelAsync(context, starRating: 5);
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetFeaturedDealsAsync(5, CancellationToken.None);

        // Assert
        result.First().StarRating.Should().Be(5);
    }

    [Fact]
    public async Task GetFeaturedDealsAsync_MapsThumbnailUrlCorrectly()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedSingleDiscountedHotelAsync(context, thumbnailUrl: "https://example.com/image.jpg");
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetFeaturedDealsAsync(5, CancellationToken.None);

        // Assert
        result.First().ThumbnailUrl.Should().Be("https://example.com/image.jpg");
    }

    [Fact]
    public async Task GetFeaturedDealsAsync_MapsOriginalPriceCorrectly()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedSingleDiscountedHotelAsync(context, originalPrice: 500m);
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetFeaturedDealsAsync(5, CancellationToken.None);

        // Assert
        result.First().OriginalPrice.Should().Be(500m);
    }

    [Fact]
    public async Task GetFeaturedDealsAsync_CalculatesDiscountedPriceCorrectly()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedSingleDiscountedHotelAsync(context, originalPrice: 1000m, discountPercentage: 20);
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetFeaturedDealsAsync(5, CancellationToken.None);

        // Assert
        result.First().OriginalPrice.Should().Be(1000m);
        result.First().DiscountPercentage.Should().Be(20);
        result.First().DiscountedPrice.Should().Be(800m); // 1000 - 20% = 800
    }

    [Fact]
    public async Task GetFeaturedDealsAsync_MapsDiscountPercentageCorrectly()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedSingleDiscountedHotelAsync(context, discountPercentage: 15);
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetFeaturedDealsAsync(5, CancellationToken.None);

        // Assert
        result.First().DiscountPercentage.Should().Be(15);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task GetFeaturedDealsAsync_WithZeroDiscount_ExcludesHotel()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedHotelWithZeroDiscountAsync(context);
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetFeaturedDealsAsync(10, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFeaturedDealsAsync_WithNullDiscount_ExcludesHotel()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedHotelsWithoutDiscountsAsync(context);
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetFeaturedDealsAsync(10, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFeaturedDealsAsync_WithNullThumbnail_HandlesGracefully()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedSingleDiscountedHotelAsync(context, thumbnailUrl: null);
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetFeaturedDealsAsync(5, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().ThumbnailUrl.Should().BeNull();
    }

    #endregion

    #region Helper Methods

    private async Task SeedHotelsWithoutDiscountsAsync(ApplicationDbContext context)
    {
        var city = new City
        {
            Id = Guid.NewGuid(),
            Name = "Test City",
            Country = "Test Country",
            PostOffice = "12345"
        };
        context.Cities.Add(city);

        var hotels = new[]
        {
            new Hotel
            {
                Id = Guid.NewGuid(),
                Name = "Hotel Without Discount 1",
                Description = "No discount",
                StarRating = 4,
                CityId = city.Id,
                Address = "123 Test St",
                Latitude = 0m,
                Longitude = 0m,
                MinRoomPrice = 200m,
                DiscountPercentage = null
            },
            new Hotel
            {
                Id = Guid.NewGuid(),
                Name = "Hotel Without Discount 2",
                Description = "No discount",
                StarRating = 3,
                CityId = city.Id,
                Address = "456 Test St",
                Latitude = 0m,
                Longitude = 0m,
                MinRoomPrice = 150m,
                DiscountPercentage = null
            }
        };

        context.Hotels.AddRange(hotels);
        await context.SaveChangesAsync();
    }

    private async Task SeedMixedHotelsAsync(ApplicationDbContext context)
    {
        var city = new City
        {
            Id = Guid.NewGuid(),
            Name = "Test City",
            Country = "Test Country",
            PostOffice = "12345"
        };
        context.Cities.Add(city);

        var hotels = new[]
        {
            new Hotel
            {
                Id = Guid.NewGuid(),
                Name = "Discounted Hotel",
                Description = "Has discount",
                StarRating = 5,
                CityId = city.Id,
                Address = "123 Discount St",
                Latitude = 0m,
                Longitude = 0m,
                MinRoomPrice = 500m,
                DiscountPercentage = 15
            },
            new Hotel
            {
                Id = Guid.NewGuid(),
                Name = "Regular Hotel",
                Description = "No discount",
                StarRating = 4,
                CityId = city.Id,
                Address = "456 Regular St",
                Latitude = 0m,
                Longitude = 0m,
                MinRoomPrice = 300m,
                DiscountPercentage = null
            }
        };

        context.Hotels.AddRange(hotels);
        await context.SaveChangesAsync();
    }

    private async Task SeedMultipleDiscountedHotelsAsync(ApplicationDbContext context, int count)
    {
        var city = new City
        {
            Id = Guid.NewGuid(),
            Name = "Test City",
            Country = "Test Country",
            PostOffice = "12345"
        };
        context.Cities.Add(city);

        for (int i = 1; i <= count; i++)
        {
            context.Hotels.Add(new Hotel
            {
                Id = Guid.NewGuid(),
                Name = $"Hotel {i}",
                Description = $"Description {i}",
                StarRating = (i % 5) + 1,
                CityId = city.Id,
                Address = $"{i} Test St",
                Latitude = 0m,
                Longitude = 0m,
                MinRoomPrice = 100m + (i * 50),
                DiscountPercentage = 5 + (i % 20)
            });
        }

        await context.SaveChangesAsync();
    }

    private async Task SeedHotelsWithVaryingDiscountsAsync(ApplicationDbContext context)
    {
        var city = new City
        {
            Id = Guid.NewGuid(),
            Name = "Test City",
            Country = "Test Country",
            PostOffice = "12345"
        };
        context.Cities.Add(city);

        var hotels = new[]
        {
            new Hotel
            {
                Id = Guid.NewGuid(),
                Name = "Low Discount Hotel",
                Description = "5% off",
                StarRating = 3,
                CityId = city.Id,
                Address = "1 Test St",
                Latitude = 0m,
                Longitude = 0m,
                MinRoomPrice = 100m,
                DiscountPercentage = 5
            },
            new Hotel
            {
                Id = Guid.NewGuid(),
                Name = "High Discount Hotel",
                Description = "30% off",
                StarRating = 5,
                CityId = city.Id,
                Address = "2 Test St",
                Latitude = 0m,
                Longitude = 0m,
                MinRoomPrice = 500m,
                DiscountPercentage = 30
            },
            new Hotel
            {
                Id = Guid.NewGuid(),
                Name = "Medium Discount Hotel",
                Description = "15% off",
                StarRating = 4,
                CityId = city.Id,
                Address = "3 Test St",
                Latitude = 0m,
                Longitude = 0m,
                MinRoomPrice = 300m,
                DiscountPercentage = 15
            }
        };

        context.Hotels.AddRange(hotels);
        await context.SaveChangesAsync();
    }

    private async Task<Guid> SeedSingleDiscountedHotelAsync(
        ApplicationDbContext context,
        string hotelName = "Test Hotel",
        string cityName = "Test City",
        string country = "Test Country",
        int starRating = 4,
        string? thumbnailUrl = null,
        decimal originalPrice = 200m,
        int discountPercentage = 10)
    {
        var city = new City
        {
            Id = Guid.NewGuid(),
            Name = cityName,
            Country = country,
            PostOffice = "12345"
        };
        context.Cities.Add(city);

        var hotel = new Hotel
        {
            Id = Guid.NewGuid(),
            Name = hotelName,
            Description = "Test description",
            StarRating = starRating,
            CityId = city.Id,
            Address = "123 Test St",
            Latitude = 0m,
            Longitude = 0m,
            ThumbnailUrl = thumbnailUrl,
            MinRoomPrice = originalPrice,
            DiscountPercentage = discountPercentage
        };

        context.Hotels.Add(hotel);
        await context.SaveChangesAsync();

        return hotel.Id;
    }

    private async Task SeedHotelWithZeroDiscountAsync(ApplicationDbContext context)
    {
        var city = new City
        {
            Id = Guid.NewGuid(),
            Name = "Test City",
            Country = "Test Country",
            PostOffice = "12345"
        };
        context.Cities.Add(city);

        var hotel = new Hotel
        {
            Id = Guid.NewGuid(),
            Name = "Zero Discount Hotel",
            Description = "0% discount",
            StarRating = 3,
            CityId = city.Id,
            Address = "123 Zero St",
            Latitude = 0m,
            Longitude = 0m,
            MinRoomPrice = 100m,
            DiscountPercentage = 0
        };

        context.Hotels.Add(hotel);
        await context.SaveChangesAsync();
    }

    #endregion
}
