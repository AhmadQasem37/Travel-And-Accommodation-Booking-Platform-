namespace TAABP.Application.DTOs.Hotels;

/// <summary>
/// DTO for recently visited hotel by the authenticated user.
/// </summary>
public sealed record RecentlyVisitedHotelDto(
    Guid HotelId,
    string HotelName,
    string CityName,
    int StarRating,
    string? ThumbnailUrl,
    decimal PriceStartingFrom,
    DateTime VisitedAt
);
