using coFind.Application.DTOs;
using coFind.Application.Interfaces;
using coFind.Application.Services;
using coFind.Domain.Entities;
using Xunit;

namespace coFind.UnitTests.Services;

public class UserServiceTests
{
    [Fact]
    public async Task Login_WithInvalidCredentials_ThrowsUnauthorized()
    {
        var repository = new FakeUserRepository
        {
            User = new User { UserId = 1, Email = "user@example.com", PasswordHash = "hash" }
        };
        var service = CreateService(repository, new FakePasswordHasher { VerifyResult = false });

        var act = () => service.LoginUserAsync(
            new LoginUserRequest("user@example.com", "wrong"));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(act);
    }

    [Fact]
    public async Task Register_WithExistingEmail_ThrowsConflict()
    {
        var repository = new FakeUserRepository { EmailExistsResult = true };
        var service = CreateService(repository);

        var act = () => service.RegisterUserAsync(
            new RegisterUserRequest("Mohamed", "user@example.com", "01012345678", "Password123!"));

        await Assert.ThrowsAsync<InvalidOperationException>(act);
        Assert.False(repository.AddCalled);
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_RotatesToken()
    {
        var user = new User { UserId = 1, Name = "Mohamed", Email = "user@example.com" };
        var current = new RefreshToken
        {
            RefreshTokenId = 10,
            UserId = user.UserId,
            User = user,
            TokenHash = "hashed-old",
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };
        var repository = new FakeRefreshTokenRepository { CurrentToken = current };
        var tokenService = new FakeTokenService
        {
            RefreshToken = "new-refresh",
            GeneratedAccessToken = "new-access"
        };
        var service = CreateService(new FakeUserRepository(), tokenService, repository);

        var response = await service.RefreshTokenAsync("old-refresh");

        Assert.Equal("new-access", response.Token);
        Assert.Equal("new-refresh", response.RefreshToken);
        Assert.True(repository.RotateCalled);
        Assert.NotNull(repository.Replacement);
    }

    [Fact]
    public async Task RefreshToken_WithRevokedToken_ThrowsUnauthorized()
    {
        var repository = new FakeRefreshTokenRepository
        {
            CurrentToken = new RefreshToken
            {
                UserId = 1,
                TokenHash = "hashed-old",
                ExpiresAt = DateTime.UtcNow.AddDays(1),
                RevokedAt = DateTime.UtcNow.AddMinutes(-1)
            }
        };
        var service = CreateService(new FakeUserRepository(), new FakeTokenService(), repository);

        var act = () => service.RefreshTokenAsync("old-refresh");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(act);
        Assert.False(repository.RotateCalled);
    }

    [Fact]
    public async Task RefreshToken_WithExpiredToken_ThrowsUnauthorized()
    {
        var repository = new FakeRefreshTokenRepository
        {
            CurrentToken = new RefreshToken
            {
                UserId = 1,
                TokenHash = "hashed-old",
                ExpiresAt = DateTime.UtcNow.AddMinutes(-1)
            }
        };
        var service = CreateService(new FakeUserRepository(), new FakeTokenService(), repository);

        var act = () => service.RefreshTokenAsync("old-refresh");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(act);
        Assert.False(repository.RotateCalled);
    }

    [Fact]
    public async Task RefreshToken_WhenRotationFails_ThrowsUnauthorized()
    {
        var repository = new FakeRefreshTokenRepository
        {
            CurrentToken = new RefreshToken
            {
                UserId = 1,
                User = new User { UserId = 1 },
                TokenHash = "hashed-old",
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            },
            RotateResult = false
        };
        var service = CreateService(new FakeUserRepository(), new FakeTokenService(), repository);

        var act = () => service.RefreshTokenAsync("old-refresh");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(act);
    }

    private static UserService CreateService(
        FakeUserRepository userRepository,
        FakeTokenService? tokenService = null,
        FakeRefreshTokenRepository? refreshRepository = null)
        => new(
            userRepository,
            new FakePasswordHasher(),
            tokenService ?? new FakeTokenService(),
            refreshRepository ?? new FakeRefreshTokenRepository(),
            TimeSpan.FromDays(30));

    private sealed class FakeUserRepository : IUserRepository
    {
        public User? User { get; init; }
        public bool EmailExistsResult { get; init; }
        public bool AddCalled { get; private set; }

        public Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken = default) => Task.FromResult(User);
        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) => Task.FromResult(User);
        public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default) => Task.FromResult(EmailExistsResult);
        public Task AddAsync(User user, CancellationToken cancellationToken = default) { AddCalled = true; return Task.CompletedTask; }
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public bool VerifyResult { get; init; } = true;
        public string Hash(string password) => "hash";
        public bool Verify(string password, string passwordHash) => VerifyResult;
    }

    private sealed class FakeTokenService : ITokenService
    {
        public string RefreshToken { get; init; } = "refresh";
        public string GeneratedAccessToken { get; init; } = "access";
        public string GenerateToken(User user) => GeneratedAccessToken;
        public string GenerateRefreshToken() => RefreshToken;
        public string HashRefreshToken(string refreshToken) => $"hashed-{refreshToken}";
    }

    private sealed class FakeRefreshTokenRepository : IRefreshTokenRepository
    {
        public RefreshToken? CurrentToken { get; init; }
        public bool RotateResult { get; init; } = true;
        public bool RotateCalled { get; private set; }
        public RefreshToken? Replacement { get; private set; }

        public Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default) => Task.FromResult(CurrentToken);
        public Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<bool> RotateAsync(RefreshToken currentToken, RefreshToken replacementToken, CancellationToken cancellationToken = default)
        {
            RotateCalled = true;
            Replacement = replacementToken;
            return Task.FromResult(RotateResult);
        }
    }
}