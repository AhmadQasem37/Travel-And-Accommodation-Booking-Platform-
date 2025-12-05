using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TAABP.Application.Features.Hotels.Queries.SearchHotels;
using TAABP.Application.Tests.Integration.Fixtures;
using TAABP.Domain.Entities;
using TAABP.Domain.Enums;
using TAABP.Infrastructure.Persistence.Context;
using TAABP.Infrastructure.Persistence.Repositories;

namespace TAABP.Application.Tests.Integration.Features.Hotels.Queries;

public class SearchHotelsIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public SearchHotelsIntegrationTests(DatabaseFixture fixture)
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

    #region Basic Search Tests

    [Fact]
    public async Task SearchAsync_WithNoFilters_ReturnsAllHotels()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedTestDataAsync(context);
        var repository = new HotelRepository(context);
        var query = new SearchHotelsQuery();

        // Act
        var result = await repository.SearchAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().NotBeEmpty();
        result.TotalCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task SearchAsync_WithQueryFilter_ReturnsMatchingHotels()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedTestDataAsync(context);
        var repository = new HotelRepository(context);
        var query = new SearchHotelsQuery(SearchQuery: "Dubai");

        // Act
        var result = await repository.SearchAsync(query, CancellationToken.None);

        // Assert
        result.Items.Should().OnlyContain(h => 
            h.HotelName.Contains("Dubai", StringComparison.OrdinalIgnoreCase) ||
            h.CityName.Contains("Dubai", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task SearchAsync_WithNonExistentQuery_ReturnsEmptyList()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedTestDataAsync(context);
        var repository = new HotelRepository(context);
        var query = new SearchHotelsQuery(SearchQuery: "NonExistentCity12345");

        // Act
        var result = await repository.SearchAsync(query, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    #endregion

    #region Price Filter Tests

    [Fact]
    public async Task SearchAsync_WithMinPrice_ReturnsHotelsAboveMinPrice()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedTestDataAsync(context);
        var repository = new HotelRepository(context);
        var query = new SearchHotelsQuery(MinPrice: 200m);

        // Act
        var result = await repository.SearchAsync(query, CancellationToken.None);

        // Assert
        result.Items.Should().OnlyContain(h => h.PriceStartingFrom >= 200m);
    }

    [Fact]
    public async Task SearchAsync_WithMaxPrice_ReturnsHotelsBelowMaxPrice()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedTestDataAsync(context);
        var repository = new HotelRepository(context);
        var query = new SearchHotelsQuery(MaxPrice: 200m);

        // Act
        var result = await repository.SearchAsync(query, CancellationToken.None);

        // Assert
        result.Items.Should().OnlyContain(h => h.PriceStartingFrom <= 200m);
    }

    [Fact]
    public async Task SearchAsync_WithPriceRange_ReturnsHotelsInRange()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedTestDataAsync(context);
        var repository = new HotelRepository(context);
        var query = new SearchHotelsQuery(MinPrice: 100m, MaxPrice: 250m);

        // Act
        var result = await repository.SearchAsync(query, CancellationToken.None);

        // Assert
        result.Items.Should().OnlyContain(h => h.PriceStartingFrom >= 100m && h.PriceStartingFrom <= 250m);
    }

    #endregion

    #region Star Rating Filter Tests

    [Fact]
    public async Task SearchAsync_WithStarRatings_ReturnsMatchingHotels()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedTestDataAsync(context);
        var repository = new HotelRepository(context);
        var query = new SearchHotelsQuery(StarRatings: new[] { 4, 5 });

        // Act
        var result = await repository.SearchAsync(query, CancellationToken.None);

        // Assert
        result.Items.Should().OnlyContain(h => h.StarRating == 4 || h.StarRating == 5);
    }

    [Fact]
    public async Task SearchAsync_WithSingleStarRating_ReturnsMatchingHotels()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedTestDataAsync(context);
        var repository = new HotelRepository(context);
        var query = new SearchHotelsQuery(StarRatings: new[] { 5 });

        // Act
        var result = await repository.SearchAsync(query, CancellationToken.None);

        // Assert
        result.Items.Should().OnlyContain(h => h.StarRating == 5);
    }

    #endregion

    #region Sorting Tests

    [Fact]
    public async Task SearchAsync_SortByPriceAsc_ReturnsSortedResults()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedTestDataAsync(context);
        var repository = new HotelRepository(context);
        var query = new SearchHotelsQuery(SortBy: HotelSortBy.Price, SortOrder: SortOrder.Asc);

        // Act
        var result = await repository.SearchAsync(query, CancellationToken.None);

        // Assert
        result.Items.Should().BeInAscendingOrder(h => h.PriceStartingFrom);
    }

    [Fact]
    public async Task SearchAsync_SortByPriceDesc_ReturnsSortedResults()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedTestDataAsync(context);
        var repository = new HotelRepository(context);
        var query = new SearchHotelsQuery(SortBy: HotelSortBy.Price, SortOrder: SortOrder.Desc);

        // Act
        var result = await repository.SearchAsync(query, CancellationToken.None);

        // Assert
        result.Items.Should().BeInDescendingOrder(h => h.PriceStartingFrom);
    }

    [Fact]
    public async Task SearchAsync_SortByStarRatingAsc_ReturnsSortedResults()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedTestDataAsync(context);
        var repository = new HotelRepository(context);
        var query = new SearchHotelsQuery(SortBy: HotelSortBy.StarRating, SortOrder: SortOrder.Asc);

        // Act
        var result = await repository.SearchAsync(query, CancellationToken.None);

        // Assert
        result.Items.Should().BeInAscendingOrder(h => h.StarRating);
    }

    [Fact]
    public async Task SearchAsync_SortByStarRatingDesc_ReturnsSortedResults()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedTestDataAsync(context);
        var repository = new HotelRepository(context);
        var query = new SearchHotelsQuery(SortBy: HotelSortBy.StarRating, SortOrder: SortOrder.Desc);

        // Act
        var result = await repository.SearchAsync(query, CancellationToken.None);

        // Assert
        result.Items.Should().BeInDescendingOrder(h => h.StarRating);
    }

    #endregion

    #region Pagination Tests

    [Fact]
    public async Task SearchAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedTestDataAsync(context);
        var repository = new HotelRepository(context);
        var query = new SearchHotelsQuery(Page: 1, PageSize: 2);

        // Act
        var result = await repository.SearchAsync(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCountLessOrEqualTo(2);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(2);
    }

    [Fact]
    public async Task SearchAsync_WithSecondPage_ReturnsDifferentResults()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedTestDataAsync(context);
        var repository = new HotelRepository(context);
        
        var queryPage1 = new SearchHotelsQuery(Page: 1, PageSize: 2);
        var queryPage2 = new SearchHotelsQuery(Page: 2, PageSize: 2);

        // Act
        var resultPage1 = await repository.SearchAsync(queryPage1, CancellationToken.None);
        var resultPage2 = await repository.SearchAsync(queryPage2, CancellationToken.None);

        // Assert
        if (resultPage2.Items.Any())
        {
            resultPage1.Items.Select(h => h.HotelId)
                .Should().NotIntersectWith(resultPage2.Items.Select(h => h.HotelId));
        }
    }

    [Fact]
    public async Task SearchAsync_PaginationInfo_IsCorrect()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedTestDataAsync(context);
        var repository = new HotelRepository(context);
        var query = new SearchHotelsQuery(Page: 1, PageSize: 2);

        // Act
        var result = await repository.SearchAsync(query, CancellationToken.None);

        // Assert
        result.TotalCount.Should().BeGreaterThan(0);
        result.TotalPages.Should().Be((int)Math.Ceiling((double)result.TotalCount / 2));
        result.HasNextPage.Should().Be(result.TotalPages > 1);
        result.HasPreviousPage.Should().BeFalse();
    }

    #endregion

    #region Room Availability Tests

    [Fact]
    public async Task SearchAsync_WithGuestCapacity_ReturnsHotelsWithMatchingRooms()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedTestDataAsync(context);
        var repository = new HotelRepository(context);
        var query = new SearchHotelsQuery(Adults: 2, Children: 1);

        // Act
        var result = await repository.SearchAsync(query, CancellationToken.None);

        // Assert
        result.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SearchAsync_ExcludesHotelsWithBookedRooms()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var (hotelWithBooking, hotelWithoutBooking) = await SeedHotelsWithBookingsAsync(context);
        var repository = new HotelRepository(context);
        
        var checkIn = DateOnly.FromDateTime(DateTime.Today.AddDays(5));
        var checkOut = DateOnly.FromDateTime(DateTime.Today.AddDays(7));
        var query = new SearchHotelsQuery(CheckInDate: checkIn, CheckOutDate: checkOut);

        // Act
        var result = await repository.SearchAsync(query, CancellationToken.None);

        // Assert
        // Hotel with booked rooms for overlapping dates should be excluded
        result.Items.Should().NotContain(h => h.HotelId == hotelWithBooking);
    }

    #endregion

    #region Discount Tests

    [Fact]
    public async Task SearchAsync_HotelsWithDiscount_ShowDiscountedPrice()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        await SeedTestDataAsync(context);
        var repository = new HotelRepository(context);
        var query = new SearchHotelsQuery();

        // Act
        var result = await repository.SearchAsync(query, CancellationToken.None);

        // Assert
        var hotelsWithDiscount = result.Items.Where(h => h.DiscountPercentage.HasValue);
        hotelsWithDiscount.Should().OnlyContain(h => 
            h.DiscountedPrice.HasValue && 
            h.DiscountedPrice < h.PriceStartingFrom);
    }

    #endregion

    #region Helper Methods

    private async Task SeedTestDataAsync(ApplicationDbContext context)
    {
        var city1 = new City
        {
            Id = Guid.NewGuid(),
            Name = "Dubai",
            Country = "UAE",
            PostOffice = "00000",
            ThumbnailUrl = "https://example.com/dubai.jpg"
        };

        var city2 = new City
        {
            Id = Guid.NewGuid(),
            Name = "Paris",
            Country = "France",
            PostOffice = "75000",
            ThumbnailUrl = "https://example.com/paris.jpg"
        };

        context.Cities.AddRange(city1, city2);

        var roomType = new RoomType
        {
            Id = Guid.NewGuid(),
            Name = "Deluxe",
            Description = "Deluxe room with city view"
        };

        context.RoomTypes.Add(roomType);

        var hotels = new List<Hotel>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Grand Dubai Hotel",
                Description = "Luxury 5-star hotel",
                StarRating = 5,
                CityId = city1.Id,
                Address = "123 Sheikh Zayed Road",
                Latitude = 25.2048m,
                Longitude = 55.2708m,
                ThumbnailUrl = "https://example.com/grand-dubai.jpg",
                MinRoomPrice = 300m,
                DiscountPercentage = 20,
                Rooms = new List<Room>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        RoomNumber = "101",
                        RoomTypeId = roomType.Id,
                        PricePerNight = 300m,
                        AdultCapacity = 2,
                        ChildCapacity = 2,
                        IsAvailable = true
                    }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Dubai Beach Resort",
                Description = "Beautiful beach resort",
                StarRating = 4,
                CityId = city1.Id,
                Address = "456 Beach Road",
                Latitude = 25.1048m,
                Longitude = 55.1708m,
                ThumbnailUrl = "https://example.com/beach-resort.jpg",
                MinRoomPrice = 200m,
                Rooms = new List<Room>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        RoomNumber = "201",
                        RoomTypeId = roomType.Id,
                        PricePerNight = 200m,
                        AdultCapacity = 2,
                        ChildCapacity = 1,
                        IsAvailable = true
                    }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Paris Luxury Hotel",
                Description = "Elegant Parisian hotel",
                StarRating = 5,
                CityId = city2.Id,
                Address = "789 Champs-Élysées",
                Latitude = 48.8566m,
                Longitude = 2.3522m,
                ThumbnailUrl = "https://example.com/paris-luxury.jpg",
                MinRoomPrice = 400m,
                DiscountPercentage = 15,
                Rooms = new List<Room>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        RoomNumber = "301",
                        RoomTypeId = roomType.Id,
                        PricePerNight = 400m,
                        AdultCapacity = 2,
                        ChildCapacity = 2,
                        IsAvailable = true
                    }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Budget Inn Paris",
                Description = "Affordable accommodation",
                StarRating = 3,
                CityId = city2.Id,
                Address = "100 Rue de Rivoli",
                Latitude = 48.8606m,
                Longitude = 2.3376m,
                ThumbnailUrl = "https://example.com/budget-inn.jpg",
                MinRoomPrice = 80m,
                Rooms = new List<Room>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        RoomNumber = "401",
                        RoomTypeId = roomType.Id,
                        PricePerNight = 80m,
                        AdultCapacity = 2,
                        ChildCapacity = 0,
                        IsAvailable = true
                    }
                }
            }
        };

        context.Hotels.AddRange(hotels);
        await context.SaveChangesAsync();
    }

    private async Task<(Guid hotelWithBooking, Guid hotelWithoutBooking)> SeedHotelsWithBookingsAsync(ApplicationDbContext context)
    {
        var city = new City
        {
            Id = Guid.NewGuid(),
            Name = "Test City",
            Country = "Test Country",
            PostOffice = "12345"
        };

        var roomType = new RoomType
        {
            Id = Guid.NewGuid(),
            Name = "Standard",
            Description = "Standard room"
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@test.com",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            FirstName = "Test",
            LastName = "User",
            Role = UserRole.User
        };

        context.Cities.Add(city);
        context.RoomTypes.Add(roomType);
        context.Users.Add(user);

        var room1 = new Room
        {
            Id = Guid.NewGuid(),
            RoomNumber = "101",
            RoomTypeId = roomType.Id,
            PricePerNight = 100m,
            AdultCapacity = 2,
            ChildCapacity = 1,
            IsAvailable = true
        };

        var hotelWithBooking = new Hotel
        {
            Id = Guid.NewGuid(),
            Name = "Hotel With Booking",
            Description = "Test hotel",
            StarRating = 4,
            CityId = city.Id,
            Address = "123 Test St",
            Latitude = 0m,
            Longitude = 0m,
            MinRoomPrice = 100m,
            Rooms = new List<Room> { room1 }
        };

        var room2 = new Room
        {
            Id = Guid.NewGuid(),
            RoomNumber = "201",
            RoomTypeId = roomType.Id,
            PricePerNight = 150m,
            AdultCapacity = 2,
            ChildCapacity = 1,
            IsAvailable = true
        };

        var hotelWithoutBooking = new Hotel
        {
            Id = Guid.NewGuid(),
            Name = "Hotel Without Booking",
            Description = "Test hotel 2",
            StarRating = 4,
            CityId = city.Id,
            Address = "456 Test St",
            Latitude = 0m,
            Longitude = 0m,
            MinRoomPrice = 150m,
            Rooms = new List<Room> { room2 }
        };

        context.Hotels.AddRange(hotelWithBooking, hotelWithoutBooking);
        await context.SaveChangesAsync();

        // Create a booking for the first hotel
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            ConfirmationNumber = "BK-TEST-001",
            Status = BookingStatus.Confirmed,
            PaymentStatus = PaymentStatus.Paid,
            PaymentMethod = PaymentMethod.CreditCard,
            TotalPrice = 200m,
            Items = new List<BookingItem>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    RoomId = room1.Id,
                    CheckInDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
                    CheckOutDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7)),
                    PricePerNight = 100m,
                    TotalPrice = 200m
                }
            }
        };

        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        return (hotelWithBooking.Id, hotelWithoutBooking.Id);
    }

    #endregion
}
