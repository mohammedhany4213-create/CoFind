using coFind.Application.DTOs;
using coFind.Application.Interfaces;
using coFind.Domain.Entities;

namespace coFind.Application.Services;

public class UserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly TimeSpan _refreshTokenLifetime;

    public UserService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IRefreshTokenRepository refreshTokenRepository,
        TimeSpan refreshTokenLifetime)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _refreshTokenLifetime = refreshTokenLifetime;
    }

    public async Task<RegisterUserResponse> RegisterUserAsync(RegisterUserRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var name = request.Name.Trim();
        var whatsappNumber = NormalizePhone(request.WhatsappNumber);

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.");
        if (string.IsNullOrWhiteSpace(whatsappNumber))
            throw new ArgumentException("WhatsApp number is required.");

        if (await _userRepository.EmailExistsAsync(email, cancellationToken))
            throw new InvalidOperationException("Email is already registered.");

        var user = new User
        {
            Name = name,
            Email = email,
            WhatsappNumber = whatsappNumber,
            PasswordHash = _passwordHasher.Hash(request.Password)
        };

        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return new RegisterUserResponse(user.UserId, user.Name, user.Email, user.WhatsappNumber);
    }

    public async Task<LoginUserResponse> LoginUserAsync(LoginUserRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        var refreshToken = _tokenService.GenerateRefreshToken();
        await _refreshTokenRepository.AddAsync(new RefreshToken
        {
            UserId = user.UserId,
            TokenHash = _tokenService.HashRefreshToken(refreshToken),
            ExpiresAt = DateTime.UtcNow.Add(_refreshTokenLifetime)
        }, cancellationToken);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return new LoginUserResponse(user.UserId, user.Name, user.Email, _tokenService.GenerateToken(user), refreshToken);
    }

    public async Task<RefreshTokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new UnauthorizedAccessException("Invalid refresh token.");

        var currentToken = await _refreshTokenRepository.GetByHashAsync(
            _tokenService.HashRefreshToken(refreshToken), cancellationToken);

        if (currentToken is null || currentToken.RevokedAt is not null || currentToken.ExpiresAt <= DateTime.UtcNow)
            throw new UnauthorizedAccessException("Invalid refresh token.");

        var replacementRefreshToken = _tokenService.GenerateRefreshToken();
        var replacement = new RefreshToken
        {
            UserId = currentToken.UserId,
            TokenHash = _tokenService.HashRefreshToken(replacementRefreshToken),
            ExpiresAt = DateTime.UtcNow.Add(_refreshTokenLifetime)
        };

        if (!await _refreshTokenRepository.RotateAsync(currentToken, replacement, cancellationToken))
            throw new UnauthorizedAccessException("Invalid refresh token.");

        return new RefreshTokenResponse(_tokenService.GenerateToken(currentToken.User), replacementRefreshToken);
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return;

        await _refreshTokenRepository.RevokeAsync(
            _tokenService.HashRefreshToken(refreshToken), cancellationToken);
    }

    public async Task<UserProfileResponse?> GetProfileAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        return user is null ? null : MapToProfile(user);
    }

    public async Task<UserProfileResponse?> UpdateProfileAsync(int userId, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null) return null;

        var name = request.Name.Trim();
        var whatsappNumber = NormalizePhone(request.WhatsappNumber);
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.");
        if (string.IsNullOrWhiteSpace(whatsappNumber)) throw new ArgumentException("WhatsApp number is required.");

        user.Name = name;
        user.WhatsappNumber = whatsappNumber;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.SaveChangesAsync(cancellationToken);
        return MapToProfile(user);
    }

    private static string NormalizePhone(string phone)
        => new string(phone.Where(char.IsDigit).ToArray());

    private static UserProfileResponse MapToProfile(User user)
        => new(user.UserId, user.Name, user.Email, user.WhatsappNumber, user.CreatedAt, user.UpdatedAt);
}