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

    public async Task<CreateOfferResponse> CreateOfferAsync(
        int userId,
        CreateOfferRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
            throw new InvalidOperationException("User not found.");

        if (request.Skills is null || request.Skills.Count == 0)
            throw new ArgumentException("At least one skill is required.");

        var offer = new Offer
        {
            UserId = userId,
            Owner = user,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Role = request.Role.Trim(),
            Skills = request.Skills
                .Where(skill => !string.IsNullOrWhiteSpace(skill))
                .Select(skill => skill.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList(),
            Industry = request.Industry.Trim(),
            IsAvilable = request.IsAvailable,
            Location = request.Location?.Trim() ?? string.Empty,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (offer.Skills.Count == 0)
            throw new ArgumentException("At least one valid skill is required.");

        await _offerRepository.AddAsync(offer, cancellationToken);
        await _offerRepository.SaveChangesAsync(cancellationToken);

        return new CreateOfferResponse(
            offer.OfferId,
            offer.UserId,
            offer.Title,
            offer.Description,
            offer.Role,
            offer.Skills,
            offer.Industry,
            offer.IsAvilable,
            offer.Location,
            offer.IsActive,
            offer.CreatedAt);
    }

    public async Task<IEnumerable<Offer>> GetAllActiveOffersAsync(
        CancellationToken cancellationToken = default)
    {
        return await _offerRepository.GetAllAsync(cancellationToken);
    }
}
