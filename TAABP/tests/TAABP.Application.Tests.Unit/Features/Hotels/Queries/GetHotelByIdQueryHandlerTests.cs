using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using TAABP.Application.Common.Errors;
using TAABP.Application.DTOs.Hotels;
using TAABP.Application.Features.Hotels.Queries.GetHotelById;
using TAABP.Application.Interfaces;
using TAABP.Application.Interfaces.Repositories;
using TAABP.Application.Interfaces.Services;

namespace TAABP.Application.Tests.Unit.Features.Hotels.Queries;

public class GetHotelByIdQueryHandlerTests
{
    private readonly Mock<IHotelRepository> _mockHotelRepository;
    private readonly Mock<IRecentlyVisitedHotelRepository> _mockRecentlyVisitedHotelRepository;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly ILogger<GetHotelByIdQueryHandler> _logger;
    private readonly GetHotelByIdQueryHandler _handler;

    public GetHotelByIdQueryHandlerTests()
    {
        _mockHotelRepository = new Mock<IHotelRepository>();
        _mockRecentlyVisitedHotelRepository = new Mock<IRecentlyVisitedHotelRepository>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _logger = LoggerFactory.Create(b => b.AddConsole())
            .CreateLogger<GetHotelByIdQueryHandler>();

        _handler = new GetHotelByIdQueryHandler(
            _mockHotelRepository.Object,
            _mockRecentlyVisitedHotelRepository.Object,
            _mockCurrentUserService.Object,
            _mockUnitOfWork.Object,
            _logger);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidHotelId_ReturnsSuccess()
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var query = new GetHotelByIdQuery(hotelId);
        var hotelDetails = CreateHotelDetailsDto(hotelId, "Grand Hotel", "Dubai");

        _mockHotelRepository
            .Setup(r => r.GetByIdWithDetailsAsync(hotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotelDetails);
        _mockCurrentUserService.Setup(s => s.UserId).Returns((Guid?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(hotelId);
        result.Value.Name.Should().Be("Grand Hotel");

        _mockHotelRepository.Verify(
            r => r.GetByIdWithDetailsAsync(hotelId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithAuthenticatedUser_RecordsVisit()
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var query = new GetHotelByIdQuery(hotelId);
        var hotelDetails = CreateHotelDetailsDto(hotelId, "Grand Hotel", "Dubai");

        _mockHotelRepository
            .Setup(r => r.GetByIdWithDetailsAsync(hotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotelDetails);
        _mockCurrentUserService.Setup(s => s.UserId).Returns(userId);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _mockRecentlyVisitedHotelRepository.Verify(
            r => r.UpsertVisitAsync(userId, hotelId, It.IsAny<CancellationToken>()),
            Times.Once);
        _mockUnitOfWork.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithUnauthenticatedUser_DoesNotRecordVisit()
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var query = new GetHotelByIdQuery(hotelId);
        var hotelDetails = CreateHotelDetailsDto(hotelId, "Grand Hotel", "Dubai");

        _mockHotelRepository
            .Setup(r => r.GetByIdWithDetailsAsync(hotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotelDetails);
        _mockCurrentUserService.Setup(s => s.UserId).Returns((Guid?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _mockRecentlyVisitedHotelRepository.Verify(
            r => r.UpsertVisitAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _mockUnitOfWork.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsHotelDetailsWithCorrectProperties()
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var cityId = Guid.NewGuid();
        var query = new GetHotelByIdQuery(hotelId);
        var hotelDetails = new HotelDetailsDto(
            hotelId,
            "Luxury Palace Hotel",
            "A magnificent luxury hotel",
            5,
            "123 Main Street",
            25.2048m,
            55.2708m,
            "Burj Khalifa, Dubai Mall",
            new HotelCityDto(cityId, "Dubai", "UAE"),
            500m,
            15,
            new List<HotelImageDto>
            {
                new HotelImageDto(Guid.NewGuid(), "https://example.com/image1.jpg"),
                new HotelImageDto(Guid.NewGuid(), "https://example.com/image2.jpg")
            },
            new List<HotelAmenityDto>
            {
                new HotelAmenityDto(Guid.NewGuid(), "Free WiFi"),
                new HotelAmenityDto(Guid.NewGuid(), "Swimming Pool")
            },
            4.5m,
            100,
            DateTime.UtcNow.AddYears(-1));

        _mockHotelRepository
            .Setup(r => r.GetByIdWithDetailsAsync(hotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotelDetails);
        _mockCurrentUserService.Setup(s => s.UserId).Returns((Guid?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var hotel = result.Value;

        hotel.Id.Should().Be(hotelId);
        hotel.Name.Should().Be("Luxury Palace Hotel");
        hotel.Description.Should().Be("A magnificent luxury hotel");
        hotel.StarRating.Should().Be(5);
        hotel.Address.Should().Be("123 Main Street");
        hotel.Latitude.Should().Be(25.2048m);
        hotel.Longitude.Should().Be(55.2708m);
        hotel.NearbyAttractions.Should().Be("Burj Khalifa, Dubai Mall");
        hotel.City.Id.Should().Be(cityId);
        hotel.City.Name.Should().Be("Dubai");
        hotel.City.Country.Should().Be("UAE");
        hotel.MinRoomPrice.Should().Be(500m);
        hotel.DiscountPercentage.Should().Be(15);
        hotel.Images.Should().HaveCount(2);
        hotel.Amenities.Should().HaveCount(2);
        hotel.AverageRating.Should().Be(4.5m);
        hotel.ReviewCount.Should().Be(100);
    }

    [Fact]
    public async Task Handle_WithHotelWithoutOptionalFields_ReturnsSuccess()
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var query = new GetHotelByIdQuery(hotelId);
        var hotelDetails = new HotelDetailsDto(
            hotelId,
            "Basic Hotel",
            null, // No description
            3,
            "456 Street",
            null, // No latitude
            null, // No longitude
            null, // No nearby attractions
            new HotelCityDto(Guid.NewGuid(), "Abu Dhabi", "UAE"),
            200m,
            null, // No discount
            new List<HotelImageDto>(), // No images
            new List<HotelAmenityDto>(), // No amenities
            0m, // No ratings
            0, // No reviews
            DateTime.UtcNow);

        _mockHotelRepository
            .Setup(r => r.GetByIdWithDetailsAsync(hotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotelDetails);
        _mockCurrentUserService.Setup(s => s.UserId).Returns((Guid?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Description.Should().BeNull();
        result.Value.Latitude.Should().BeNull();
        result.Value.Longitude.Should().BeNull();
        result.Value.NearbyAttractions.Should().BeNull();
        result.Value.DiscountPercentage.Should().BeNull();
        result.Value.Images.Should().BeEmpty();
        result.Value.Amenities.Should().BeEmpty();
        result.Value.AverageRating.Should().Be(0);
        result.Value.ReviewCount.Should().Be(0);
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenHotelNotFound_ReturnsFailure()
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var query = new GetHotelByIdQuery(hotelId);

        _mockHotelRepository
            .Setup(r => r.GetByIdWithDetailsAsync(hotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((HotelDetailsDto?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Hotel.NotFound");
        result.Error.Description.Should().Contain(hotelId.ToString());
    }

    [Fact]
    public async Task Handle_WhenHotelNotFound_DoesNotRecordVisit()
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var query = new GetHotelByIdQuery(hotelId);

        _mockHotelRepository
            .Setup(r => r.GetByIdWithDetailsAsync(hotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((HotelDetailsDto?)null);
        _mockCurrentUserService.Setup(s => s.UserId).Returns(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();

        _mockRecentlyVisitedHotelRepository.Verify(
            r => r.UpsertVisitAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _mockUnitOfWork.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithEmptyGuid_ReturnsNotFound()
    {
        // Arrange
        var emptyGuid = Guid.Empty;
        var query = new GetHotelByIdQuery(emptyGuid);

        _mockHotelRepository
            .Setup(r => r.GetByIdWithDetailsAsync(emptyGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((HotelDetailsDto?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Hotel.NotFound");
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task Handle_PassesCancellationTokenToRepository()
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var query = new GetHotelByIdQuery(hotelId);
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _mockHotelRepository
            .Setup(r => r.GetByIdWithDetailsAsync(hotelId, token))
            .ReturnsAsync((HotelDetailsDto?)null);

        // Act
        await _handler.Handle(query, token);

        // Assert
        _mockHotelRepository.Verify(
            r => r.GetByIdWithDetailsAsync(hotelId, token),
            Times.Once);
    }

    [Fact]
    public async Task Handle_PassesCancellationTokenToVisitRecording()
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var query = new GetHotelByIdQuery(hotelId);
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        var hotelDetails = CreateHotelDetailsDto(hotelId, "Test Hotel", "Dubai");
        _mockHotelRepository
            .Setup(r => r.GetByIdWithDetailsAsync(hotelId, token))
            .ReturnsAsync(hotelDetails);
        _mockCurrentUserService.Setup(s => s.UserId).Returns(userId);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(token)).ReturnsAsync(1);

        // Act
        await _handler.Handle(query, token);

        // Assert
        _mockRecentlyVisitedHotelRepository.Verify(
            r => r.UpsertVisitAsync(userId, hotelId, token),
            Times.Once);
        _mockUnitOfWork.Verify(
            u => u.SaveChangesAsync(token),
            Times.Once);
    }

    #endregion

    #region Visit Recording Integration Tests

    [Fact]
    public async Task Handle_RecordsVisitOnlyAfterHotelIsFound()
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var query = new GetHotelByIdQuery(hotelId);
        var hotelDetails = CreateHotelDetailsDto(hotelId, "Test Hotel", "Dubai");
        var callOrder = new List<string>();

        _mockHotelRepository
            .Setup(r => r.GetByIdWithDetailsAsync(hotelId, It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("GetHotel"))
            .ReturnsAsync(hotelDetails);
        _mockCurrentUserService.Setup(s => s.UserId).Returns(userId);
        _mockRecentlyVisitedHotelRepository
            .Setup(r => r.UpsertVisitAsync(userId, hotelId, It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("RecordVisit"))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("SaveChanges"))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        callOrder.Should().BeEquivalentTo(
            new[] { "GetHotel", "RecordVisit", "SaveChanges" },
            options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Handle_MultipleCallsForSameUser_RecordsVisitEachTime()
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var query = new GetHotelByIdQuery(hotelId);
        var hotelDetails = CreateHotelDetailsDto(hotelId, "Test Hotel", "Dubai");

        _mockHotelRepository
            .Setup(r => r.GetByIdWithDetailsAsync(hotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotelDetails);
        _mockCurrentUserService.Setup(s => s.UserId).Returns(userId);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _handler.Handle(query, CancellationToken.None);
        await _handler.Handle(query, CancellationToken.None);
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockRecentlyVisitedHotelRepository.Verify(
            r => r.UpsertVisitAsync(userId, hotelId, It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }

    #endregion

    #region Edge Cases

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task Handle_WithVariousStarRatings_ReturnsCorrectRating(int starRating)
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var query = new GetHotelByIdQuery(hotelId);
        var hotelDetails = new HotelDetailsDto(
            hotelId, "Test Hotel", null, starRating, "Address",
            null, null, null,
            new HotelCityDto(Guid.NewGuid(), "City", "Country"),
            100m, null,
            new List<HotelImageDto>(),
            new List<HotelAmenityDto>(),
            0m, 0, DateTime.UtcNow);

        _mockHotelRepository
            .Setup(r => r.GetByIdWithDetailsAsync(hotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotelDetails);
        _mockCurrentUserService.Setup(s => s.UserId).Returns((Guid?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.StarRating.Should().Be(starRating);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(50)]
    public async Task Handle_WithVariousDiscountPercentages_ReturnsCorrectDiscount(int discountPercentage)
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var query = new GetHotelByIdQuery(hotelId);
        var hotelDetails = new HotelDetailsDto(
            hotelId, "Test Hotel", null, 4, "Address",
            null, null, null,
            new HotelCityDto(Guid.NewGuid(), "City", "Country"),
            100m, discountPercentage,
            new List<HotelImageDto>(),
            new List<HotelAmenityDto>(),
            0m, 0, DateTime.UtcNow);

        _mockHotelRepository
            .Setup(r => r.GetByIdWithDetailsAsync(hotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotelDetails);
        _mockCurrentUserService.Setup(s => s.UserId).Returns((Guid?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.DiscountPercentage.Should().Be(discountPercentage);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(4.5, 10)]
    [InlineData(5.0, 100)]
    [InlineData(3.7, 50)]
    public async Task Handle_WithVariousReviewStats_ReturnsCorrectStats(decimal avgRating, int reviewCount)
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var query = new GetHotelByIdQuery(hotelId);
        var hotelDetails = new HotelDetailsDto(
            hotelId, "Test Hotel", null, 4, "Address",
            null, null, null,
            new HotelCityDto(Guid.NewGuid(), "City", "Country"),
            100m, null,
            new List<HotelImageDto>(),
            new List<HotelAmenityDto>(),
            avgRating, reviewCount, DateTime.UtcNow);

        _mockHotelRepository
            .Setup(r => r.GetByIdWithDetailsAsync(hotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotelDetails);
        _mockCurrentUserService.Setup(s => s.UserId).Returns((Guid?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AverageRating.Should().Be(avgRating);
        result.Value.ReviewCount.Should().Be(reviewCount);
    }

    [Fact]
    public async Task Handle_WithManyImages_ReturnsAllImages()
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var query = new GetHotelByIdQuery(hotelId);
        var images = Enumerable.Range(1, 20)
            .Select(i => new HotelImageDto(Guid.NewGuid(), $"https://example.com/image{i}.jpg"))
            .ToList();

        var hotelDetails = new HotelDetailsDto(
            hotelId, "Test Hotel", null, 4, "Address",
            null, null, null,
            new HotelCityDto(Guid.NewGuid(), "City", "Country"),
            100m, null,
            images,
            new List<HotelAmenityDto>(),
            0m, 0, DateTime.UtcNow);

        _mockHotelRepository
            .Setup(r => r.GetByIdWithDetailsAsync(hotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotelDetails);
        _mockCurrentUserService.Setup(s => s.UserId).Returns((Guid?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Images.Should().HaveCount(20);
    }

    [Fact]
    public async Task Handle_WithManyAmenities_ReturnsAllAmenities()
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var query = new GetHotelByIdQuery(hotelId);
        var amenities = Enumerable.Range(1, 15)
            .Select(i => new HotelAmenityDto(Guid.NewGuid(), $"Amenity {i}"))
            .ToList();

        var hotelDetails = new HotelDetailsDto(
            hotelId, "Test Hotel", null, 4, "Address",
            null, null, null,
            new HotelCityDto(Guid.NewGuid(), "City", "Country"),
            100m, null,
            new List<HotelImageDto>(),
            amenities,
            0m, 0, DateTime.UtcNow);

        _mockHotelRepository
            .Setup(r => r.GetByIdWithDetailsAsync(hotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotelDetails);
        _mockCurrentUserService.Setup(s => s.UserId).Returns((Guid?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Amenities.Should().HaveCount(15);
    }

    #endregion

    #region Helper Methods

    private static HotelDetailsDto CreateHotelDetailsDto(
        Guid hotelId,
        string name,
        string cityName)
    {
        return new HotelDetailsDto(
            hotelId,
            name,
            $"Description of {name}",
            4,
            "123 Test Street",
            25.0m,
            55.0m,
            "Nearby attractions",
            new HotelCityDto(Guid.NewGuid(), cityName, "UAE"),
            300m,
            10,
            new List<HotelImageDto>
            {
                new HotelImageDto(Guid.NewGuid(), "https://example.com/thumb.jpg")
            },
            new List<HotelAmenityDto>
            {
                new HotelAmenityDto(Guid.NewGuid(), "Free WiFi")
            },
            4.2m,
            50,
            DateTime.UtcNow.AddMonths(-6));
    }

    #endregion
}
