using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TAABP.Application.Features.Reviews.Commands.CreateReview;
using TAABP.Application.Interfaces;
using TAABP.Application.Interfaces.Repositories;
using TAABP.Application.Interfaces.Services;
using TAABP.Domain.Entities;
using Xunit;

namespace TAABP.Application.Tests.Unit.Features.Reviews.Commands;

public sealed class CreateReviewCommandHandlerTests
{
    private readonly Mock<IHotelRepository> _hotelRepositoryMock;
    private readonly Mock<IReviewRepository> _reviewRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ILogger<CreateReviewCommandHandler>> _loggerMock;
    private readonly CreateReviewCommandHandler _handler;

    private static readonly Guid TestHotelId = Guid.Parse("66666666-6666-6666-6666-666666666666");
    private static readonly Guid TestUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public CreateReviewCommandHandlerTests()
    {
        _hotelRepositoryMock = new Mock<IHotelRepository>();
        _reviewRepositoryMock = new Mock<IReviewRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _loggerMock = new Mock<ILogger<CreateReviewCommandHandler>>();

        _currentUserServiceMock.Setup(x => x.UserId).Returns(TestUserId);

        _handler = new CreateReviewCommandHandler(
            _hotelRepositoryMock.Object,
            _reviewRepositoryMock.Object,
            _currentUserServiceMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateReviewAndReturnSuccess()
    {
        // Arrange
        var command = new CreateReviewCommand(TestHotelId, 5, "Amazing hotel! Great service and beautiful rooms. Highly recommended for everyone.");
        var hotel = new Hotel { Id = TestHotelId, Name = "Test Hotel" };

        _hotelRepositoryMock.Setup(x => x.GetByIdAsync(TestHotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotel);

        _reviewRepositoryMock.Setup(x => x.GetByUserAndHotelAsync(TestUserId, TestHotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Review?)null);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _reviewRepositoryMock.Verify(x => x.Add(
            It.Is<Review>(r =>
                r.HotelId == TestHotelId &&
                r.UserId == TestUserId &&
                r.Rating == 5 &&
                r.Content == command.Content)), Times.Once);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_HotelNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateReviewCommand(TestHotelId, 4, "This review should fail because hotel doesn't exist.");

        _hotelRepositoryMock.Setup(x => x.GetByIdAsync(TestHotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Hotel?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Hotel.NotFound");

        _reviewRepositoryMock.Verify(x => x.Add(It.IsAny<Review>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UserAlreadyReviewedHotel_ShouldReturnConflictError()
    {
        // Arrange
        var command = new CreateReviewCommand(TestHotelId, 3, "This is a duplicate review that should fail.");
        var hotel = new Hotel { Id = TestHotelId, Name = "Test Hotel" };
        var existingReview = new Review
        {
            Id = Guid.NewGuid(),
            HotelId = TestHotelId,
            UserId = TestUserId,
            Rating = 4,
            Content = "Existing review"
        };

        _hotelRepositoryMock.Setup(x => x.GetByIdAsync(TestHotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotel);

        _reviewRepositoryMock.Setup(x => x.GetByUserAndHotelAsync(TestUserId, TestHotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingReview);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Review.AlreadyReviewed");

        _reviewRepositoryMock.Verify(x => x.Add(It.IsAny<Review>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public async Task Handle_AllValidRatings_ShouldCreateReview(int rating)
    {
        // Arrange
        var command = new CreateReviewCommand(TestHotelId, rating, "Testing all valid rating values from 1 to 5.");
        var hotel = new Hotel { Id = TestHotelId, Name = "Test Hotel" };

        _hotelRepositoryMock.Setup(x => x.GetByIdAsync(TestHotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotel);

        _reviewRepositoryMock.Setup(x => x.GetByUserAndHotelAsync(TestUserId, TestHotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Review?)null);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _reviewRepositoryMock.Verify(x => x.Add(
            It.Is<Review>(r => r.Rating == rating)), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSetCorrectReviewProperties()
    {
        // Arrange
        var command = new CreateReviewCommand(TestHotelId, 5, "Verifying all review properties are set correctly.");
        var hotel = new Hotel { Id = TestHotelId, Name = "Test Hotel" };
        Review? capturedReview = null;

        _hotelRepositoryMock.Setup(x => x.GetByIdAsync(TestHotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotel);

        _reviewRepositoryMock.Setup(x => x.GetByUserAndHotelAsync(TestUserId, TestHotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Review?)null);

        _reviewRepositoryMock.Setup(x => x.Add(It.IsAny<Review>()))
            .Callback<Review>(review => capturedReview = review);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedReview.Should().NotBeNull();
        capturedReview!.Id.Should().NotBe(Guid.Empty);
        capturedReview.HotelId.Should().Be(TestHotelId);
        capturedReview.UserId.Should().Be(TestUserId);
        capturedReview.Rating.Should().Be(5);
        capturedReview.Content.Should().Be(command.Content);
    }

    [Fact]
    public async Task Handle_ShouldRespectCancellationToken()
    {
        // Arrange
        var command = new CreateReviewCommand(TestHotelId, 4, "This operation should respect cancellation token.");
        var cancellationToken = new CancellationToken(true);

        _hotelRepositoryMock.Setup(x => x.GetByIdAsync(TestHotelId, cancellationToken))
            .ThrowsAsync(new OperationCanceledException(cancellationToken));

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _handler.Handle(command, cancellationToken));
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToRepositoryMethods()
    {
        // Arrange
        var command = new CreateReviewCommand(TestHotelId, 4, "Verifying cancellation token propagation to all methods.");
        var hotel = new Hotel { Id = TestHotelId, Name = "Test Hotel" };
        var cancellationToken = new CancellationToken();

        _hotelRepositoryMock.Setup(x => x.GetByIdAsync(TestHotelId, cancellationToken))
            .ReturnsAsync(hotel);

        _reviewRepositoryMock.Setup(x => x.GetByUserAndHotelAsync(TestUserId, TestHotelId, cancellationToken))
            .ReturnsAsync((Review?)null);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        _hotelRepositoryMock.Verify(x => x.GetByIdAsync(TestHotelId, cancellationToken), Times.Once);
        _reviewRepositoryMock.Verify(x => x.GetByUserAndHotelAsync(TestUserId, TestHotelId, cancellationToken), Times.Once);
        _reviewRepositoryMock.Verify(x => x.Add(It.IsAny<Review>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotCallSaveChanges_WhenHotelNotFound()
    {
        // Arrange
        var command = new CreateReviewCommand(Guid.NewGuid(), 4, "Testing that save is not called when hotel not found.");

        _hotelRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Hotel?)null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldNotCallSaveChanges_WhenAlreadyReviewed()
    {
        // Arrange
        var command = new CreateReviewCommand(TestHotelId, 4, "Testing that save is not called when already reviewed.");
        var hotel = new Hotel { Id = TestHotelId, Name = "Test Hotel" };
        var existingReview = new Review { Id = Guid.NewGuid(), HotelId = TestHotelId, UserId = TestUserId };

        _hotelRepositoryMock.Setup(x => x.GetByIdAsync(TestHotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotel);

        _reviewRepositoryMock.Setup(x => x.GetByUserAndHotelAsync(TestUserId, TestHotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingReview);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
