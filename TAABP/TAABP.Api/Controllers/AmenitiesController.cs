using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TAABP.Application.DTOs.Amenities;
using TAABP.Application.Features.Amenities.Queries.GetAllAmenities;

namespace TAABP.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/amenities")]
[Authorize]
public sealed class AmenitiesController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Get all amenities for filter dropdowns
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of amenities with id and name</returns>
    [HttpGet]
    [Authorize(Policy = "User")]
    [ProducesResponseType(typeof(IReadOnlyList<AmenityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<AmenityDto>>> GetAllAmenities(
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetAllAmenitiesQuery(), cancellationToken);

        return Ok(result.Value);
    }
}
