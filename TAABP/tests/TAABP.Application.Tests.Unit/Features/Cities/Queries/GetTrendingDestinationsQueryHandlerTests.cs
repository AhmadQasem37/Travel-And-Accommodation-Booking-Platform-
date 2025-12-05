using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using TAABP.Application.DTOs.Cities;
using TAABP.Application.Features.Cities.Queries.GetTrendingDestinations;
using TAABP.Application.Interfaces.Repositories;

namespace TAABP.Application.Tests.Unit.Features.Cities.Queries;

public class GetTrendingDestinationsQueryHandlerTests
{
    private readonly Mock<ICityRepository> _mockRepository;
    private readonly ILogger<GetTrendingDestinationsQueryHandler> _logger;
    private readonly GetTrendingDestinationsQueryHandler _handler;

    public GetTrendingDestinationsQueryHandlerTests()
    {
        _mockRepository = new Mock<ICityRepository>();
        _logger = LoggerFactory.Create(b => b.AddConsole())
            .CreateLogger<GetTrendingDestinationsQueryHandler>();

        _handler = new GetTrendingDestinationsQueryHandler(
            _mockRepository.Object,
            _logger);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithDefaultCount_ReturnsSuccess()
    {
        // Arrange
        var query = new GetTrendingDestinationsQuery();
        var destinations = new List<TrendingDestinationDto>
        {
            CreateTrendingDestinationDto("Dubai", "UAE", 15420),
            CreateTrendingDestinationDto("Paris", "France", 12300)
        };

        _mockRepository
            .Setup(r => r.GetTrendingDestinationsAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(destinations);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);

        _mockRepository.Verify(
            r => r.GetTrendingDestinationsAsync(5, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithCustomCount_PassesCountToRepository()
    {
        // Arrange
        var query = new GetTrendingDestinationsQuery(Count: 10);
        var destinations = new List<TrendingDestinationDto>();

        _mockRepository
            .Setup(r => r.GetTrendingDestinationsAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(destinations);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockRepository.Verify(
            r => r.GetTrendingDestinationsAsync(10, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNoTrendingDestinations_ReturnsEmptyList()
    {
        // Arrange
        var query = new GetTrendingDestinationsQuery();
        var destinations = new List<TrendingDestinationDto>();

        _mockRepository
            .Setup(r => r.GetTrendingDestinationsAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(destinations);

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
        var query = new GetTrendingDestinationsQuery(Count: count);
        var destinations = new List<TrendingDestinationDto>();

        _mockRepository
            .Setup(r => r.GetTrendingDestinationsAsync(count, It.IsAny<CancellationToken>()))
            .ReturnsAsync(destinations);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockRepository.Verify(
            r => r.GetTrendingDestinationsAsync(count, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsDestinationsWithCorrectProperties()
    {
        // Arrange
        var query = new GetTrendingDestinationsQuery();
        var expectedCityId = Guid.NewGuid();
        var destinations = new List<TrendingDestinationDto>
        {
            new TrendingDestinationDto(
                expectedCityId,
                "Dubai",
                "UAE",
                "https://example.com/dubai.jpg",
                15420)
        };

        _mockRepository
            .Setup(r => r.GetTrendingDestinationsAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(destinations);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);

        var destination = result.Value.First();
        destination.CityId.Should().Be(expectedCityId);
        destination.CityName.Should().Be("Dubai");
        destination.Country.Should().Be("UAE");
        destination.ThumbnailUrl.Should().Be("https://example.com/dubai.jpg");
        destination.VisitCount.Should().Be(15420);
    }

    [Fact]
    public async Task Handle_WithNullThumbnailUrl_ReturnsSuccessfully()
    {
        // Arrange
        var query = new GetTrendingDestinationsQuery();
        var destinations = new List<TrendingDestinationDto>
        {
            new TrendingDestinationDto(
                Guid.NewGuid(),
                "Dubai",
                "UAE",
                null,
                15420)
        };

        _mockRepository
            .Setup(r => r.GetTrendingDestinationsAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(destinations);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.First().ThumbnailUrl.Should().BeNull();
    }

    #endregion

    #region Cancellation Tests

    [Fact]
    public async Task Handle_WhenCancelled_PassesCancellationToken()
    {
        // Arrange
        var query = new GetTrendingDestinationsQuery();
        var cts = new CancellationTokenSource();
        var destinations = new List<TrendingDestinationDto>();

        _mockRepository
            .Setup(r => r.GetTrendingDestinationsAsync(5, cts.Token))
            .ReturnsAsync(destinations);

        // Act
        var result = await _handler.Handle(query, cts.Token);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockRepository.Verify(
            r => r.GetTrendingDestinationsAsync(5, cts.Token),
            Times.Once);
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task Handle_CallsRepositoryExactlyOnce()
    {
        // Arrange
        var query = new GetTrendingDestinationsQuery();
        var destinations = new List<TrendingDestinationDto>();

        _mockRepository
            .Setup(r => r.GetTrendingDestinationsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(destinations);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockRepository.Verify(
            r => r.GetTrendingDestinationsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Helper Methods

    private static TrendingDestinationDto CreateTrendingDestinationDto(
        string cityName,
        string country,
        int visitCount)
    {
        return new TrendingDestinationDto(
            Guid.NewGuid(),
            cityName,
            country,
            null,
            visitCount);
    }

    #endregion
}
