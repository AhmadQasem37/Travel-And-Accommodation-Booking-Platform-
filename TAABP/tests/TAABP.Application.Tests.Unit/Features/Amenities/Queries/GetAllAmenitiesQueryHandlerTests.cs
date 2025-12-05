using AutoMapper;
using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using TAABP.Application.DTOs.Amenities;
using TAABP.Application.Features.Amenities.Queries.GetAllAmenities;
using TAABP.Application.Interfaces.Repositories;
using TAABP.Application.Mappings;
using TAABP.Domain.Entities;

namespace TAABP.Application.Tests.Unit.Features.Amenities.Queries;

public class GetAllAmenitiesQueryHandlerTests
{
    private readonly Mock<IAmenityRepository> _mockRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllAmenitiesQueryHandler> _logger;
    private readonly GetAllAmenitiesQueryHandler _handler;

    public GetAllAmenitiesQueryHandlerTests()
    {
        _mockRepository = new Mock<IAmenityRepository>();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AmenityMappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        _logger = LoggerFactory.Create(b => b.AddConsole())
            .CreateLogger<GetAllAmenitiesQueryHandler>();

        _handler = new GetAllAmenitiesQueryHandler(
            _mockRepository.Object,
            _mapper,
            _logger);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenAmenitiesExist_ReturnsSuccessWithAmenities()
    {
        // Arrange
        var query = new GetAllAmenitiesQuery();
        var amenities = new List<Amenity>
        {
            CreateAmenity("Free WiFi"),
            CreateAmenity("Swimming Pool"),
            CreateAmenity("Gym")
        };

        _mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(amenities);

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
    public async Task Handle_WhenNoAmenitiesExist_ReturnsEmptyList()
    {
        // Arrange
        var query = new GetAllAmenitiesQuery();
        var amenities = new List<Amenity>();

        _mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(amenities);

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
        var query = new GetAllAmenitiesQuery();
        var amenityId = Guid.NewGuid();
        var amenity = new Amenity
        {
            Id = amenityId,
            Name = "Free WiFi",
            Description = "High-speed internet"
        };

        _mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Amenity> { amenity });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle();
        result.Value[0].Id.Should().Be(amenityId);
        result.Value[0].Name.Should().Be("Free WiFi");
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task Handle_PassesCancellationTokenToRepository()
    {
        // Arrange
        var query = new GetAllAmenitiesQuery();
        var cancellationToken = new CancellationToken();

        _mockRepository
            .Setup(r => r.GetAllAsync(cancellationToken))
            .ReturnsAsync(new List<Amenity>());

        // Act
        await _handler.Handle(query, cancellationToken);

        // Assert
        _mockRepository.Verify(
            r => r.GetAllAsync(cancellationToken),
            Times.Once);
    }

    #endregion

    #region Multiple Amenities Tests

    [Fact]
    public async Task Handle_WithMultipleAmenities_ReturnsAllAmenities()
    {
        // Arrange
        var query = new GetAllAmenitiesQuery();
        var amenities = Enumerable.Range(1, 10)
            .Select(i => CreateAmenity($"Amenity {i}"))
            .ToList();

        _mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(amenities);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(10);
    }

    [Fact]
    public async Task Handle_PreservesAmenityOrder()
    {
        // Arrange
        var query = new GetAllAmenitiesQuery();
        var amenities = new List<Amenity>
        {
            CreateAmenity("Alpha"),
            CreateAmenity("Beta"),
            CreateAmenity("Gamma")
        };

        _mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(amenities);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value[0].Name.Should().Be("Alpha");
        result.Value[1].Name.Should().Be("Beta");
        result.Value[2].Name.Should().Be("Gamma");
    }

    #endregion

    #region Dto Property Tests

    [Fact]
    public async Task Handle_ReturnsDtoWithOnlyIdAndName()
    {
        // Arrange
        var query = new GetAllAmenitiesQuery();
        var amenity = new Amenity
        {
            Id = Guid.NewGuid(),
            Name = "Test Amenity",
            Description = "This description should not be in DTO"
        };

        _mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Amenity> { amenity });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dto = result.Value[0];
        dto.Should().BeOfType<AmenityDto>();
        dto.Id.Should().NotBeEmpty();
        dto.Name.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData("Free WiFi")]
    [InlineData("Swimming Pool")]
    [InlineData("24/7 Room Service")]
    [InlineData("Spa & Wellness")]
    public async Task Handle_WithDifferentAmenityNames_ReturnsCorrectNames(string amenityName)
    {
        // Arrange
        var query = new GetAllAmenitiesQuery();
        var amenity = CreateAmenity(amenityName);

        _mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Amenity> { amenity });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value[0].Name.Should().Be(amenityName);
    }

    #endregion

    #region Result Pattern Tests

    [Fact]
    public async Task Handle_AlwaysReturnsSuccessResult()
    {
        // Arrange
        var query = new GetAllAmenitiesQuery();

        _mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Amenity>());

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
        var query = new GetAllAmenitiesQuery();

        _mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Amenity>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.Should().NotBeNull();
    }

    #endregion

    #region Helper Methods

    private static Amenity CreateAmenity(string name)
    {
        return new Amenity
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = $"Description for {name}"
        };
    }

    #endregion
}
