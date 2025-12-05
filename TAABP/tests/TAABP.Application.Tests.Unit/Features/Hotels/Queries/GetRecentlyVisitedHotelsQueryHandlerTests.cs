using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using TAABP.Application.Common.Errors;
using TAABP.Application.DTOs.Hotels;
using TAABP.Application.Features.Hotels.Queries.GetRecentlyVisitedHotels;
using TAABP.Application.Interfaces.Repositories;
using TAABP.Application.Interfaces.Services;

namespace TAABP.Application.Tests.Unit.Features.Hotels.Queries;

public class GetRecentlyVisitedHotelsQueryHandlerTests
{
    private readonly Mock<IRecentlyVisitedHotelRepository> _mockRepository;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly ILogger<GetRecentlyVisitedHotelsQueryHandler> _logger;
    private readonly GetRecentlyVisitedHotelsQueryHandler _handler;

    public GetRecentlyVisitedHotelsQueryHandlerTests()
    {
        _mockRepository = new Mock<IRecentlyVisitedHotelRepository>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _logger = LoggerFactory.Create(b => b.AddConsole())
            .CreateLogger<GetRecentlyVisitedHotelsQueryHandler>();

        _handler = new GetRecentlyVisitedHotelsQueryHandler(
            _mockRepository.Object,
            _mockCurrentUserService.Object,
            _logger);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithAuthenticatedUser_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetRecentlyVisitedHotelsQuery();
        var hotels = new List<RecentlyVisitedHotelDto>
        {
            CreateRecentlyVisitedHotelDto("Hotel A", "Dubai", 5, 500m),
            CreateRecentlyVisitedHotelDto("Hotel B", "Abu Dhabi", 4, 350m)
        };

        _mockCurrentUserService.Setup(s => s.UserId).Returns(userId);
        _mockRepository
            .Setup(r => r.GetByUserIdAsync(userId, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotels);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);

        _mockRepository.Verify(
            r => r.GetByUserIdAsync(userId, 5, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithCustomCount_PassesCountToRepository()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetRecentlyVisitedHotelsQuery(Count: 10);
        var hotels = new List<RecentlyVisitedHotelDto>();

        _mockCurrentUserService.Setup(s => s.UserId).Returns(userId);
        _mockRepository
            .Setup(r => r.GetByUserIdAsync(userId, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotels);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockRepository.Verify(
            r => r.GetByUserIdAsync(userId, 10, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNoVisitedHotels_ReturnsEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetRecentlyVisitedHotelsQuery();
        var hotels = new List<RecentlyVisitedHotelDto>();

        _mockCurrentUserService.Setup(s => s.UserId).Returns(userId);
        _mockRepository
            .Setup(r => r.GetByUserIdAsync(userId, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotels);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(20)]
    public async Task Handle_WithVariousCounts_PassesCorrectCountToRepository(int count)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetRecentlyVisitedHotelsQuery(Count: count);
        var hotels = new List<RecentlyVisitedHotelDto>();

        _mockCurrentUserService.Setup(s => s.UserId).Returns(userId);
        _mockRepository
            .Setup(r => r.GetByUserIdAsync(userId, count, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotels);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockRepository.Verify(
            r => r.GetByUserIdAsync(userId, count, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsHotelsWithCorrectProperties()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetRecentlyVisitedHotelsQuery();
        var expectedHotelId = Guid.NewGuid();
        var visitedAt = DateTime.UtcNow.AddHours(-2);
        var hotels = new List<RecentlyVisitedHotelDto>
        {
            new RecentlyVisitedHotelDto(
                expectedHotelId,
                "Luxury Hotel",
                "Dubai",
                5,
                "https://example.com/thumb.jpg",
                1000m,
                visitedAt)
        };

        _mockCurrentUserService.Setup(s => s.UserId).Returns(userId);
        _mockRepository
            .Setup(r => r.GetByUserIdAsync(userId, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotels);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);

        var hotel = result.Value.First();
        hotel.HotelId.Should().Be(expectedHotelId);
        hotel.HotelName.Should().Be("Luxury Hotel");
        hotel.CityName.Should().Be("Dubai");
        hotel.StarRating.Should().Be(5);
        hotel.ThumbnailUrl.Should().Be("https://example.com/thumb.jpg");
        hotel.PriceStartingFrom.Should().Be(1000m);
        hotel.VisitedAt.Should().Be(visitedAt);
    }

    #endregion

    #region Unauthorized Cases

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var query = new GetRecentlyVisitedHotelsQuery();

        _mockCurrentUserService.Setup(s => s.UserId).Returns((Guid?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.Unauthorized);

        _mockRepository.Verify(
            r => r.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region Cancellation Tests

    [Fact]
    public async Task Handle_WhenCancelled_PassesCancellationToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetRecentlyVisitedHotelsQuery();
        var cts = new CancellationTokenSource();
        var hotels = new List<RecentlyVisitedHotelDto>();

        _mockCurrentUserService.Setup(s => s.UserId).Returns(userId);
        _mockRepository
            .Setup(r => r.GetByUserIdAsync(userId, 5, cts.Token))
            .ReturnsAsync(hotels);

        // Act
        var result = await _handler.Handle(query, cts.Token);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockRepository.Verify(
            r => r.GetByUserIdAsync(userId, 5, cts.Token),
            Times.Once);
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task Handle_CallsRepositoryExactlyOnce()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetRecentlyVisitedHotelsQuery();
        var hotels = new List<RecentlyVisitedHotelDto>();

        _mockCurrentUserService.Setup(s => s.UserId).Returns(userId);
        _mockRepository
            .Setup(r => r.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotels);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockRepository.Verify(
            r => r.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DoesNotModifyRepositoryData()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetRecentlyVisitedHotelsQuery();
        var hotels = new List<RecentlyVisitedHotelDto>
        {
            CreateRecentlyVisitedHotelDto("Hotel A", "Dubai", 5, 500m)
        };

        _mockCurrentUserService.Setup(s => s.UserId).Returns(userId);
        _mockRepository
            .Setup(r => r.GetByUserIdAsync(userId, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotels);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockRepository.Verify(
            r => r.Add(It.IsAny<Domain.Entities.RecentlyVisitedHotel>()),
            Times.Never);
        _mockRepository.Verify(
            r => r.Remove(It.IsAny<Domain.Entities.RecentlyVisitedHotel>()),
            Times.Never);
    }

    #endregion

    #region Helper Methods

    private static RecentlyVisitedHotelDto CreateRecentlyVisitedHotelDto(
        string hotelName,
        string cityName,
        int starRating,
        decimal priceStartingFrom)
    {
        return new RecentlyVisitedHotelDto(
            Guid.NewGuid(),
            hotelName,
            cityName,
            starRating,
            null,
            priceStartingFrom,
            DateTime.UtcNow);
    }

    #endregion
}
