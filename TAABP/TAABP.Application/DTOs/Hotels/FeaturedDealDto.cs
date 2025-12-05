namespace TAABP.Application.DTOs.Hotels;

/// <summary>
/// DTO for featured hotel deals with active discounts.
/// </summary>
public sealed record FeaturedDealDto(
    Guid HotelId,
    string HotelName,
    string CityName,
    string Country,
    int StarRating,
    string? ThumbnailUrl,
    decimal OriginalPrice,
    decimal DiscountedPrice,
    int DiscountPercentage
);
