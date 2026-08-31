using System.Security.Claims;
using coFind.Application.DTOs;
using coFind.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace coFind.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OffersController : ControllerBase
{
    private readonly OfferService _offerService;

    public OffersController(OfferService offerService)
    {
        _offerService = offerService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var offers = await _offerService.GetAllActiveOffersAsync(cancellationToken);
        return Ok(offers);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<CreateOfferResponse>> Create(
        [FromBody] CreateOfferRequest request,
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Invalid user identity." });

        try
        {
            var response = await _offerService.CreateOfferAsync(
                userId,
                request,
                cancellationToken);

            return CreatedAtAction(nameof(Create), new { id = response.OfferId }, response);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
