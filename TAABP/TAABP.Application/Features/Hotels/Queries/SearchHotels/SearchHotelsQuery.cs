using MediatR;
using TAABP.Application.Common;
using TAABP.Application.DTOs.Hotels;
using TAABP.Domain.Enums;

namespace TAABP.Application.Features.Hotels.Queries.SearchHotels;

public sealed record SearchHotelsQuery(
    string? SearchQuery = null,
    DateOnly? CheckInDate = null,
    DateOnly? CheckOutDate = null,
    int Adults = 2,
    int Children = 0,
    int Rooms = 1,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    int[]? StarRatings = null,
    Guid[]? AmenityIds = null,
    Guid[]? RoomTypeIds = null,
    HotelSortBy SortBy = HotelSortBy.Price,
    SortOrder SortOrder = SortOrder.Asc,
    int Page = 1,
    int PageSize = 10
) : IRequest<Result<PagedResult<SearchHotelDto>>>
{
    public DateOnly EffectiveCheckInDate => CheckInDate ?? DateOnly.FromDateTime(DateTime.Today);
    public DateOnly EffectiveCheckOutDate => CheckOutDate ?? DateOnly.FromDateTime(DateTime.Today.AddDays(1));
}
