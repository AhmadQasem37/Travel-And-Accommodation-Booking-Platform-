using AutoMapper;
using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using TAABP.Application.DTOs.RoomTypes;
using TAABP.Application.Features.RoomTypes.Queries.GetAllRoomTypes;
using TAABP.Application.Interfaces.Repositories;
using TAABP.Application.Mappings;
using TAABP.Domain.Entities;

namespace TAABP.Application.Tests.Unit.Features.RoomTypes.Queries;

public class GetAllRoomTypesQueryHandlerTests
{
    private readonly Mock<IRoomTypeRepository> _mockRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllRoomTypesQueryHandler> _logger;
    private readonly GetAllRoomTypesQueryHandler _handler;

    public GetAllRoomTypesQueryHandlerTests()
    {
        _mockRepository = new Mock<IRoomTypeRepository>();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<RoomTypeMappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        _logger = LoggerFactory.Create(b => b.AddConsole())
            .CreateLogger<GetAllRoomTypesQueryHandler>();

        _handler = new GetAllRoomTypesQueryHandler(
            _mockRepository.Object,
            _mapper,
            _logger);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenRoomTypesExist_ReturnsSuccessWithRoomTypes()
    {
        // Arrange
        var query = new GetAllRoomTypesQuery();
        var roomTypes = new List<RoomType>
        {
            CreateRoomType("Standard"),
            CreateRoomType("Deluxe"),
            CreateRoomType("Suite")
        };

        _mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(roomTypes);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(3);

        _mockRepository.Verify(
            r => r.GetAllAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNoRoomTypesExist_ReturnsEmptyList()
    {
        // Arrange
        var query = new GetAllRoomTypesQuery();
        var roomTypes = new List<RoomType>();

        _mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(roomTypes);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ReturnsCorrectDtoMapping()
    {
        // Arrange
        var query = new GetAllRoomTypesQuery();
        var roomTypeId = Guid.NewGuid();
        var roomType = new RoomType
        {
            Id = roomTypeId,
            Name = "Deluxe",
            Description = "Spacious room with premium amenities"
        };

        _mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoomType> { roomType });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle();
        result.Value[0].Id.Should().Be(roomTypeId);
        result.Value[0].Name.Should().Be("Deluxe");
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task Handle_PassesCancellationTokenToRepository()
    {
        // Arrange
        var query = new GetAllRoomTypesQuery();
        var cancellationToken = new CancellationToken();

        _mockRepository
            .Setup(r => r.GetAllAsync(cancellationToken))
            .ReturnsAsync(new List<RoomType>());

        // Act
        await _handler.Handle(query, cancellationToken);

        // Assert
        _mockRepository.Verify(
            r => r.GetAllAsync(cancellationToken),
            Times.Once);
    }

    #endregion

    #region Multiple Room Types Tests

    [Fact]
    public async Task Handle_WithMultipleRoomTypes_ReturnsAllRoomTypes()
    {
        // Arrange
        var query = new GetAllRoomTypesQuery();
        var roomTypes = Enumerable.Range(1, 10)
            .Select(i => CreateRoomType($"Room Type {i}"))
            .ToList();

        _mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(roomTypes);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(10);
    }

    [Fact]
    public async Task Handle_PreservesRoomTypeOrder()
    {
        // Arrange
        var query = new GetAllRoomTypesQuery();
        var roomTypes = new List<RoomType>
        {
            CreateRoomType("Standard"),
            CreateRoomType("Deluxe"),
            CreateRoomType("Suite")
        };

        _mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(roomTypes);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value[0].Name.Should().Be("Standard");
        result.Value[1].Name.Should().Be("Deluxe");
        result.Value[2].Name.Should().Be("Suite");
    }

    #endregion

    #region Dto Property Tests

    [Fact]
    public async Task Handle_ReturnsDtoWithOnlyIdAndName()
    {
        // Arrange
        var query = new GetAllRoomTypesQuery();
        var roomType = new RoomType
        {
            Id = Guid.NewGuid(),
            Name = "Test Room Type",
            Description = "This description should not be in DTO"
        };

        _mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoomType> { roomType });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dto = result.Value[0];
        dto.Should().BeOfType<RoomTypeDto>();
        dto.Id.Should().NotBeEmpty();
        dto.Name.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData("Standard")]
    [InlineData("Deluxe")]
    [InlineData("Executive Suite")]
    [InlineData("Presidential Suite")]
    public async Task Handle_WithDifferentRoomTypeNames_ReturnsCorrectNames(string roomTypeName)
    {
        // Arrange
        var query = new GetAllRoomTypesQuery();
        var roomType = CreateRoomType(roomTypeName);

        _mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoomType> { roomType });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value[0].Name.Should().Be(roomTypeName);
    }

    #endregion

    #region Result Pattern Tests

    [Fact]
    public async Task Handle_AlwaysReturnsSuccessResult()
    {
        // Arrange
        var query = new GetAllRoomTypesQuery();

        _mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoomType>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ReturnsNonNullValueInResult()
    {
        // Arrange
        var query = new GetAllRoomTypesQuery();

        _mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoomType>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.Should().NotBeNull();
    }

    #endregion

    #region Helper Methods

    private static RoomType CreateRoomType(string name)
    {
        return new RoomType
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = $"Description for {name}"
        };
    }

    #endregion
}
