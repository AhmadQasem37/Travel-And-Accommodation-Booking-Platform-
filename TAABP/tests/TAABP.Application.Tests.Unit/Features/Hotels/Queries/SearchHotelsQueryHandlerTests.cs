using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using TAABP.Application.Common;
using TAABP.Application.DTOs.Hotels;
using TAABP.Application.Features.Hotels.Queries.SearchHotels;
using TAABP.Application.Interfaces.Repositories;
using TAABP.Domain.Enums;

namespace TAABP.Application.Tests.Unit.Features.Hotels.Queries;

public class SearchHotelsQueryHandlerTests
{
    private readonly Mock<IHotelRepository> _mockHotelRepository;
    private readonly ILogger<SearchHotelsQueryHandler> _logger;
    private readonly SearchHotelsQueryHandler _handler;

    public SearchHotelsQueryHandlerTests()
    {
        _mockHotelRepository = new Mock<IHotelRepository>();
        _logger = LoggerFactory.Create(b => b.AddConsole())
            .CreateLogger<SearchHotelsQueryHandler>();

        _handler = new SearchHotelsQueryHandler(
            _mockHotelRepository.Object,
            _logger);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithDefaultParameters_ReturnsSuccess()
    {
        // Arrange
        var query = new SearchHotelsQuery();
        var hotels = new List<SearchHotelDto>
        {
            CreateSearchHotelDto("Hotel A", "Dubai", 5, 200m),
            CreateSearchHotelDto("Hotel B", "Dubai", 4, 150m)
        };
        var pagedResult = new PagedResult<SearchHotelDto>(hotels, 1, 10, 2);

        _mockHotelRepository
            .Setup(r => r.SearchAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(2);

        _mockHotelRepository.Verify(
            r => r.SearchAsync(query, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithSearchQuery_PassesQueryToRepository()
    {
        // Arrange
        var query = new SearchHotelsQuery(SearchQuery: "Dubai");
        var pagedResult = new PagedResult<SearchHotelDto>(new List<SearchHotelDto>(), 1, 10, 0);

        _mockHotelRepository
            .Setup(r => r.SearchAsync(It.Is<SearchHotelsQuery>(q => q.SearchQuery == "Dubai"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockHotelRepository.Verify(
            r => r.SearchAsync(It.Is<SearchHotelsQuery>(q => q.SearchQuery == "Dubai"), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithDateRange_PassesDatesToRepository()
    {
        // Arrange
        var checkIn = DateOnly.FromDateTime(DateTime.Today.AddDays(7));
        var checkOut = DateOnly.FromDateTime(DateTime.Today.AddDays(10));
        var query = new SearchHotelsQuery(CheckInDate: checkIn, CheckOutDate: checkOut);
        var pagedResult = new PagedResult<SearchHotelDto>(new List<SearchHotelDto>(), 1, 10, 0);

        _mockHotelRepository
            .Setup(r => r.SearchAsync(
                It.Is<SearchHotelsQuery>(q => q.CheckInDate == checkIn && q.CheckOutDate == checkOut),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockHotelRepository.Verify(
            r => r.SearchAsync(
                It.Is<SearchHotelsQuery>(q => q.CheckInDate == checkIn && q.CheckOutDate == checkOut),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithPriceRange_PassesPricesToRepository()
    {
        // Arrange
        var query = new SearchHotelsQuery(MinPrice: 100m, MaxPrice: 500m);
        var pagedResult = new PagedResult<SearchHotelDto>(new List<SearchHotelDto>(), 1, 10, 0);

        _mockHotelRepository
            .Setup(r => r.SearchAsync(
                It.Is<SearchHotelsQuery>(q => q.MinPrice == 100m && q.MaxPrice == 500m),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData(HotelSortBy.Price, SortOrder.Asc)]
    [InlineData(HotelSortBy.Price, SortOrder.Desc)]
    [InlineData(HotelSortBy.StarRating, SortOrder.Asc)]
    [InlineData(HotelSortBy.StarRating, SortOrder.Desc)]
    public async Task Handle_WithSorting_PassesSortParametersToRepository(HotelSortBy sortBy, SortOrder sortOrder)
    {
        // Arrange
        var query = new SearchHotelsQuery(SortBy: sortBy, SortOrder: sortOrder);
        var pagedResult = new PagedResult<SearchHotelDto>(new List<SearchHotelDto>(), 1, 10, 0);

        _mockHotelRepository
            .Setup(r => r.SearchAsync(
                It.Is<SearchHotelsQuery>(q => q.SortBy == sortBy && q.SortOrder == sortOrder),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData(1, 10)]
    [InlineData(2, 20)]
    [InlineData(5, 50)]
    public async Task Handle_WithPagination_PassesPaginationToRepository(int page, int pageSize)
    {
        // Arrange
        var query = new SearchHotelsQuery(Page: page, PageSize: pageSize);
        var pagedResult = new PagedResult<SearchHotelDto>(new List<SearchHotelDto>(), page, pageSize, 100);

        _mockHotelRepository
            .Setup(r => r.SearchAsync(
                It.Is<SearchHotelsQuery>(q => q.Page == page && q.PageSize == pageSize),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.PageNumber.Should().Be(page);
        result.Value.PageSize.Should().Be(pageSize);
    }

    [Fact]
    public async Task Handle_WithStarRatings_PassesStarRatingsToRepository()
    {
        // Arrange
        var starRatings = new[] { 4, 5 };
        var query = new SearchHotelsQuery(StarRatings: starRatings);
        var pagedResult = new PagedResult<SearchHotelDto>(new List<SearchHotelDto>(), 1, 10, 0);

        _mockHotelRepository
            .Setup(r => r.SearchAsync(
                It.Is<SearchHotelsQuery>(q => q.StarRatings != null && q.StarRatings.SequenceEqual(starRatings)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithGuestCount_PassesGuestCountToRepository()
    {
        // Arrange
        var query = new SearchHotelsQuery(Adults: 3, Children: 2, Rooms: 2);
        var pagedResult = new PagedResult<SearchHotelDto>(new List<SearchHotelDto>(), 1, 10, 0);

        _mockHotelRepository
            .Setup(r => r.SearchAsync(
                It.Is<SearchHotelsQuery>(q => q.Adults == 3 && q.Children == 2 && q.Rooms == 2),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Empty Results

    [Fact]
    public async Task Handle_WhenNoHotelsFound_ReturnsEmptyList()
    {
        // Arrange
        var query = new SearchHotelsQuery(SearchQuery: "NonExistentCity");
        var pagedResult = new PagedResult<SearchHotelDto>(new List<SearchHotelDto>(), 1, 10, 0);

        _mockHotelRepository
            .Setup(r => r.SearchAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    #endregion

    #region Helper Methods

    private static SearchHotelDto CreateSearchHotelDto(
        string name,
        string city,
        int starRating,
        decimal price,
        decimal? discountedPrice = null,
        int? discountPercentage = null)
    {
        return new SearchHotelDto(
            HotelId: Guid.NewGuid(),
            HotelName: name,
            CityName: city,
            Country: "UAE",
            StarRating: starRating,
            ThumbnailUrl: $"https://example.com/{name.ToLower().Replace(" ", "-")}.jpg",
            Description: $"A wonderful {starRating}-star hotel in {city}",
            PriceStartingFrom: price,
            DiscountedPrice: discountedPrice,
            DiscountPercentage: discountPercentage,
            Amenities: new List<string> { "WiFi", "Pool" });
    }

    #endregion
}
