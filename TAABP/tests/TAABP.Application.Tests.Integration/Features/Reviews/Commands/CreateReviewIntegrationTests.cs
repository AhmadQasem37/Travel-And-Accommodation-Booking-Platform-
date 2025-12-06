using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TAABP.Application.Features.Reviews.Commands.CreateReview;
using TAABP.Application.Interfaces;
using TAABP.Application.Interfaces.Services;
using TAABP.Domain.Entities;
using TAABP.Infrastructure.Persistence.Context;
using TAABP.Infrastructure.Persistence.Repositories;
using Xunit;

namespace TAABP.Application.Tests.Integration.Features.Reviews.Commands;

public sealed class CreateReviewIntegrationTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly HotelRepository _hotelRepository;
    private readonly ReviewRepository _reviewRepository;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ILogger<CreateReviewCommandHandler>> _loggerMock;

    private static readonly Guid TestHotelId = Guid.Parse("66666666-6666-6666-6666-666666666666");
    private static readonly Guid TestUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid TestCityId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public CreateReviewIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _hotelRepository = new HotelRepository(_context);
        _reviewRepository = new ReviewRepository(_context);
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _loggerMock = new Mock<ILogger<CreateReviewCommandHandler>>();

        _currentUserServiceMock.Setup(x => x.UserId).Returns(TestUserId);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var city = new City
        {
            Id = TestCityId,
            Name = "Test City",
            Country = "Test Country",
            PostOffice = "12345"
        };

        var hotel = new Hotel
        {
            Id = TestHotelId,
            Name = "Grand Test Hotel",
            Description = "A wonderful test hotel",
            StarRating = 5,
            CityId = TestCityId,
            Address = "123 Test Street"
        };

        _context.Cities.Add(city);
        _context.Hotels.Add(hotel);
        _context.SaveChanges();
    }

    private CreateReviewCommandHandler CreateHandler()
    {
        var unitOfWork = new UnitOfWork(_context);
        return new CreateReviewCommandHandler(
            _hotelRepository,
            _reviewRepository,
            _currentUserServiceMock.Object,
            unitOfWork,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldPersistReviewToDatabase()
    {
        // Arrange
        var handler = CreateHandler();
        var command = new CreateReviewCommand(
            TestHotelId,
            5,
            "This is an amazing hotel! Great service and beautiful rooms. Highly recommended.");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var savedReview = await _context.Reviews
            .FirstOrDefaultAsync(r => r.HotelId == TestHotelId && r.UserId == TestUserId);

        savedReview.Should().NotBeNull();
        savedReview!.Rating.Should().Be(5);
        savedReview.Content.Should().Be(command.Content);
        savedReview.HotelId.Should().Be(TestHotelId);
        savedReview.UserId.Should().Be(TestUserId);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldGenerateNewReviewId()
    {
        // Arrange
        var handler = CreateHandler();
        var command = new CreateReviewCommand(TestHotelId, 4, "Testing that a new review ID is generated correctly.");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var savedReview = await _context.Reviews.FirstOrDefaultAsync(r => r.HotelId == TestHotelId);
        savedReview.Should().NotBeNull();
        savedReview!.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_HotelNotFound_ShouldReturnFailureWithoutPersisting()
    {
        // Arrange
        var handler = CreateHandler();
        var nonExistentHotelId = Guid.NewGuid();
        var command = new CreateReviewCommand(
            nonExistentHotelId,
            4,
            "This review should not be created because hotel doesn't exist.");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Hotel.NotFound");

        var reviewCount = await _context.Reviews.CountAsync();
        reviewCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_UserAlreadyReviewedHotel_ShouldReturnConflict()
    {
        // Arrange
        var existingReview = new Review
        {
            Id = Guid.NewGuid(),
            HotelId = TestHotelId,
            UserId = TestUserId,
            Rating = 4,
            Content = "My original review of this hotel",
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };
        await _context.Reviews.AddAsync(existingReview);
        await _context.SaveChangesAsync();

        var handler = CreateHandler();
        var command = new CreateReviewCommand(
            TestHotelId,
            5,
            "This is a duplicate review that should fail.");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Review.AlreadyReviewed");

        var reviewCount = await _context.Reviews.CountAsync();
        reviewCount.Should().Be(1); // Only the original review
    }

    [Fact]
    public async Task Handle_SameUserDifferentHotels_ShouldAllowMultipleReviews()
    {
        // Arrange
        var secondHotelId = Guid.NewGuid();
        var secondHotel = new Hotel
        {
            Id = secondHotelId,
            Name = "Second Test Hotel",
            Description = "Another hotel",
            StarRating = 4,
            CityId = TestCityId,
            Address = "456 Test Avenue"
        };
        await _context.Hotels.AddAsync(secondHotel);
        await _context.SaveChangesAsync();

        var handler = CreateHandler();

        // Create review for first hotel
        var command1 = new CreateReviewCommand(TestHotelId, 5, "Great experience at the first hotel.");
        await handler.Handle(command1, CancellationToken.None);

        // Create review for second hotel
        var command2 = new CreateReviewCommand(secondHotelId, 4, "Good experience at the second hotel.");

        // Act
        var result = await handler.Handle(command2, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var reviews = await _context.Reviews.Where(r => r.UserId == TestUserId).ToListAsync();
        reviews.Should().HaveCount(2);
        reviews.Should().Contain(r => r.HotelId == TestHotelId);
        reviews.Should().Contain(r => r.HotelId == secondHotelId);
    }

    [Fact]
    public async Task Handle_DifferentUsersSameHotel_ShouldAllowMultipleReviews()
    {
        // Arrange
        var secondUserId = Guid.NewGuid();

        // First user creates review
        var handler1 = CreateHandler();
        var command1 = new CreateReviewCommand(TestHotelId, 5, "First user's review of this hotel.");
        await handler1.Handle(command1, CancellationToken.None);

        // Switch to second user
        _currentUserServiceMock.Setup(x => x.UserId).Returns(secondUserId);
        var handler2 = CreateHandler();
        var command2 = new CreateReviewCommand(TestHotelId, 4, "Second user's review of this hotel.");

        // Act
        var result = await handler2.Handle(command2, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var reviews = await _context.Reviews.Where(r => r.HotelId == TestHotelId).ToListAsync();
        reviews.Should().HaveCount(2);
        reviews.Should().Contain(r => r.UserId == TestUserId);
        reviews.Should().Contain(r => r.UserId == secondUserId);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public async Task Handle_AllValidRatings_ShouldPersistCorrectly(int rating)
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var hotel = new Hotel
        {
            Id = hotelId,
            Name = $"Hotel for rating {rating}",
            Description = "Test hotel",
            StarRating = 3,
            CityId = TestCityId,
            Address = "Test address"
        };
        await _context.Hotels.AddAsync(hotel);
        await _context.SaveChangesAsync();

        var handler = CreateHandler();
        var command = new CreateReviewCommand(hotelId, rating, $"Testing rating {rating} persists correctly.");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var savedReview = await _context.Reviews.FirstOrDefaultAsync(r => r.HotelId == hotelId);
        savedReview.Should().NotBeNull();
        savedReview!.Rating.Should().Be(rating);
    }

    [Fact]
    public async Task Handle_LongContent_ShouldPersistCorrectly()
    {
        // Arrange
        var handler = CreateHandler();
        var longContent = new string('A', 2000); // Max length content
        var command = new CreateReviewCommand(TestHotelId, 4, longContent);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var savedReview = await _context.Reviews.FirstOrDefaultAsync(r => r.HotelId == TestHotelId);
        savedReview.Should().NotBeNull();
        savedReview!.Content.Should().Be(longContent);
        savedReview.Content.Length.Should().Be(2000);
    }

    [Fact]
    public async Task Handle_ConcurrentReviewCreation_ShouldOnlyAllowOne()
    {
        // Arrange
        // Simulate checking the same hotel twice but with no review yet
        var handler = CreateHandler();
        var command = new CreateReviewCommand(TestHotelId, 5, "Testing concurrent review creation scenario.");

        // Act - Create first review
        var result1 = await handler.Handle(command, CancellationToken.None);

        // Try to create second review (simulating concurrent request)
        var result2 = await handler.Handle(command, CancellationToken.None);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsFailure.Should().BeTrue();
        result2.Error.Code.Should().Be("Review.AlreadyReviewed");

        var reviewCount = await _context.Reviews.CountAsync(r => r.HotelId == TestHotelId && r.UserId == TestUserId);
        reviewCount.Should().Be(1);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
