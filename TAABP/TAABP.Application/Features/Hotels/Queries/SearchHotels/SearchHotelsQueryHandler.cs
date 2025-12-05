using MediatR;
using Microsoft.Extensions.Logging;
using TAABP.Application.Common;
using TAABP.Application.DTOs.Hotels;
using TAABP.Application.Interfaces.Repositories;

namespace TAABP.Application.Features.Hotels.Queries.SearchHotels;

public sealed class SearchHotelsQueryHandler(
    IHotelRepository hotelRepository,
    ILogger<SearchHotelsQueryHandler> logger) : IRequestHandler<SearchHotelsQuery, Result<PagedResult<SearchHotelDto>>>
{
    public async Task<Result<PagedResult<SearchHotelDto>>> Handle(
        SearchHotelsQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Searching hotels - Query: {Query}, Page: {Page}, PageSize: {PageSize}",
            request.SearchQuery ?? "(null)", request.Page, request.PageSize);

        var result = await hotelRepository.SearchAsync(request, cancellationToken);

        logger.LogInformation(
            "Found {Count} hotels (Page {Page} of {TotalPages})",
            result.Items.Count, result.PageNumber, result.TotalPages);

        return Result.Success(result);
    }
}
