using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TAABP.Application.Common;
using TAABP.Application.Common.Errors;
using TAABP.Application.DTOs.Reviews;
using TAABP.Application.Features.Reviews.Queries.GetHotelReviews;
using TAABP.Application.Interfaces.Repositories;
using TAABP.Domain.Entities;

namespace TAABP.Application.Tests.Unit.Features.Reviews.Queries;

public class GetHotelReviewsQueryHandlerTests
{
    private readonly Mock<IHotelRepository> _mockHotelRepository;
    private readonly Mock<IReviewRepository> _mockReviewRepository;
    private readonly ILogger<GetHotelReviewsQueryHandler> _logger;
    private readonly GetHotelReviewsQueryHandler _handler;

    public GetHotelReviewsQueryHandlerTests()
    {
        _mockHotelRepository = new Mock<IHotelRepository>();
        _mockReviewRepository = new Mock<IReviewRepository>();
        _logger = NullLogger<GetHotelReviewsQueryHandler>.Instance;

        _handler = new GetHotelReviewsQueryHandler(
            _mockHotelRepository.Object,
            _mockReviewRepository.Object,
            _logger);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidHotelId_ReturnsSuccess()
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var query = new GetHotelReviewsQuery(hotelId);
        var hotel = CreateHotel(hotelId);
        var pagedResult = CreatePagedReviews(3, 1, 10);

        _mockHotelRepository
            .Setup(r => r.GetByIdAsync(hotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotel);
        _mockReviewRepository
            .Setup(r => r.GetHotelReviewsAsync(hotelId, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_WithNoReviews_ReturnsEmptyList()
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var query = new GetHotelReviewsQuery(hotelId);
        var hotel = CreateHotel(hotelId);
        var emptyResult = new PagedResult<ReviewDto>(new List<ReviewDto>(), 1, 10, 0);

        _mockHotelRepository
            .Setup(r => r.GetByIdAsync(hotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotel);
        _mockReviewRepository
            .Setup(r => r.GetHotelReviewsAsync(hotelId, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithCustomPagination_UsesProvidedPageAndSize()
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var page = 2;
        var pageSize = 5;
        var query = new GetHotelReviewsQuery(hotelId, page, pageSize);
        var hotel = CreateHotel(hotelId);
        var pagedResult = CreatePagedReviews(5, page, pageSize, 12);

        _mockHotelRepository
            .Setup(r => r.GetByIdAsync(hotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotel);
        _mockReviewRepository
            .Setup(r => r.GetHotelReviewsAsync(hotelId, page, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.PageNumber.Should().Be(page);
        result.Value.PageSize.Should().Be(pageSize);
        _mockReviewRepository.Verify(
            r => r.GetHotelReviewsAsync(hotelId, page, pageSize, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsCorrectPaginationInfo()
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var page = 1;
        var pageSize = 10;
        var totalCount = 25;
        var query = new GetHotelReviewsQuery(hotelId, page, pageSize);
        var hotel = CreateHotel(hotelId);
        var pagedResult = CreatePagedReviews(10, page, pageSize, totalCount);

        _mockHotelRepository
            .Setup(r => r.GetByIdAsync(hotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotel);
        _mockReviewRepository
            .Setup(r => r.GetHotelReviewsAsync(hotelId, page, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.PageNumber.Should().Be(page);
        result.Value.PageSize.Should().Be(pageSize);
        result.Value.TotalCount.Should().Be(totalCount);
        result.Value.TotalPages.Should().Be(3);
    }

    #endregion

    #region Not Found Cases

    [Fact]
    public async Task Handle_WithNonExistentHotel_ReturnsNotFoundError()
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var query = new GetHotelReviewsQuery(hotelId);

        _mockHotelRepository
            .Setup(r => r.GetByIdAsync(hotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Hotel?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(HotelErrors.NotFound(hotelId));
    }

    [Fact]
    public async Task Handle_WithNonExistentHotel_DoesNotCallReviewRepository()
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var query = new GetHotelReviewsQuery(hotelId);

        _mockHotelRepository
            .Setup(r => r.GetByIdAsync(hotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Hotel?)null);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockReviewRepository.Verify(
            r => r.GetHotelReviewsAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region Data Mapping Tests

    [Fact]
    public async Task Handle_ReturnsCorrectReviewData()
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var reviewId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow.AddDays(-5);
        var query = new GetHotelReviewsQuery(hotelId);
        var hotel = CreateHotel(hotelId);
        var review = new ReviewDto(
            reviewId, userId, "John D.", 5,
            "Excellent hotel!", createdAt);
        var pagedResult = new PagedResult<ReviewDto>(
            new List<ReviewDto> { review }, 1, 10, 1);

        _mockHotelRepository
            .Setup(r => r.GetByIdAsync(hotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotel);
        _mockReviewRepository
            .Setup(r => r.GetHotelReviewsAsync(hotelId, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var returnedReview = result.Value.Items.First();
        returnedReview.Id.Should().Be(reviewId);
        returnedReview.UserId.Should().Be(userId);
        returnedReview.UserName.Should().Be("John D.");
        returnedReview.Rating.Should().Be(5);
        returnedReview.Content.Should().Be("Excellent hotel!");
        returnedReview.CreatedAt.Should().Be(createdAt);
    }

    #endregion

    #region Cancellation Tests

    [Fact]
    public async Task Handle_WhenCancelled_PropagatesCancellation()
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var query = new GetHotelReviewsQuery(hotelId);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _mockHotelRepository
            .Setup(r => r.GetByIdAsync(hotelId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _handler.Handle(query, cts.Token));
    }

    #endregion

    #region Default Values Tests

    [Fact]
    public async Task Handle_WithDefaultPagination_UsesDefaultValues()
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var query = new GetHotelReviewsQuery(hotelId);
        var hotel = CreateHotel(hotelId);
        var pagedResult = CreatePagedReviews(5, 1, 10);

        _mockHotelRepository
            .Setup(r => r.GetByIdAsync(hotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotel);
        _mockReviewRepository
            .Setup(r => r.GetHotelReviewsAsync(hotelId, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockReviewRepository.Verify(
            r => r.GetHotelReviewsAsync(hotelId, 1, 10, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Helper Methods

    private static Hotel CreateHotel(Guid hotelId) => new()
    {
        Id = hotelId,
        Name = "Test Hotel",
        Description = "A test hotel",
        CityId = Guid.NewGuid(),
        StarRating = 4
    };

    private static PagedResult<ReviewDto> CreatePagedReviews(int count, int page, int pageSize, int? totalCount = null)
    {
        var reviews = Enumerable.Range(1, count)
            .Select(i => new ReviewDto(
                Guid.NewGuid(),
                Guid.NewGuid(),
                $"User{i} N.",
                Math.Min(i, 5),
                $"Review content {i}",
                DateTime.UtcNow.AddDays(-i)))
            .ToList();

        return new PagedResult<ReviewDto>(reviews, page, pageSize, totalCount ?? count);
    }

    #endregion
}
