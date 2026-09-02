using coFind.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace coFind.Infrastructure.Services;

public sealed class RefreshTokenCleanupService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);
    private static readonly TimeSpan Retention = TimeSpan.FromDays(30);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RefreshTokenCleanupService> _logger;

    public RefreshTokenCleanupService(IServiceScopeFactory scopeFactory, ILogger<RefreshTokenCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await CleanupAsync(stoppingToken);

        using var timer = new PeriodicTimer(Interval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
            await CleanupAsync(stoppingToken);
    }

    private async Task CleanupAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();
            var cutoff = DateTime.UtcNow.Subtract(Retention);
            var deleted = await repository.DeleteExpiredOrRevokedAsync(cutoff, cancellationToken);

            if (deleted > 0)
                _logger.LogInformation("Removed {Count} expired or revoked refresh tokens.", deleted);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Refresh token cleanup failed.");
        }
    }
}