using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TAABP.Application.Common;
using TAABP.Application.Common.Errors;
using TAABP.Application.DTOs.Rooms;
using TAABP.Application.Features.Rooms.Queries.GetAvailableRooms;
using TAABP.Application.Interfaces.Repositories;
using TAABP.Domain.Entities;

namespace TAABP.Application.Tests.Unit.Features.Rooms.Queries;

public class GetAvailableRoomsQueryHandlerTests
{
    private readonly Mock<IHotelRepository> _mockHotelRepository;
    private readonly Mock<IRoomRepository> _mockRoomRepository;
    private readonly ILogger<GetAvailableRoomsQueryHandler> _logger;
    private readonly GetAvailableRoomsQueryHandler _handler;

    public GetAvailableRoomsQueryHandlerTests()
    {
        _mockHotelRepository = new Mock<IHotelRepository>();
        _mockRoomRepository = new Mock<IRoomRepository>();
        _logger = NullLogger<GetAvailableRoomsQueryHandler>.Instance;

        _handler = new GetAvailableRoomsQueryHandler(
            _mockHotelRepository.Object,
            _mockRoomRepository.Object,
            _logger);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidHotelId_ReturnsSuccess()
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var query = new GetAvailableRoomsQuery(hotelId);
        var hotel = CreateHotel(hotelId, discountPercentage: null);
        var pagedRooms = CreatePagedRooms(2, 1, 10);

        _mockHotelRepository
            .Setup(r => r.GetByIdAsync(hotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotel);
        _mockRoomRepository
            .Setup(r => r.GetAvailableRoomsByHotelIdAsync(hotelId, null, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedRooms);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithHotelDiscount_PassesDiscountToRepository()
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var discountPercentage = 25;
        var query = new GetAvailableRoomsQuery(hotelId);
        var hotel = CreateHotel(hotelId, discountPercentage);
        var pagedRooms = CreatePagedRoomsWithDiscount(2, 1, 10, discountPercentage);

        _mockHotelRepository
            .Setup(r => r.GetByIdAsync(hotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotel);
        _mockRoomRepository
            .Setup(r => r.GetAvailableRoomsByHotelIdAsync(hotelId, discountPercentage, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedRooms);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockRoomRepository.Verify(
            r => r.GetAvailableRoomsByHotelIdAsync(hotelId, discountPercentage, 1, 10, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithNoAvailableRooms_ReturnsEmptyPagedResult()
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var query = new GetAvailableRoomsQuery(hotelId);
        var hotel = CreateHotel(hotelId, discountPercentage: null);
        var emptyResult = new PagedResult<AvailableRoomDto>(
            new List<AvailableRoomDto>(), 1, 10, 0);

        _mockHotelRepository
            .Setup(r => r.GetByIdAsync(hotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotel);
        _mockRoomRepository
            .Setup(r => r.GetAvailableRoomsByHotelIdAsync(hotelId, null, 1, 10, It.IsAny<CancellationToken>()))
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
        var query = new GetAvailableRoomsQuery(hotelId, page, pageSize);
        var hotel = CreateHotel(hotelId, discountPercentage: null);
        var pagedRooms = CreatePagedRooms(5, page, pageSize, totalCount: 12);

        _mockHotelRepository
            .Setup(r => r.GetByIdAsync(hotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotel);
        _mockRoomRepository
            .Setup(r => r.GetAvailableRoomsByHotelIdAsync(hotelId, null, page, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedRooms);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.PageNumber.Should().Be(page);
        result.Value.PageSize.Should().Be(pageSize);
        _mockRoomRepository.Verify(
            r => r.GetAvailableRoomsByHotelIdAsync(hotelId, null, page, pageSize, It.IsAny<CancellationToken>()),
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
        var query = new GetAvailableRoomsQuery(hotelId, page, pageSize);
        var hotel = CreateHotel(hotelId, discountPercentage: null);
        var pagedRooms = CreatePagedRooms(10, page, pageSize, totalCount);

        _mockHotelRepository
            .Setup(r => r.GetByIdAsync(hotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotel);
        _mockRoomRepository
            .Setup(r => r.GetAvailableRoomsByHotelIdAsync(hotelId, null, page, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedRooms);

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
        var query = new GetAvailableRoomsQuery(hotelId);

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
    public async Task Handle_WithNonExistentHotel_DoesNotCallRoomRepository()
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var query = new GetAvailableRoomsQuery(hotelId);

        _mockHotelRepository
            .Setup(r => r.GetByIdAsync(hotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Hotel?)null);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockRoomRepository.Verify(
            r => r.GetAvailableRoomsByHotelIdAsync(It.IsAny<Guid>(), It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region Data Mapping Tests

    [Fact]
    public async Task Handle_ReturnsCorrectRoomData()
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var roomTypeId = Guid.NewGuid();
        var query = new GetAvailableRoomsQuery(hotelId);
        var hotel = CreateHotel(hotelId, discountPercentage: null);
        var room = new AvailableRoomDto(
            roomId, "101", roomTypeId, "Deluxe", "A deluxe room",
            200m, null, 2, 1, new List<RoomImageDto>());
        var pagedRooms = new PagedResult<AvailableRoomDto>(
            new List<AvailableRoomDto> { room }, 1, 10, 1);

        _mockHotelRepository
            .Setup(r => r.GetByIdAsync(hotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotel);
        _mockRoomRepository
            .Setup(r => r.GetAvailableRoomsByHotelIdAsync(hotelId, null, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedRooms);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var returnedRoom = result.Value.Items.First();
        returnedRoom.RoomId.Should().Be(roomId);
        returnedRoom.RoomNumber.Should().Be("101");
        returnedRoom.RoomTypeId.Should().Be(roomTypeId);
        returnedRoom.RoomTypeName.Should().Be("Deluxe");
        returnedRoom.RoomTypeDescription.Should().Be("A deluxe room");
        returnedRoom.PricePerNight.Should().Be(200m);
        returnedRoom.DiscountedPrice.Should().BeNull();
        returnedRoom.AdultCapacity.Should().Be(2);
        returnedRoom.ChildCapacity.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithDiscount_ReturnsDiscountedPrice()
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var discountPercentage = 20;
        var query = new GetAvailableRoomsQuery(hotelId);
        var hotel = CreateHotel(hotelId, discountPercentage);
        var room = new AvailableRoomDto(
            Guid.NewGuid(), "101", Guid.NewGuid(), "Deluxe", null,
            100m, 80m, 2, 1, new List<RoomImageDto>());
        var pagedRooms = new PagedResult<AvailableRoomDto>(
            new List<AvailableRoomDto> { room }, 1, 10, 1);

        _mockHotelRepository
            .Setup(r => r.GetByIdAsync(hotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotel);
        _mockRoomRepository
            .Setup(r => r.GetAvailableRoomsByHotelIdAsync(hotelId, discountPercentage, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedRooms);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var returnedRoom = result.Value.Items.First();
        returnedRoom.PricePerNight.Should().Be(100m);
        returnedRoom.DiscountedPrice.Should().Be(80m);
    }

    [Fact]
    public async Task Handle_WithRoomImages_ReturnsImages()
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var query = new GetAvailableRoomsQuery(hotelId);
        var hotel = CreateHotel(hotelId, discountPercentage: null);
        var images = new List<RoomImageDto>
        {
            new("https://example.com/image1.jpg"),
            new("https://example.com/image2.jpg")
        };
        var room = new AvailableRoomDto(
            Guid.NewGuid(), "101", Guid.NewGuid(), "Deluxe", null,
            200m, null, 2, 1, images);
        var pagedRooms = new PagedResult<AvailableRoomDto>(
            new List<AvailableRoomDto> { room }, 1, 10, 1);

        _mockHotelRepository
            .Setup(r => r.GetByIdAsync(hotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotel);
        _mockRoomRepository
            .Setup(r => r.GetAvailableRoomsByHotelIdAsync(hotelId, null, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedRooms);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var returnedRoom = result.Value.Items.First();
        returnedRoom.Images.Should().HaveCount(2);
        returnedRoom.Images.First().ImageUrl.Should().Be("https://example.com/image1.jpg");
    }

    [Fact]
    public async Task Handle_WithMultipleRooms_ReturnsAllRooms()
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var query = new GetAvailableRoomsQuery(hotelId);
        var hotel = CreateHotel(hotelId, discountPercentage: null);
        var rooms = new List<AvailableRoomDto>
        {
            CreateAvailableRoomDto("101", "Standard", 100m),
            CreateAvailableRoomDto("102", "Deluxe", 200m),
            CreateAvailableRoomDto("201", "Suite", 350m)
        };
        var pagedRooms = new PagedResult<AvailableRoomDto>(rooms, 1, 10, 3);

        _mockHotelRepository
            .Setup(r => r.GetByIdAsync(hotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotel);
        _mockRoomRepository
            .Setup(r => r.GetAvailableRoomsByHotelIdAsync(hotelId, null, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedRooms);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(3);
        result.Value.Items.Select(r => r.RoomNumber).Should().BeEquivalentTo(new[] { "101", "102", "201" });
    }

    #endregion

    #region Cancellation Tests

    [Fact]
    public async Task Handle_WhenCancelled_PropagatesCancellation()
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var query = new GetAvailableRoomsQuery(hotelId);
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
        var query = new GetAvailableRoomsQuery(hotelId);
        var hotel = CreateHotel(hotelId, discountPercentage: null);
        var pagedRooms = CreatePagedRooms(5, 1, 10);

        _mockHotelRepository
            .Setup(r => r.GetByIdAsync(hotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotel);
        _mockRoomRepository
            .Setup(r => r.GetAvailableRoomsByHotelIdAsync(hotelId, null, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedRooms);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockRoomRepository.Verify(
            r => r.GetAvailableRoomsByHotelIdAsync(hotelId, null, 1, 10, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithNullDiscount_PassesNullToRepository()
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var query = new GetAvailableRoomsQuery(hotelId);
        var hotel = CreateHotel(hotelId, discountPercentage: null);
        var pagedRooms = CreatePagedRooms(1, 1, 10);

        _mockHotelRepository
            .Setup(r => r.GetByIdAsync(hotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotel);
        _mockRoomRepository
            .Setup(r => r.GetAvailableRoomsByHotelIdAsync(hotelId, null, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedRooms);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockRoomRepository.Verify(
            r => r.GetAvailableRoomsByHotelIdAsync(hotelId, null, 1, 10, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithZeroDiscount_PassesZeroToRepository()
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var query = new GetAvailableRoomsQuery(hotelId);
        var hotel = CreateHotel(hotelId, discountPercentage: 0);
        var pagedRooms = CreatePagedRooms(1, 1, 10);

        _mockHotelRepository
            .Setup(r => r.GetByIdAsync(hotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotel);
        _mockRoomRepository
            .Setup(r => r.GetAvailableRoomsByHotelIdAsync(hotelId, 0, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedRooms);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockRoomRepository.Verify(
            r => r.GetAvailableRoomsByHotelIdAsync(hotelId, 0, 1, 10, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Helper Methods

    private static Hotel CreateHotel(Guid hotelId, int? discountPercentage) => new()
    {
        Id = hotelId,
        Name = "Test Hotel",
        Description = "A test hotel",
        CityId = Guid.NewGuid(),
        StarRating = 4,
        DiscountPercentage = discountPercentage
    };

    private static PagedResult<AvailableRoomDto> CreatePagedRooms(int count, int page, int pageSize, int? totalCount = null)
    {
        var rooms = Enumerable.Range(1, count)
            .Select(i => CreateAvailableRoomDto($"{100 + i}", "Standard", 100m + i * 10))
            .ToList();

        return new PagedResult<AvailableRoomDto>(rooms, page, pageSize, totalCount ?? count);
    }

    private static PagedResult<AvailableRoomDto> CreatePagedRoomsWithDiscount(int count, int page, int pageSize, int discountPercentage)
    {
        var rooms = Enumerable.Range(1, count)
            .Select(i =>
            {
                var price = 100m + i * 10;
                var discountedPrice = price * (1 - discountPercentage / 100m);
                return new AvailableRoomDto(
                    Guid.NewGuid(), $"{100 + i}", Guid.NewGuid(), "Standard", null,
                    price, discountedPrice, 2, 1, new List<RoomImageDto>());
            })
            .ToList();

        return new PagedResult<AvailableRoomDto>(rooms, page, pageSize, count);
    }

    private static AvailableRoomDto CreateAvailableRoomDto(string roomNumber, string roomTypeName, decimal price) =>
        new(Guid.NewGuid(), roomNumber, Guid.NewGuid(), roomTypeName, null, price, null, 2, 1, new List<RoomImageDto>());

    #endregion
}
