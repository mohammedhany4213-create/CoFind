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

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var offer = await _offerService.GetByIdAsync(id, cancellationToken);
        if (offer is null)
            return NotFound(new { message = "Offer not found." });

        return Ok(offer);
    }

    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> GetMyOffers(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Invalid user identity." });

        var offers = await _offerService.GetMyOffersAsync(userId, cancellationToken);
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
            var response = await _offerService.CreateOfferAsync(userId, request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = response.OfferId }, response);
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
