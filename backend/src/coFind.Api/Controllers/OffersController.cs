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
    public OffersController(OfferService offerService) => _offerService = offerService;

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await _offerService.GetAllActiveOffersAsync(cancellationToken));

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var offer = await _offerService.GetByIdAsync(id, cancellationToken);
        return offer is null ? NotFound(new { message = "Offer not found." }) : Ok(offer);
    }

    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> GetMyOffers(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized(new { message = "Invalid user identity." });
        return Ok(await _offerService.GetMyOffersAsync(userId, cancellationToken));
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<CreateOfferResponse>> Create([FromBody] CreateOfferRequest request, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized(new { message = "Invalid user identity." });
        try
        {
            var response = await _offerService.CreateOfferAsync(userId, request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = response.OfferId }, response);
        }
        catch (InvalidOperationException ex) { return NotFound(new { message = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateOfferRequest request, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized(new { message = "Invalid user identity." });
        try
        {
            var offer = await _offerService.UpdateOfferAsync(userId, id, request, cancellationToken);
            return offer is null ? NotFound(new { message = "Offer not found." }) : Ok(offer);
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [Authorize]
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOfferStatusRequest request, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized(new { message = "Invalid user identity." });
        try
        {
            var offer = await _offerService.UpdateOfferStatusAsync(userId, id, request.IsActive, cancellationToken);
            return offer is null ? NotFound(new { message = "Offer not found." }) : Ok(offer);
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized(new { message = "Invalid user identity." });
        try
        {
            var deleted = await _offerService.DeleteOfferAsync(userId, id, cancellationToken);
            return deleted ? NoContent() : NotFound(new { message = "Offer not found." });
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }
}
