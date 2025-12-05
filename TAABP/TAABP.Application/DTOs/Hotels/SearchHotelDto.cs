namespace TAABP.Application.DTOs.Hotels;

public sealed record SearchHotelDto(
    Guid HotelId,
    string HotelName,
    string CityName,
    string Country,
    int StarRating,
    string? ThumbnailUrl,
    string? Description,
    decimal PriceStartingFrom,
    decimal? DiscountedPrice,
    int? DiscountPercentage,
    IReadOnlyList<string> Amenities
);
