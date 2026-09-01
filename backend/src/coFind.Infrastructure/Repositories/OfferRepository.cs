using coFind.Application.Interfaces;
using coFind.Domain.Entities;
using coFind.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace coFind.Infrastructure.Repositories;

public class OfferRepository : IOfferRepository
{
    private readonly AppDbContext _context;

    public OfferRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Offer?> GetByIdAsync(int offerId, CancellationToken cancellationToken = default)
    {
        return await _context.Offers
            .Include(o => o.Owner)
            .FirstOrDefaultAsync(o => o.OfferId == offerId, cancellationToken);
    }

    public async Task<(IReadOnlyList<Offer> Items, int TotalCount)> GetActivePagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Offers
            .AsNoTracking()
            .Include(o => o.Owner)
            .Where(o => o.IsActive)
            .OrderByDescending(o => o.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IEnumerable<Offer>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.Offers
            .Include(o => o.Owner)
            .Where(o => o.UserId == userId)
            .AsNoTracking()
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Offer offer, CancellationToken cancellationToken = default)
    {
        await _context.Offers.AddAsync(offer, cancellationToken);
    }

    public Task DeleteAsync(Offer offer, CancellationToken cancellationToken = default)
    {
        _context.Offers.Remove(offer);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
