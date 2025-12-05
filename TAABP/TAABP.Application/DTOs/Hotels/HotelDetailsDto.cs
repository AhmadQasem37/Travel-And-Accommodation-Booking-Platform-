namespace TAABP.Application.DTOs.Hotels;

public sealed record HotelDetailsDto(
    Guid Id,
    string Name,
    string? Description,
    int StarRating,
    string Address,
    decimal? Latitude,
    decimal? Longitude,
    string? NearbyAttractions,
    HotelCityDto City,
    decimal MinRoomPrice,
    int? DiscountPercentage,
    IReadOnlyList<HotelImageDto> Images,
    IReadOnlyList<HotelAmenityDto> Amenities,
    decimal AverageRating,
    int ReviewCount,
    DateTime CreatedAt);
