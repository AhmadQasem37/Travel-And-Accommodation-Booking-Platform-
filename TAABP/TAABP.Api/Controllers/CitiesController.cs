using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TAABP.Application.DTOs.Cities;
using TAABP.Application.Features.Cities.Queries.GetTrendingDestinations;

namespace TAABP.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/cities")]
[Authorize]
public sealed class CitiesController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Get trending destinations - top visited cities
    /// </summary>
    /// <param name="query">Query parameters for trending destinations</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of trending destinations</returns>
    [HttpGet("trending-destinations")]
    [Authorize(Policy = "User")]
    [ProducesResponseType(typeof(IReadOnlyList<TrendingDestinationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<TrendingDestinationDto>>> GetTrendingDestinations(
        [FromQuery] GetTrendingDestinationsQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error.Description });
        }

        return Ok(result.Value);
    }
}
