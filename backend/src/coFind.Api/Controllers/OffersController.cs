using coFind.Api.Extensions;
using coFind.Application.DTOs;
using coFind.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace coFind.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OffersController : ControllerBase
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 50;
    private readonly OfferService _offerService;

    public OffersController(OfferService offerService) => _offerService = offerService;

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) return BadRequest(new { message = "Page must be greater than 0." });
        if (pageSize < 1 || pageSize > MaxPageSize)
            return BadRequest(new { message = $"Page size must be between 1 and {MaxPageSize}." });

        return Ok(await _offerService.GetAllActiveOffersAsync(page, pageSize, cancellationToken));
    }

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
        if (!User.TryGetUserId(out var userId)) return Unauthorized(new { message = "Invalid user identity." });
        return Ok(await _offerService.GetMyOffersAsync(userId, cancellationToken));
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<CreateOfferResponse>> Create([FromBody] CreateOfferRequest request, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId)) return Unauthorized(new { message = "Invalid user identity." });
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
        if (!User.TryGetUserId(out var userId)) return Unauthorized(new { message = "Invalid user identity." });
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
        if (!User.TryGetUserId(out var userId)) return Unauthorized(new { message = "Invalid user identity." });
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
        if (!User.TryGetUserId(out var userId)) return Unauthorized(new { message = "Invalid user identity." });
        try
        {
            var deleted = await _offerService.DeleteOfferAsync(userId, id, cancellationToken);
            return deleted ? NoContent() : NotFound(new { message = "Offer not found." });
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }
}
