using coFind.Domain.Entities;

namespace coFind.Application.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<bool> RotateAsync(RefreshToken currentToken, RefreshToken replacementToken, CancellationToken cancellationToken = default);
    Task<bool> RevokeAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task<int> DeleteExpiredOrRevokedAsync(DateTime olderThan, CancellationToken cancellationToken = default);
}