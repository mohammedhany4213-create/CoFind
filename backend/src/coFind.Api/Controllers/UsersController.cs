using coFind.Api.Extensions;
using coFind.Application.DTOs;
using coFind.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace coFind.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;

    public UsersController(UserService userService)
    {
        _userService = userService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId)) return Unauthorized(new { message = "Invalid user identity." });

        var user = await _userService.GetProfileAsync(userId, cancellationToken);
        return user is null ? NotFound(new { message = "User not found." }) : Ok(user);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId)) return Unauthorized(new { message = "Invalid user identity." });

        try
        {
            var user = await _userService.UpdateProfileAsync(userId, request, cancellationToken);
            return user is null ? NotFound(new { message = "User not found." }) : Ok(user);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
