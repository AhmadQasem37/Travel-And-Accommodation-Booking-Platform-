using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TAABP.Application.Common;
using TAABP.Application.DTOs.Hotels;
using TAABP.Application.Features.Hotels.Queries.GetFeaturedDeals;
using TAABP.Application.Features.Hotels.Queries.SearchHotels;

namespace TAABP.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/hotels")]
[Authorize]
public sealed class HotelsController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Get featured deals - hotels with active discounts
    /// </summary>
    /// <param name="query">Query parameters for featured deals</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of featured hotel deals</returns>
    [HttpGet("featured-deals")]
    [Authorize(Policy = "User")]
    [ProducesResponseType(typeof(IReadOnlyList<FeaturedDealDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<FeaturedDealDto>>> GetFeaturedDeals(
        [FromQuery] GetFeaturedDealsQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error.Description });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Search hotels with filters, sorting and pagination
    /// </summary>
    [HttpGet("search")]
    [Authorize(Policy = "AdminOrUser")]
    [ProducesResponseType(typeof(PagedResult<SearchHotelDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<SearchHotelDto>>> Search(
        [FromQuery] SearchHotelsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error.Description });
        }

        return Ok(result.Value);
    }
}
