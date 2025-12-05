using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TAABP.Application.Common;
using TAABP.Application.DTOs.Hotels;
using TAABP.Application.DTOs.Rooms;
using TAABP.Application.Features.Hotels.Queries.GetFeaturedDeals;
using TAABP.Application.Features.Hotels.Queries.GetHotelById;
using TAABP.Application.Features.Hotels.Queries.GetRecentlyVisitedHotels;
using TAABP.Application.Features.Hotels.Queries.SearchHotels;
using TAABP.Application.Features.Rooms.Queries.GetAvailableRooms;

namespace TAABP.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/hotels")]
[Authorize]
public sealed class HotelsController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Get hotel details by ID
    /// </summary>
    /// <param name="query">The query containing the hotel ID from route</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Hotel details including city, images, amenities, and review statistics</returns>
    [HttpGet("{hotelId:guid}")]
    [Authorize(Policy = "User")]
    [ProducesResponseType(typeof(HotelDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<HotelDetailsDto>> GetById(
        [FromRoute] GetHotelByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new { error = result.Error.Description });
        }

        return Ok(result.Value);
    }

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
    /// Get recently visited hotels by the authenticated user
    /// </summary>
    /// <param name="query">Query parameters for recently visited hotels</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of recently visited hotels</returns>
    [HttpGet("recently-visited")]
    [Authorize(Policy = "User")]
    [ProducesResponseType(typeof(IReadOnlyList<RecentlyVisitedHotelDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<RecentlyVisitedHotelDto>>> GetRecentlyVisited(
        [FromQuery] GetRecentlyVisitedHotelsQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return Unauthorized(new { error = result.Error.Description });
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

    /// <summary>
    /// Get available rooms for a hotel with pagination
    /// </summary>
    /// <param name="hotelId">Hotel ID from route</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of available rooms</returns>
    [HttpGet("{hotelId:guid}/rooms")]
    [Authorize(Policy = "AdminOrUser")]
    [ProducesResponseType(typeof(PagedResult<AvailableRoomDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<AvailableRoomDto>>> GetAvailableRooms(
        [FromRoute] Guid hotelId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAvailableRoomsQuery(hotelId, page, pageSize);
        var result = await sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new { error = result.Error.Description });
        }

        return Ok(result.Value);
    }
}
