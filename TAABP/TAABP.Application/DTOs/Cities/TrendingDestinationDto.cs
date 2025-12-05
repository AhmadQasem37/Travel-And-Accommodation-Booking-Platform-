namespace TAABP.Application.DTOs.Cities;

public sealed record TrendingDestinationDto(
    Guid CityId,
    string CityName,
    string Country,
    string? ThumbnailUrl,
    int VisitCount);
