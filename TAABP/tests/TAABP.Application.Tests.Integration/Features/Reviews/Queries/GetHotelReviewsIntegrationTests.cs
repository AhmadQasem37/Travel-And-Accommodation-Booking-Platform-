using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TAABP.Application.Tests.Integration.Fixtures;
using TAABP.Domain.Entities;
using TAABP.Domain.Enums;
using TAABP.Infrastructure.Persistence.Context;
using TAABP.Infrastructure.Persistence.Repositories;

namespace TAABP.Application.Tests.Integration.Features.Reviews.Queries;

public class GetHotelReviewsIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public GetHotelReviewsIntegrationTests(DatabaseFixture fixture)
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
    public async Task GetHotelReviewsAsync_WithNonExistentHotel_ReturnsEmptyResult()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var nonExistentId = Guid.NewGuid();
        var repository = new ReviewRepository(context);

        // Act
        var result = await repository.GetHotelReviewsAsync(
            nonExistentId, 1, 10, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetHotelReviewsAsync_WithHotelNoReviews_ReturnsEmptyResult()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Grand Hotel");
        var repository = new ReviewRepository(context);

        // Act
        var result = await repository.GetHotelReviewsAsync(
            hotel.Id, 1, 10, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetHotelReviewsAsync_WithReviews_ReturnsReviews()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Grand Hotel");
        var user = await SeedUserAsync(context, "john", "John", "Doe");
        await SeedReviewAsync(context, hotel.Id, user.Id, 5, "Excellent!");
        var repository = new ReviewRepository(context);

        // Act
        var result = await repository.GetHotelReviewsAsync(
            hotel.Id, 1, 10, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
        result.Items.First().Rating.Should().Be(5);
        result.Items.First().Content.Should().Be("Excellent!");
    }

    #endregion

    #region Pagination Tests

    [Fact]
    public async Task GetHotelReviewsAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Grand Hotel");

        // Seed 15 reviews
        for (int i = 1; i <= 15; i++)
        {
            var user = await SeedUserAsync(context, $"user{i}", $"User{i}", "Test");
            await SeedReviewAsync(context, hotel.Id, user.Id, (i % 5) + 1, $"Review {i}");
        }
        var repository = new ReviewRepository(context);

        // Act
        var result = await repository.GetHotelReviewsAsync(
            hotel.Id, 2, 5, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(5);
        result.TotalCount.Should().Be(15);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(5);
    }

    [Fact]
    public async Task GetHotelReviewsAsync_WithLastPage_ReturnsRemainingItems()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Grand Hotel");

        // Seed 12 reviews
        for (int i = 1; i <= 12; i++)
        {
            var user = await SeedUserAsync(context, $"user{i}", $"User{i}", "Test");
            await SeedReviewAsync(context, hotel.Id, user.Id, 4, $"Review {i}");
        }
        var repository = new ReviewRepository(context);

        // Act - Get page 3 with pageSize 5 (should have 2 items)
        var result = await repository.GetHotelReviewsAsync(
            hotel.Id, 3, 5, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(12);
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task GetHotelReviewsAsync_WithPageBeyondTotal_ReturnsEmpty()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Grand Hotel");
        var user = await SeedUserAsync(context, "john", "John", "Doe");
        await SeedReviewAsync(context, hotel.Id, user.Id, 5, "Great!");
        var repository = new ReviewRepository(context);

        // Act
        var result = await repository.GetHotelReviewsAsync(
            hotel.Id, 10, 10, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(1);
    }

    #endregion

    #region Sorting Tests

    [Fact]
    public async Task GetHotelReviewsAsync_SortsByCreatedAtDescending()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Grand Hotel");

        var user1 = await SeedUserAsync(context, "user1", "User1", "Test");
        var user2 = await SeedUserAsync(context, "user2", "User2", "Test");
        var user3 = await SeedUserAsync(context, "user3", "User3", "Test");

        await SeedReviewAsync(context, hotel.Id, user1.Id, 3, "First", DateTime.UtcNow.AddDays(-3));
        await SeedReviewAsync(context, hotel.Id, user2.Id, 4, "Second", DateTime.UtcNow.AddDays(-1));
        await SeedReviewAsync(context, hotel.Id, user3.Id, 5, "Third", DateTime.UtcNow);
        var repository = new ReviewRepository(context);

        // Act
        var result = await repository.GetHotelReviewsAsync(
            hotel.Id, 1, 10, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(3);
        result.Items[0].Content.Should().Be("Third"); // Most recent first
        result.Items[1].Content.Should().Be("Second");
        result.Items[2].Content.Should().Be("First");
    }

    #endregion

    #region User Name Format Tests

    [Fact]
    public async Task GetHotelReviewsAsync_FormatsUserNameCorrectly()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Grand Hotel");
        var user = await SeedUserAsync(context, "johndoe", "John", "Doe");
        await SeedReviewAsync(context, hotel.Id, user.Id, 5, "Great!");
        var repository = new ReviewRepository(context);

        // Act
        var result = await repository.GetHotelReviewsAsync(
            hotel.Id, 1, 10, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().UserName.Should().Be("John D.");
    }

    #endregion

    #region Review Data Tests

    [Fact]
    public async Task GetHotelReviewsAsync_ReturnsCorrectReviewData()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Grand Hotel");
        var user = await SeedUserAsync(context, "johndoe", "John", "Doe");
        var createdAt = DateTime.UtcNow.AddDays(-5);
        var review = await SeedReviewAsync(context, hotel.Id, user.Id, 5,
            "The hotel exceeded all expectations!", createdAt);
        var repository = new ReviewRepository(context);

        // Act
        var result = await repository.GetHotelReviewsAsync(
            hotel.Id, 1, 10, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        var returnedReview = result.Items.First();
        returnedReview.Id.Should().Be(review.Id);
        returnedReview.UserId.Should().Be(user.Id);
        returnedReview.UserName.Should().Be("John D.");
        returnedReview.Rating.Should().Be(5);
        returnedReview.Content.Should().Be("The hotel exceeded all expectations!");
        returnedReview.CreatedAt.Should().BeCloseTo(createdAt, TimeSpan.FromSeconds(1));
    }

    #endregion

    #region Hotel Isolation Tests

    [Fact]
    public async Task GetHotelReviewsAsync_DoesNotReturnReviewsFromOtherHotels()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel1 = await SeedHotelAsync(context, city.Id, "Hotel 1");
        var hotel2 = await SeedHotelAsync(context, city.Id, "Hotel 2");

        var user1 = await SeedUserAsync(context, "user1", "User1", "Test");
        var user2 = await SeedUserAsync(context, "user2", "User2", "Test");

        await SeedReviewAsync(context, hotel1.Id, user1.Id, 5, "Hotel 1 review");
        await SeedReviewAsync(context, hotel2.Id, user2.Id, 3, "Hotel 2 review");
        var repository = new ReviewRepository(context);

        // Act
        var result = await repository.GetHotelReviewsAsync(
            hotel1.Id, 1, 10, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
        result.Items.First().Content.Should().Be("Hotel 1 review");
    }

    #endregion

    #region Cancellation Tests

    [Fact]
    public async Task GetHotelReviewsAsync_WhenCancelled_ThrowsOperationCancelledException()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var repository = new ReviewRepository(context);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => repository.GetHotelReviewsAsync(Guid.NewGuid(), 1, 10, cts.Token));
    }

    #endregion

    #region Helper Methods

    private static async Task<City> SeedCityAsync(ApplicationDbContext context, string name, string country)
    {
        var city = new City
        {
            Id = Guid.NewGuid(),
            Name = name,
            Country = country,
            PostOffice = "12345",
            CreatedAt = DateTime.UtcNow
        };
        context.Cities.Add(city);
        await context.SaveChangesAsync();
        return city;
    }

    private static async Task<Hotel> SeedHotelAsync(ApplicationDbContext context, Guid cityId, string name)
    {
        var hotel = new Hotel
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = "A test hotel",
            CityId = cityId,
            StarRating = 4,
            CreatedAt = DateTime.UtcNow
        };
        context.Hotels.Add(hotel);
        await context.SaveChangesAsync();
        return hotel;
    }

    private static async Task<User> SeedUserAsync(
        ApplicationDbContext context,
        string username,
        string firstName,
        string lastName)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = $"{username}@test.com",
            FirstName = firstName,
            LastName = lastName,
            PasswordHash = "hash",
            PasswordSalt = "salt",
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    private static async Task<Review> SeedReviewAsync(
        ApplicationDbContext context,
        Guid hotelId,
        Guid userId,
        int rating,
        string content,
        DateTime? createdAt = null)
    {
        var review = new Review
        {
            Id = Guid.NewGuid(),
            HotelId = hotelId,
            UserId = userId,
            Rating = rating,
            Content = content,
            CreatedAt = createdAt ?? DateTime.UtcNow
        };
        context.Reviews.Add(review);
        await context.SaveChangesAsync();
        return review;
    }

    #endregion
}
