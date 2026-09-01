using coFind.Application.DTOs;
using coFind.Application.Interfaces;
using coFind.Domain.Entities;

namespace coFind.Application.Services;

public class UserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public UserService(IUserRepository userRepository, IPasswordHasher passwordHasher, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
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

        try
        {
            await _userRepository.SaveChangesAsync(cancellationToken);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            throw new InvalidOperationException("Email is already registered.");
        }

        return new RegisterUserResponse(user.UserId, user.Name, user.Email, user.WhatsappNumber);
    }

    public async Task<LoginUserResponse> LoginUserAsync(LoginUserRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        var token = _tokenService.GenerateToken(user);
        return new LoginUserResponse(user.UserId, user.Name, user.Email, token);
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

        var name = request.Name?.Trim();
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
    {
        var value = new string(phone.Where(char.IsDigit).ToArray());
        if (value.StartsWith("00", StringComparison.Ordinal))
            value = value[2..];
        if (value.StartsWith("+", StringComparison.Ordinal))
            value = value[1..];
        return value;
    }

    private static UserProfileResponse MapToProfile(User user)
        => new(user.UserId, user.Name, user.Email, user.WhatsappNumber, user.CreatedAt, user.UpdatedAt);
}