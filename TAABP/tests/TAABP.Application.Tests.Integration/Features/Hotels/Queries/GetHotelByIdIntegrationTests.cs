using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TAABP.Application.Tests.Integration.Fixtures;
using TAABP.Domain.Entities;
using TAABP.Domain.Enums;
using TAABP.Infrastructure.Persistence.Context;
using TAABP.Infrastructure.Persistence.Repositories;

namespace TAABP.Application.Tests.Integration.Features.Hotels.Queries;

public class GetHotelByIdIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public GetHotelByIdIntegrationTests(DatabaseFixture fixture)
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
    public async Task GetByIdWithDetailsAsync_WithNonExistentHotel_ReturnsNull()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var nonExistentId = Guid.NewGuid();
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetByIdWithDetailsAsync(nonExistentId, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_WithExistingHotel_ReturnsHotelDetails()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Grand Hotel");
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetByIdWithDetailsAsync(hotel.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(hotel.Id);
        result.Name.Should().Be("Grand Hotel");
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_WithEmptyGuid_ReturnsNull()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetByIdWithDetailsAsync(Guid.Empty, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Hotel Property Mapping Tests

    [Fact]
    public async Task GetByIdWithDetailsAsync_MapsHotelIdCorrectly()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Test Hotel");
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetByIdWithDetailsAsync(hotel.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(hotel.Id);
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_MapsNameCorrectly()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Luxury Palace Hotel");
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetByIdWithDetailsAsync(hotel.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Luxury Palace Hotel");
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_MapsDescriptionCorrectly()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Test Hotel",
            description: "A magnificent luxury hotel with stunning views");
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetByIdWithDetailsAsync(hotel.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Description.Should().Be("A magnificent luxury hotel with stunning views");
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_MapsNullDescriptionCorrectly()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Test Hotel", description: null);
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetByIdWithDetailsAsync(hotel.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Description.Should().BeNull();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public async Task GetByIdWithDetailsAsync_MapsStarRatingCorrectly(int starRating)
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Test Hotel", starRating: starRating);
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetByIdWithDetailsAsync(hotel.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.StarRating.Should().Be(starRating);
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_MapsAddressCorrectly()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Test Hotel",
            address: "123 Sheikh Zayed Road, Downtown Dubai");
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetByIdWithDetailsAsync(hotel.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Address.Should().Be("123 Sheikh Zayed Road, Downtown Dubai");
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_MapsCoordinatesCorrectly()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Test Hotel",
            latitude: 25.2048m, longitude: 55.2708m);
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetByIdWithDetailsAsync(hotel.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Latitude.Should().Be(25.2048m);
        result.Longitude.Should().Be(55.2708m);
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_MapsNullCoordinatesCorrectly()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Test Hotel",
            latitude: null, longitude: null);
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetByIdWithDetailsAsync(hotel.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Latitude.Should().BeNull();
        result.Longitude.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_MapsNearbyAttractionsCorrectly()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Test Hotel",
            nearbyAttractions: "Burj Khalifa, Dubai Mall, Dubai Fountain");
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetByIdWithDetailsAsync(hotel.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.NearbyAttractions.Should().Be("Burj Khalifa, Dubai Mall, Dubai Fountain");
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_MapsMinRoomPriceCorrectly()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Test Hotel", minRoomPrice: 599.99m);
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetByIdWithDetailsAsync(hotel.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.MinRoomPrice.Should().Be(599.99m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(25)]
    [InlineData(50)]
    public async Task GetByIdWithDetailsAsync_MapsDiscountPercentageCorrectly(int discountPercentage)
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Test Hotel",
            discountPercentage: discountPercentage);
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetByIdWithDetailsAsync(hotel.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.DiscountPercentage.Should().Be(discountPercentage);
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_MapsNullDiscountPercentageCorrectly()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Test Hotel", discountPercentage: null);
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetByIdWithDetailsAsync(hotel.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.DiscountPercentage.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_MapsCreatedAtCorrectly()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var createdAt = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var hotel = await SeedHotelAsync(context, city.Id, "Test Hotel", createdAt: createdAt);
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetByIdWithDetailsAsync(hotel.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.CreatedAt.Should().Be(createdAt);
    }

    #endregion

    #region City Mapping Tests

    [Fact]
    public async Task GetByIdWithDetailsAsync_MapsCityCorrectly()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "United Arab Emirates");
        var hotel = await SeedHotelAsync(context, city.Id, "Test Hotel");
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetByIdWithDetailsAsync(hotel.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.City.Should().NotBeNull();
        result.City.Id.Should().Be(city.Id);
        result.City.Name.Should().Be("Dubai");
        result.City.Country.Should().Be("United Arab Emirates");
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_WithDifferentCities_MapsCorrectCity()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var dubai = await SeedCityAsync(context, "Dubai", "UAE");
        var abuDhabi = await SeedCityAsync(context, "Abu Dhabi", "UAE");
        var dubaiHotel = await SeedHotelAsync(context, dubai.Id, "Dubai Hotel");
        var abuDhabiHotel = await SeedHotelAsync(context, abuDhabi.Id, "Abu Dhabi Hotel");
        var repository = new HotelRepository(context);

        // Act
        var dubaiResult = await repository.GetByIdWithDetailsAsync(dubaiHotel.Id, CancellationToken.None);
        var abuDhabiResult = await repository.GetByIdWithDetailsAsync(abuDhabiHotel.Id, CancellationToken.None);

        // Assert
        dubaiResult!.City.Name.Should().Be("Dubai");
        abuDhabiResult!.City.Name.Should().Be("Abu Dhabi");
    }

    #endregion

    #region Images Mapping Tests

    [Fact]
    public async Task GetByIdWithDetailsAsync_WithNoImages_ReturnsEmptyImageList()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Test Hotel");
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetByIdWithDetailsAsync(hotel.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Images.Should().NotBeNull();
        result.Images.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_WithImages_ReturnsAllImages()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Test Hotel");
        await SeedHotelImagesAsync(context, hotel.Id, 3);
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetByIdWithDetailsAsync(hotel.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Images.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_MapsImagePropertiesCorrectly()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Test Hotel");
        var imageId = Guid.NewGuid();
        var imageUrl = "https://example.com/hotel-image.jpg";
        await SeedHotelImageAsync(context, hotel.Id, imageId, imageUrl);
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetByIdWithDetailsAsync(hotel.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Images.Should().HaveCount(1);
        result.Images.First().Id.Should().Be(imageId);
        result.Images.First().ImageUrl.Should().Be(imageUrl);
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_WithManyImages_ReturnsAllImages()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Test Hotel");
        await SeedHotelImagesAsync(context, hotel.Id, 20);
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetByIdWithDetailsAsync(hotel.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Images.Should().HaveCount(20);
    }

    #endregion

    #region Amenities Mapping Tests

    [Fact]
    public async Task GetByIdWithDetailsAsync_WithNoAmenities_ReturnsEmptyAmenityList()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Test Hotel");
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetByIdWithDetailsAsync(hotel.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Amenities.Should().NotBeNull();
        result.Amenities.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_WithAmenities_ReturnsAllAmenities()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Test Hotel");
        await SeedHotelAmenitiesAsync(context, hotel.Id, new[] { "Free WiFi", "Pool", "Gym" });
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetByIdWithDetailsAsync(hotel.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Amenities.Should().HaveCount(3);
        result.Amenities.Select(a => a.Name).Should().Contain("Free WiFi");
        result.Amenities.Select(a => a.Name).Should().Contain("Pool");
        result.Amenities.Select(a => a.Name).Should().Contain("Gym");
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_MapsAmenityPropertiesCorrectly()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Test Hotel");
        var amenityId = Guid.NewGuid();
        await SeedHotelAmenityAsync(context, hotel.Id, amenityId, "Free WiFi");
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetByIdWithDetailsAsync(hotel.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Amenities.Should().HaveCount(1);
        result.Amenities.First().Id.Should().Be(amenityId);
        result.Amenities.First().Name.Should().Be("Free WiFi");
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_WithManyAmenities_ReturnsAllAmenities()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Test Hotel");
        var amenityNames = Enumerable.Range(1, 15).Select(i => $"Amenity {i}").ToArray();
        await SeedHotelAmenitiesAsync(context, hotel.Id, amenityNames);
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetByIdWithDetailsAsync(hotel.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Amenities.Should().HaveCount(15);
    }

    #endregion

    #region Review Statistics Tests

    [Fact]
    public async Task GetByIdWithDetailsAsync_WithNoReviews_ReturnsZeroStats()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Test Hotel");
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetByIdWithDetailsAsync(hotel.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.AverageRating.Should().Be(0m);
        result.ReviewCount.Should().Be(0);
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_WithReviews_ReturnsCorrectCount()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Test Hotel");
        await SeedReviewsAsync(context, hotel.Id, new[] { 5, 4, 4, 5, 3 });
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetByIdWithDetailsAsync(hotel.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.ReviewCount.Should().Be(5);
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_WithReviews_ReturnsCorrectAverageRating()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Test Hotel");
        // 5 + 4 + 4 + 5 + 2 = 20 / 5 = 4.0
        await SeedReviewsAsync(context, hotel.Id, new[] { 5, 4, 4, 5, 2 });
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetByIdWithDetailsAsync(hotel.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.AverageRating.Should().Be(4.0m);
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_WithSingleReview_ReturnsCorrectStats()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Test Hotel");
        await SeedReviewsAsync(context, hotel.Id, new[] { 5 });
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetByIdWithDetailsAsync(hotel.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.AverageRating.Should().Be(5.0m);
        result.ReviewCount.Should().Be(1);
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_WithManyReviews_CalculatesCorrectAverage()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Test Hotel");
        // Create 100 reviews with ratings 1-5 (average should be around 3)
        var ratings = Enumerable.Range(1, 100).Select(i => (i % 5) + 1).ToArray();
        await SeedReviewsAsync(context, hotel.Id, ratings);
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetByIdWithDetailsAsync(hotel.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.ReviewCount.Should().Be(100);
        result.AverageRating.Should().BeGreaterThan(0m);
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_AverageRatingIsRoundedToOneDecimal()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Test Hotel");
        // 5 + 4 + 4 = 13 / 3 = 4.333... should round to 4.3
        await SeedReviewsAsync(context, hotel.Id, new[] { 5, 4, 4 });
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetByIdWithDetailsAsync(hotel.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.AverageRating.Should().Be(4.3m);
    }

    #endregion

    #region UpsertVisitAsync Tests

    [Fact]
    public async Task UpsertVisitAsync_WithNewVisit_CreatesRecord()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Test Hotel");
        var userId = Guid.NewGuid();
        var repository = new RecentlyVisitedHotelRepository(context);

        // Act
        await repository.UpsertVisitAsync(userId, hotel.Id, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        var visit = await context.RecentlyVisitedHotels
            .FirstOrDefaultAsync(v => v.UserId == userId && v.HotelId == hotel.Id);
        visit.Should().NotBeNull();
    }

    [Fact]
    public async Task UpsertVisitAsync_WithExistingVisit_UpdatesTimestamp()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Test Hotel");
        var userId = Guid.NewGuid();
        var oldVisitedAt = DateTime.UtcNow.AddDays(-5);

        context.RecentlyVisitedHotels.Add(new RecentlyVisitedHotel
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            HotelId = hotel.Id,
            VisitedAt = oldVisitedAt
        });
        await context.SaveChangesAsync();

        var repository = new RecentlyVisitedHotelRepository(context);

        // Act
        await repository.UpsertVisitAsync(userId, hotel.Id, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        var visits = await context.RecentlyVisitedHotels
            .Where(v => v.UserId == userId && v.HotelId == hotel.Id)
            .ToListAsync();

        visits.Should().HaveCount(1);
        visits.First().VisitedAt.Should().BeAfter(oldVisitedAt);
    }

    [Fact]
    public async Task UpsertVisitAsync_WithDifferentHotels_CreatesSeparateRecords()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel1 = await SeedHotelAsync(context, city.Id, "Hotel 1");
        var hotel2 = await SeedHotelAsync(context, city.Id, "Hotel 2");
        var userId = Guid.NewGuid();
        var repository = new RecentlyVisitedHotelRepository(context);

        // Act
        await repository.UpsertVisitAsync(userId, hotel1.Id, CancellationToken.None);
        await repository.UpsertVisitAsync(userId, hotel2.Id, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        var visits = await context.RecentlyVisitedHotels
            .Where(v => v.UserId == userId)
            .ToListAsync();

        visits.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpsertVisitAsync_WithDifferentUsers_CreatesSeparateRecords()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Test Hotel");
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        var repository = new RecentlyVisitedHotelRepository(context);

        // Act
        await repository.UpsertVisitAsync(user1, hotel.Id, CancellationToken.None);
        await repository.UpsertVisitAsync(user2, hotel.Id, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        var visits = await context.RecentlyVisitedHotels
            .Where(v => v.HotelId == hotel.Id)
            .ToListAsync();

        visits.Should().HaveCount(2);
    }

    #endregion

    #region Cancellation Tests

    [Fact]
    public async Task GetByIdWithDetailsAsync_WithCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Test Hotel");
        var repository = new HotelRepository(context);
        var cts = new CancellationTokenSource();

        // Act
        var result = await repository.GetByIdWithDetailsAsync(hotel.Id, cts.Token);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Hotel Isolation Tests

    [Fact]
    public async Task GetByIdWithDetailsAsync_ReturnsOnlyRequestedHotel()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel1 = await SeedHotelAsync(context, city.Id, "Hotel 1");
        var hotel2 = await SeedHotelAsync(context, city.Id, "Hotel 2");
        var hotel3 = await SeedHotelAsync(context, city.Id, "Hotel 3");
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetByIdWithDetailsAsync(hotel2.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(hotel2.Id);
        result.Name.Should().Be("Hotel 2");
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_ImagesAreIsolatedToHotel()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel1 = await SeedHotelAsync(context, city.Id, "Hotel 1");
        var hotel2 = await SeedHotelAsync(context, city.Id, "Hotel 2");
        await SeedHotelImagesAsync(context, hotel1.Id, 5);
        await SeedHotelImagesAsync(context, hotel2.Id, 3);
        var repository = new HotelRepository(context);

        // Act
        var result1 = await repository.GetByIdWithDetailsAsync(hotel1.Id, CancellationToken.None);
        var result2 = await repository.GetByIdWithDetailsAsync(hotel2.Id, CancellationToken.None);

        // Assert
        result1!.Images.Should().HaveCount(5);
        result2!.Images.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_AmenitiesAreIsolatedToHotel()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel1 = await SeedHotelAsync(context, city.Id, "Hotel 1");
        var hotel2 = await SeedHotelAsync(context, city.Id, "Hotel 2");
        await SeedHotelAmenitiesAsync(context, hotel1.Id, new[] { "WiFi", "Pool" });
        await SeedHotelAmenitiesAsync(context, hotel2.Id, new[] { "Gym" });
        var repository = new HotelRepository(context);

        // Act
        var result1 = await repository.GetByIdWithDetailsAsync(hotel1.Id, CancellationToken.None);
        var result2 = await repository.GetByIdWithDetailsAsync(hotel2.Id, CancellationToken.None);

        // Assert
        result1!.Amenities.Should().HaveCount(2);
        result2!.Amenities.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_ReviewsAreIsolatedToHotel()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel1 = await SeedHotelAsync(context, city.Id, "Hotel 1");
        var hotel2 = await SeedHotelAsync(context, city.Id, "Hotel 2");
        await SeedReviewsAsync(context, hotel1.Id, new[] { 5, 5, 5 }); // 3 reviews, avg 5.0
        await SeedReviewsAsync(context, hotel2.Id, new[] { 3, 3 }); // 2 reviews, avg 3.0
        var repository = new HotelRepository(context);

        // Act
        var result1 = await repository.GetByIdWithDetailsAsync(hotel1.Id, CancellationToken.None);
        var result2 = await repository.GetByIdWithDetailsAsync(hotel2.Id, CancellationToken.None);

        // Assert
        result1!.ReviewCount.Should().Be(3);
        result1.AverageRating.Should().Be(5.0m);
        result2!.ReviewCount.Should().Be(2);
        result2.AverageRating.Should().Be(3.0m);
    }

    #endregion

    #region Helper Methods

    private async Task<City> SeedCityAsync(ApplicationDbContext context, string name, string country)
    {
        var city = new City
        {
            Id = Guid.NewGuid(),
            Name = name,
            Country = country,
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
        string? description = "Test description",
        int starRating = 4,
        string address = "123 Test St",
        decimal? latitude = 25.0m,
        decimal? longitude = 55.0m,
        string? nearbyAttractions = null,
        decimal minRoomPrice = 200m,
        int? discountPercentage = 0,
        DateTime? createdAt = null)
    {
        var hotel = new Hotel
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            StarRating = starRating,
            CityId = cityId,
            Address = address,
            Latitude = latitude,
            Longitude = longitude,
            NearbyAttractions = nearbyAttractions,
            MinRoomPrice = minRoomPrice,
            DiscountPercentage = discountPercentage,
            ThumbnailUrl = null,
            CreatedAt = createdAt ?? DateTime.UtcNow
        };
        context.Hotels.Add(hotel);
        await context.SaveChangesAsync();
        return hotel;
    }

    private async Task SeedHotelImagesAsync(ApplicationDbContext context, Guid hotelId, int count)
    {
        for (int i = 0; i < count; i++)
        {
            context.HotelImages.Add(new HotelImage
            {
                Id = Guid.NewGuid(),
                HotelId = hotelId,
                ImageUrl = $"https://example.com/hotel-{hotelId}-image-{i}.jpg"
            });
        }
        await context.SaveChangesAsync();
    }

    private async Task SeedHotelImageAsync(ApplicationDbContext context, Guid hotelId, Guid imageId, string imageUrl)
    {
        context.HotelImages.Add(new HotelImage
        {
            Id = imageId,
            HotelId = hotelId,
            ImageUrl = imageUrl
        });
        await context.SaveChangesAsync();
    }

    private async Task SeedHotelAmenitiesAsync(ApplicationDbContext context, Guid hotelId, string[] amenityNames)
    {
        foreach (var name in amenityNames)
        {
            var amenity = new Amenity
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = $"Description for {name}"
            };
            context.Amenities.Add(amenity);

            context.HotelAmenities.Add(new HotelAmenity
            {
                HotelId = hotelId,
                AmenityId = amenity.Id
            });
        }
        await context.SaveChangesAsync();
    }

    private async Task SeedHotelAmenityAsync(ApplicationDbContext context, Guid hotelId, Guid amenityId, string amenityName)
    {
        var amenity = new Amenity
        {
            Id = amenityId,
            Name = amenityName,
            Description = $"Description for {amenityName}"
        };
        context.Amenities.Add(amenity);

        context.HotelAmenities.Add(new HotelAmenity
        {
            HotelId = hotelId,
            AmenityId = amenityId
        });
        await context.SaveChangesAsync();
    }

    private async Task SeedReviewsAsync(ApplicationDbContext context, Guid hotelId, int[] ratings)
    {
        foreach (var rating in ratings)
        {
            context.Reviews.Add(new Review
            {
                Id = Guid.NewGuid(),
                HotelId = hotelId,
                UserId = Guid.NewGuid(),
                Rating = rating,
                Content = "Test review content",
                CreatedAt = DateTime.UtcNow
            });
        }
        await context.SaveChangesAsync();
    }

    #endregion
}
