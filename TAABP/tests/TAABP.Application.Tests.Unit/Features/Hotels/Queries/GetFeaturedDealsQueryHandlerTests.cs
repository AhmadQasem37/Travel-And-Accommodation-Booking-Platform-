using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using TAABP.Application.DTOs.Hotels;
using TAABP.Application.Features.Hotels.Queries.GetFeaturedDeals;
using TAABP.Application.Interfaces.Repositories;

namespace TAABP.Application.Tests.Unit.Features.Hotels.Queries;

public class GetFeaturedDealsQueryHandlerTests
{
    private readonly Mock<IHotelRepository> _mockHotelRepository;
    private readonly ILogger<GetFeaturedDealsQueryHandler> _logger;
    private readonly GetFeaturedDealsQueryHandler _handler;

    public GetFeaturedDealsQueryHandlerTests()
    {
        _mockHotelRepository = new Mock<IHotelRepository>();
        _logger = LoggerFactory.Create(b => b.AddConsole())
            .CreateLogger<GetFeaturedDealsQueryHandler>();

        _handler = new GetFeaturedDealsQueryHandler(
            _mockHotelRepository.Object,
            _logger);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithDefaultCount_ReturnsSuccess()
    {
        // Arrange
        var query = new GetFeaturedDealsQuery();
        var deals = new List<FeaturedDealDto>
        {
            CreateFeaturedDealDto("Hotel A", "Dubai", 5, 500m, 450m, 10),
            CreateFeaturedDealDto("Hotel B", "Abu Dhabi", 4, 350m, 297.5m, 15)
        };

        _mockHotelRepository
            .Setup(r => r.GetFeaturedDealsAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(deals);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);

        _mockHotelRepository.Verify(
            r => r.GetFeaturedDealsAsync(5, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithCustomCount_PassesCountToRepository()
    {
        // Arrange
        var query = new GetFeaturedDealsQuery(Count: 10);
        var deals = new List<FeaturedDealDto>();

        _mockHotelRepository
            .Setup(r => r.GetFeaturedDealsAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(deals);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockHotelRepository.Verify(
            r => r.GetFeaturedDealsAsync(10, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNoDealsExist_ReturnsEmptyList()
    {
        // Arrange
        var query = new GetFeaturedDealsQuery();
        var deals = new List<FeaturedDealDto>();

        _mockHotelRepository
            .Setup(r => r.GetFeaturedDealsAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(deals);

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
        var query = new GetFeaturedDealsQuery(Count: count);
        var deals = new List<FeaturedDealDto>();

        _mockHotelRepository
            .Setup(r => r.GetFeaturedDealsAsync(count, It.IsAny<CancellationToken>()))
            .ReturnsAsync(deals);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockHotelRepository.Verify(
            r => r.GetFeaturedDealsAsync(count, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsDealsWithCorrectProperties()
    {
        // Arrange
        var query = new GetFeaturedDealsQuery();
        var expectedHotelId = Guid.NewGuid();
        var deals = new List<FeaturedDealDto>
        {
            new FeaturedDealDto(
                expectedHotelId,
                "Luxury Hotel",
                "Dubai",
                "UAE",
                5,
                "https://example.com/thumb.jpg",
                1000m,
                850m,
                15)
        };

        _mockHotelRepository
            .Setup(r => r.GetFeaturedDealsAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(deals);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);

        var deal = result.Value.First();
        deal.HotelId.Should().Be(expectedHotelId);
        deal.HotelName.Should().Be("Luxury Hotel");
        deal.CityName.Should().Be("Dubai");
        deal.Country.Should().Be("UAE");
        deal.StarRating.Should().Be(5);
        deal.ThumbnailUrl.Should().Be("https://example.com/thumb.jpg");
        deal.OriginalPrice.Should().Be(1000m);
        deal.DiscountedPrice.Should().Be(850m);
        deal.DiscountPercentage.Should().Be(15);
    }

    [Fact]
    public async Task Handle_WithMultipleDeals_ReturnsAllDeals()
    {
        // Arrange
        var query = new GetFeaturedDealsQuery(Count: 5);
        var deals = new List<FeaturedDealDto>
        {
            CreateFeaturedDealDto("Hotel 1", "City 1", 5, 500m, 400m, 20),
            CreateFeaturedDealDto("Hotel 2", "City 2", 4, 300m, 255m, 15),
            CreateFeaturedDealDto("Hotel 3", "City 3", 4, 250m, 225m, 10),
            CreateFeaturedDealDto("Hotel 4", "City 4", 3, 200m, 190m, 5),
            CreateFeaturedDealDto("Hotel 5", "City 5", 3, 150m, 147m, 2)
        };

        _mockHotelRepository
            .Setup(r => r.GetFeaturedDealsAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(deals);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(5);
    }

    #endregion

    #region Cancellation Tests

    [Fact]
    public async Task Handle_WhenCancelled_PassesCancellationToken()
    {
        // Arrange
        var query = new GetFeaturedDealsQuery();
        var cts = new CancellationTokenSource();
        var deals = new List<FeaturedDealDto>();

        _mockHotelRepository
            .Setup(r => r.GetFeaturedDealsAsync(5, cts.Token))
            .ReturnsAsync(deals);

        // Act
        var result = await _handler.Handle(query, cts.Token);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockHotelRepository.Verify(
            r => r.GetFeaturedDealsAsync(5, cts.Token),
            Times.Once);
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task Handle_CallsRepositoryExactlyOnce()
    {
        // Arrange
        var query = new GetFeaturedDealsQuery();
        var deals = new List<FeaturedDealDto>();

        _mockHotelRepository
            .Setup(r => r.GetFeaturedDealsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(deals);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockHotelRepository.Verify(
            r => r.GetFeaturedDealsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DoesNotModifyRepositoryData()
    {
        // Arrange
        var query = new GetFeaturedDealsQuery();
        var deals = new List<FeaturedDealDto>
        {
            CreateFeaturedDealDto("Hotel A", "Dubai", 5, 500m, 450m, 10)
        };

        _mockHotelRepository
            .Setup(r => r.GetFeaturedDealsAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(deals);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockHotelRepository.Verify(r => r.Add(It.IsAny<Domain.Entities.Hotel>()), Times.Never);
        _mockHotelRepository.Verify(r => r.Update(It.IsAny<Domain.Entities.Hotel>()), Times.Never);
        _mockHotelRepository.Verify(r => r.Remove(It.IsAny<Domain.Entities.Hotel>()), Times.Never);
    }

    #endregion

    #region Helper Methods

    private static FeaturedDealDto CreateFeaturedDealDto(
        string hotelName,
        string cityName,
        int starRating,
        decimal originalPrice,
        decimal discountedPrice,
        int discountPercentage)
    {
        return new FeaturedDealDto(
            Guid.NewGuid(),
            hotelName,
            cityName,
            "Test Country",
            starRating,
            null,
            originalPrice,
            discountedPrice,
            discountPercentage);
    }

    #endregion
}
