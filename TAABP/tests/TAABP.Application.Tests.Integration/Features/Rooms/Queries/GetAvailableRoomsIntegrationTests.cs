using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TAABP.Application.Tests.Integration.Fixtures;
using TAABP.Domain.Entities;
using TAABP.Infrastructure.Persistence.Context;
using TAABP.Infrastructure.Persistence.Repositories;

namespace TAABP.Application.Tests.Integration.Features.Rooms.Queries;

public class GetAvailableRoomsIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public GetAvailableRoomsIntegrationTests(DatabaseFixture fixture)
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
    public async Task GetAvailableRoomsByHotelIdAsync_WithNonExistentHotel_ReturnsEmptyPagedResult()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var nonExistentId = Guid.NewGuid();
        var repository = new RoomRepository(context);

        // Act
        var result = await repository.GetAvailableRoomsByHotelIdAsync(nonExistentId, null, 1, 10, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetAvailableRoomsByHotelIdAsync_WithHotelNoRooms_ReturnsEmptyPagedResult()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Grand Hotel");
        var repository = new RoomRepository(context);

        // Act
        var result = await repository.GetAvailableRoomsByHotelIdAsync(hotel.Id, null, 1, 10, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetAvailableRoomsByHotelIdAsync_WithAvailableRooms_ReturnsRooms()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Grand Hotel");
        var roomType = await SeedRoomTypeAsync(context, "Deluxe");
        await SeedRoomAsync(context, hotel.Id, roomType.Id, "101", 200m, isAvailable: true);
        var repository = new RoomRepository(context);

        // Act
        var result = await repository.GetAvailableRoomsByHotelIdAsync(hotel.Id, null, 1, 10, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().RoomNumber.Should().Be("101");
        result.Items.First().RoomTypeName.Should().Be("Deluxe");
    }

    [Fact]
    public async Task GetAvailableRoomsByHotelIdAsync_WithUnavailableRooms_ExcludesThem()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Grand Hotel");
        var roomType = await SeedRoomTypeAsync(context, "Deluxe");
        await SeedRoomAsync(context, hotel.Id, roomType.Id, "101", 200m, isAvailable: false);
        var repository = new RoomRepository(context);

        // Act
        var result = await repository.GetAvailableRoomsByHotelIdAsync(hotel.Id, null, 1, 10, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetAvailableRoomsByHotelIdAsync_WithMixedAvailability_ReturnsOnlyAvailable()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Grand Hotel");
        var roomType = await SeedRoomTypeAsync(context, "Deluxe");
        await SeedRoomAsync(context, hotel.Id, roomType.Id, "101", 200m, isAvailable: true);
        await SeedRoomAsync(context, hotel.Id, roomType.Id, "102", 200m, isAvailable: false);
        await SeedRoomAsync(context, hotel.Id, roomType.Id, "103", 200m, isAvailable: true);
        var repository = new RoomRepository(context);

        // Act
        var result = await repository.GetAvailableRoomsByHotelIdAsync(hotel.Id, null, 1, 10, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Items.Select(r => r.RoomNumber).Should().BeEquivalentTo(new[] { "101", "103" });
    }

    #endregion

    #region Pagination Tests

    [Fact]
    public async Task GetAvailableRoomsByHotelIdAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Grand Hotel");
        var roomType = await SeedRoomTypeAsync(context, "Standard");

        // Seed 15 rooms
        for (int i = 1; i <= 15; i++)
        {
            await SeedRoomAsync(context, hotel.Id, roomType.Id, $"{100 + i}", 100m + i * 10);
        }
        var repository = new RoomRepository(context);

        // Act
        var result = await repository.GetAvailableRoomsByHotelIdAsync(hotel.Id, null, 2, 5, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(5);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalCount.Should().Be(15);
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task GetAvailableRoomsByHotelIdAsync_WithLastPage_ReturnsRemainingItems()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Grand Hotel");
        var roomType = await SeedRoomTypeAsync(context, "Standard");

        // Seed 12 rooms
        for (int i = 1; i <= 12; i++)
        {
            await SeedRoomAsync(context, hotel.Id, roomType.Id, $"{100 + i}", 100m + i);
        }
        var repository = new RoomRepository(context);

        // Act - Get page 3 with pageSize 5 (should have 2 items)
        var result = await repository.GetAvailableRoomsByHotelIdAsync(hotel.Id, null, 3, 5, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        result.PageNumber.Should().Be(3);
        result.TotalCount.Should().Be(12);
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task GetAvailableRoomsByHotelIdAsync_WithPageBeyondTotal_ReturnsEmpty()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Grand Hotel");
        var roomType = await SeedRoomTypeAsync(context, "Standard");
        await SeedRoomAsync(context, hotel.Id, roomType.Id, "101", 100m);
        await SeedRoomAsync(context, hotel.Id, roomType.Id, "102", 100m);
        var repository = new RoomRepository(context);

        // Act
        var result = await repository.GetAvailableRoomsByHotelIdAsync(hotel.Id, null, 10, 10, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetAvailableRoomsByHotelIdAsync_WithDefaultPagination_ReturnsFirstPage()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Grand Hotel");
        var roomType = await SeedRoomTypeAsync(context, "Standard");

        for (int i = 1; i <= 5; i++)
        {
            await SeedRoomAsync(context, hotel.Id, roomType.Id, $"{100 + i}", 100m + i);
        }
        var repository = new RoomRepository(context);

        // Act
        var result = await repository.GetAvailableRoomsByHotelIdAsync(hotel.Id, null, 1, 10, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(5);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    #endregion

    #region Sorting Tests

    [Fact]
    public async Task GetAvailableRoomsByHotelIdAsync_SortsByPriceAscending()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Grand Hotel");
        var roomType = await SeedRoomTypeAsync(context, "Standard");
        await SeedRoomAsync(context, hotel.Id, roomType.Id, "103", 300m);
        await SeedRoomAsync(context, hotel.Id, roomType.Id, "101", 100m);
        await SeedRoomAsync(context, hotel.Id, roomType.Id, "102", 200m);
        var repository = new RoomRepository(context);

        // Act
        var result = await repository.GetAvailableRoomsByHotelIdAsync(hotel.Id, null, 1, 10, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(3);
        result.Items.Select(r => r.PricePerNight).Should().BeInAscendingOrder();
    }

    #endregion

    #region Room Data Tests

    [Fact]
    public async Task GetAvailableRoomsByHotelIdAsync_ReturnsCorrectRoomData()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Grand Hotel");
        var roomType = await SeedRoomTypeAsync(context, "Deluxe", "A luxurious deluxe room");
        var room = await SeedRoomAsync(context, hotel.Id, roomType.Id, "101", 200m,
            adultCapacity: 2, childCapacity: 1, isAvailable: true);
        var repository = new RoomRepository(context);

        // Act
        var result = await repository.GetAvailableRoomsByHotelIdAsync(hotel.Id, null, 1, 10, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        var returnedRoom = result.Items.First();
        returnedRoom.RoomId.Should().Be(room.Id);
        returnedRoom.RoomNumber.Should().Be("101");
        returnedRoom.RoomTypeId.Should().Be(roomType.Id);
        returnedRoom.RoomTypeName.Should().Be("Deluxe");
        returnedRoom.RoomTypeDescription.Should().Be("A luxurious deluxe room");
        returnedRoom.PricePerNight.Should().Be(200m);
        returnedRoom.AdultCapacity.Should().Be(2);
        returnedRoom.ChildCapacity.Should().Be(1);
    }

    [Fact]
    public async Task GetAvailableRoomsByHotelIdAsync_WithMultipleRoomTypes_ReturnsAllRooms()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Grand Hotel");
        var standardType = await SeedRoomTypeAsync(context, "Standard");
        var deluxeType = await SeedRoomTypeAsync(context, "Deluxe");
        var suiteType = await SeedRoomTypeAsync(context, "Suite");

        await SeedRoomAsync(context, hotel.Id, standardType.Id, "101", 100m);
        await SeedRoomAsync(context, hotel.Id, standardType.Id, "102", 100m);
        await SeedRoomAsync(context, hotel.Id, deluxeType.Id, "201", 200m);
        await SeedRoomAsync(context, hotel.Id, suiteType.Id, "301", 350m);
        var repository = new RoomRepository(context);

        // Act
        var result = await repository.GetAvailableRoomsByHotelIdAsync(hotel.Id, null, 1, 10, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(4);
        result.Items.Select(r => r.RoomTypeName).Distinct()
            .Should().BeEquivalentTo(new[] { "Standard", "Deluxe", "Suite" });
    }

    #endregion

    #region Discount Tests

    [Fact]
    public async Task GetAvailableRoomsByHotelIdAsync_WithDiscount_CalculatesDiscountedPrice()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Grand Hotel");
        var roomType = await SeedRoomTypeAsync(context, "Deluxe");
        await SeedRoomAsync(context, hotel.Id, roomType.Id, "101", 100m);
        var repository = new RoomRepository(context);

        // Act
        var result = await repository.GetAvailableRoomsByHotelIdAsync(hotel.Id, 20, 1, 10, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        var room = result.Items.First();
        room.PricePerNight.Should().Be(100m);
        room.DiscountedPrice.Should().Be(80m);
    }

    [Fact]
    public async Task GetAvailableRoomsByHotelIdAsync_WithoutDiscount_ReturnsNullDiscountedPrice()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Grand Hotel");
        var roomType = await SeedRoomTypeAsync(context, "Deluxe");
        await SeedRoomAsync(context, hotel.Id, roomType.Id, "101", 100m);
        var repository = new RoomRepository(context);

        // Act
        var result = await repository.GetAvailableRoomsByHotelIdAsync(hotel.Id, null, 1, 10, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().DiscountedPrice.Should().BeNull();
    }

    [Fact]
    public async Task GetAvailableRoomsByHotelIdAsync_WithZeroDiscount_ReturnsNullDiscountedPrice()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Grand Hotel");
        var roomType = await SeedRoomTypeAsync(context, "Deluxe");
        await SeedRoomAsync(context, hotel.Id, roomType.Id, "101", 100m);
        var repository = new RoomRepository(context);

        // Act
        var result = await repository.GetAvailableRoomsByHotelIdAsync(hotel.Id, 0, 1, 10, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().DiscountedPrice.Should().BeNull();
    }

    [Fact]
    public async Task GetAvailableRoomsByHotelIdAsync_With50PercentDiscount_CalculatesCorrectly()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Grand Hotel");
        var roomType = await SeedRoomTypeAsync(context, "Deluxe");
        await SeedRoomAsync(context, hotel.Id, roomType.Id, "101", 200m);
        var repository = new RoomRepository(context);

        // Act
        var result = await repository.GetAvailableRoomsByHotelIdAsync(hotel.Id, 50, 1, 10, CancellationToken.None);

        // Assert
        result.Items.First().DiscountedPrice.Should().Be(100m);
    }

    #endregion

    #region Room Images Tests

    [Fact]
    public async Task GetAvailableRoomsByHotelIdAsync_WithRoomImages_ReturnsImages()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Grand Hotel");
        var roomType = await SeedRoomTypeAsync(context, "Deluxe");
        var room = await SeedRoomAsync(context, hotel.Id, roomType.Id, "101", 200m);
        await SeedRoomImageAsync(context, room.Id, "https://example.com/image1.jpg");
        await SeedRoomImageAsync(context, room.Id, "https://example.com/image2.jpg");
        var repository = new RoomRepository(context);

        // Act
        var result = await repository.GetAvailableRoomsByHotelIdAsync(hotel.Id, null, 1, 10, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Images.Should().HaveCount(2);
        result.Items.First().Images.Select(i => i.ImageUrl)
            .Should().BeEquivalentTo(new[] { "https://example.com/image1.jpg", "https://example.com/image2.jpg" });
    }

    [Fact]
    public async Task GetAvailableRoomsByHotelIdAsync_WithNoImages_ReturnsEmptyImageList()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel = await SeedHotelAsync(context, city.Id, "Grand Hotel");
        var roomType = await SeedRoomTypeAsync(context, "Deluxe");
        await SeedRoomAsync(context, hotel.Id, roomType.Id, "101", 200m);
        var repository = new RoomRepository(context);

        // Act
        var result = await repository.GetAvailableRoomsByHotelIdAsync(hotel.Id, null, 1, 10, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Images.Should().BeEmpty();
    }

    #endregion

    #region Hotel Isolation Tests

    [Fact]
    public async Task GetAvailableRoomsByHotelIdAsync_DoesNotReturnRoomsFromOtherHotels()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var city = await SeedCityAsync(context, "Dubai", "UAE");
        var hotel1 = await SeedHotelAsync(context, city.Id, "Hotel 1");
        var hotel2 = await SeedHotelAsync(context, city.Id, "Hotel 2");
        var roomType = await SeedRoomTypeAsync(context, "Standard");

        await SeedRoomAsync(context, hotel1.Id, roomType.Id, "101", 100m);
        await SeedRoomAsync(context, hotel1.Id, roomType.Id, "102", 100m);
        await SeedRoomAsync(context, hotel2.Id, roomType.Id, "201", 150m);
        var repository = new RoomRepository(context);

        // Act
        var result = await repository.GetAvailableRoomsByHotelIdAsync(hotel1.Id, null, 1, 10, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(r => r.RoomNumber.StartsWith("10"));
    }

    #endregion

    #region Cancellation Tests

    [Fact]
    public async Task GetAvailableRoomsByHotelIdAsync_WhenCancelled_ThrowsOperationCancelledException()
    {
        // Arrange
        await using var context = await CreateFreshContextAsync();
        var repository = new RoomRepository(context);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => repository.GetAvailableRoomsByHotelIdAsync(Guid.NewGuid(), null, 1, 10, cts.Token));
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

    private static async Task<Hotel> SeedHotelAsync(ApplicationDbContext context, Guid cityId, string name, int? discountPercentage = null)
    {
        var hotel = new Hotel
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = "A test hotel",
            CityId = cityId,
            StarRating = 4,
            DiscountPercentage = discountPercentage,
            CreatedAt = DateTime.UtcNow
        };
        context.Hotels.Add(hotel);
        await context.SaveChangesAsync();
        return hotel;
    }

    private static async Task<RoomType> SeedRoomTypeAsync(ApplicationDbContext context, string name, string? description = null)
    {
        var roomType = new RoomType
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description ?? $"A {name} room",
            CreatedAt = DateTime.UtcNow
        };
        context.RoomTypes.Add(roomType);
        await context.SaveChangesAsync();
        return roomType;
    }

    private static async Task<Room> SeedRoomAsync(
        ApplicationDbContext context,
        Guid hotelId,
        Guid roomTypeId,
        string roomNumber,
        decimal price,
        int adultCapacity = 2,
        int childCapacity = 1,
        bool isAvailable = true)
    {
        var room = new Room
        {
            Id = Guid.NewGuid(),
            HotelId = hotelId,
            RoomTypeId = roomTypeId,
            RoomNumber = roomNumber,
            PricePerNight = price,
            AdultCapacity = adultCapacity,
            ChildCapacity = childCapacity,
            IsAvailable = isAvailable,
            CreatedAt = DateTime.UtcNow
        };
        context.Rooms.Add(room);
        await context.SaveChangesAsync();
        return room;
    }

    private static async Task<RoomImage> SeedRoomImageAsync(ApplicationDbContext context, Guid roomId, string imageUrl)
    {
        var image = new RoomImage
        {
            Id = Guid.NewGuid(),
            RoomId = roomId,
            ImageUrl = imageUrl
        };
        context.RoomImages.Add(image);
        await context.SaveChangesAsync();
        return image;
    }

    #endregion
}
