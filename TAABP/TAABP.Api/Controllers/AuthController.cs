using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TAABP.Application.DTOs.Auth;
using TAABP.Application.Features.Auth.Commands.Login;
using TAABP.Application.Features.Auth.Commands.Register;

namespace TAABP.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class AuthController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Register a new user account
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(RegisterCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                "User.UsernameAlreadyExists" or "User.EmailAlreadyExists" => Conflict(new { error = result.Error.Description }),
                "Validation.InvalidInput" => BadRequest(new { error = result.Error.Description }),
                _ => BadRequest(new { error = result.Error.Description })
            };
        }

        return Created();
    }

    /// <summary>
    /// Login and get JWT token with role information
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                "User.InvalidCredentials" => Unauthorized(new { error = result.Error.Description }),
                "Validation.InvalidInput" => BadRequest(new { error = result.Error.Description }),
                _ => BadRequest(new { error = result.Error.Description })
            };
        }

        return Ok(result.Value);
    }
}
