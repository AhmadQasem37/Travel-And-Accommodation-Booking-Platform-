using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TAABP.Application.DTOs.RoomTypes;
using TAABP.Application.Features.RoomTypes.Queries.GetAllRoomTypes;

namespace TAABP.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/room-types")]
[Authorize]
public sealed class RoomTypesController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Get all room types for filter dropdowns
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of room types with id and name</returns>
    [HttpGet]
    [Authorize(Policy = "User")]
    [ProducesResponseType(typeof(IReadOnlyList<RoomTypeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<RoomTypeDto>>> GetAllRoomTypes(
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetAllRoomTypesQuery(), cancellationToken);

        return Ok(result.Value);
    }
}
