using coFind.Domain.Entities;

namespace coFind.Application.Interfaces;

public interface IOfferRepository
{
    Task<Offer?> GetByIdAsync(int offerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Offer>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Offer>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task AddAsync(Offer offer, CancellationToken cancellationToken = default);
    Task DeleteAsync(Offer offer, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
