using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TAABP.Application.Common;
using TAABP.Application.DTOs.Hotels;
using TAABP.Application.Features.Hotels.Queries.SearchHotels;

namespace TAABP.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/hotels")]
[Authorize]
public sealed class HotelsController(ISender sender) : ControllerBase
{
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
