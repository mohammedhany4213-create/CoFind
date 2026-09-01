using coFind.Application.DTOs;
using coFind.Application.Interfaces;
using coFind.Domain.Entities;

namespace coFind.Application.Services;

public class OfferService
{
    private readonly IOfferRepository _offerRepository;
    private readonly IUserRepository _userRepository;

    public OfferService(IOfferRepository offerRepository, IUserRepository userRepository)
    {
        _offerRepository = offerRepository;
        _userRepository = userRepository;
    }

    public async Task<CreateOfferResponse> CreateOfferAsync(int userId, CreateOfferRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null) throw new InvalidOperationException("User not found.");
        if (request.Skills is null || request.Skills.Count == 0) throw new ArgumentException("At least one skill is required.");
        var offer = new Offer { UserId = userId, Owner = user, Title = request.Title.Trim(), Description = request.Description.Trim(), Role = request.Role.Trim(), Skills = request.Skills.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList(), Industry = request.Industry.Trim(), IsAvilable = request.IsAvailable, Location = request.Location?.Trim() ?? string.Empty, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        if (offer.Skills.Count == 0) throw new ArgumentException("At least one valid skill is required.");
        await _offerRepository.AddAsync(offer, cancellationToken);
        await _offerRepository.SaveChangesAsync(cancellationToken);
        return new CreateOfferResponse(offer.OfferId, offer.UserId, offer.Title, offer.Description, offer.Role, offer.Skills, offer.Industry, offer.IsAvilable, offer.Location, offer.IsActive, offer.CreatedAt);
    }

    public async Task<IEnumerable<OfferListItemResponse>> GetAllActiveOffersAsync(CancellationToken cancellationToken = default)
    {
        var offers = await _offerRepository.GetAllAsync(cancellationToken);
        return offers.Where(o => o.IsActive).Select(MapToListItem);
    }

    public async Task<OfferListItemResponse?> GetByIdAsync(int offerId, CancellationToken cancellationToken = default)
    {
        var offer = await _offerRepository.GetByIdAsync(offerId, cancellationToken);
        if (offer is null || !offer.IsActive) return null;
        return MapToListItem(offer);
    }

    public async Task<IEnumerable<OfferListItemResponse>> GetMyOffersAsync(int userId, CancellationToken cancellationToken = default)
    {
        var offers = await _offerRepository.GetByUserIdAsync(userId, cancellationToken);
        return offers.Select(MapToListItem);
    }

    public async Task<OfferListItemResponse?> UpdateOfferAsync(int userId, int offerId, UpdateOfferRequest request, CancellationToken cancellationToken = default)
    {
        var offer = await _offerRepository.GetByIdAsync(offerId, cancellationToken);
        if (offer is null) return null;
        if (offer.UserId != userId) throw new UnauthorizedAccessException("You are not allowed to update this offer.");
        if (request.Skills is null || request.Skills.Count == 0) throw new ArgumentException("At least one skill is required.");
        var skills = request.Skills.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (skills.Count == 0) throw new ArgumentException("At least one valid skill is required.");
        offer.Title = request.Title.Trim(); offer.Description = request.Description.Trim(); offer.Role = request.Role.Trim(); offer.Skills = skills; offer.Industry = request.Industry.Trim(); offer.IsAvilable = request.IsAvailable; offer.Location = request.Location?.Trim() ?? string.Empty; offer.UpdatedAt = DateTime.UtcNow;
        await _offerRepository.SaveChangesAsync(cancellationToken);
        return MapToListItem(offer);
    }

    public async Task<bool> DeleteOfferAsync(int userId, int offerId, CancellationToken cancellationToken = default)
    {
        var offer = await _offerRepository.GetByIdAsync(offerId, cancellationToken);
        if (offer is null) return false;
        if (offer.UserId != userId) throw new UnauthorizedAccessException("You are not allowed to delete this offer.");
        await _offerRepository.DeleteAsync(offer, cancellationToken);
        await _offerRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<OfferListItemResponse?> UpdateOfferStatusAsync(int userId, int offerId, bool isActive, CancellationToken cancellationToken = default)
    {
        var offer = await _offerRepository.GetByIdAsync(offerId, cancellationToken);
        if (offer is null) return null;
        if (offer.UserId != userId) throw new UnauthorizedAccessException("You are not allowed to change this offer status.");
        offer.IsActive = isActive;
        offer.UpdatedAt = DateTime.UtcNow;
        await _offerRepository.SaveChangesAsync(cancellationToken);
        return MapToListItem(offer);
    }

    private static OfferListItemResponse MapToListItem(Offer offer)
    {
        var ownerId = offer.Owner?.UserId ?? offer.UserId;
        var ownerName = offer.Owner?.Name ?? string.Empty;
        return new OfferListItemResponse(offer.OfferId, offer.Title, offer.Description, offer.Role, offer.Skills, offer.Industry, offer.IsAvilable, offer.Location, offer.IsActive, offer.CreatedAt, new OfferOwnerResponse(ownerId, ownerName));
    }
}
