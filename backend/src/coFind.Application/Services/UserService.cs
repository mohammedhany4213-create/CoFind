using coFind.Application.DTOs;
using coFind.Application.Interfaces;
using coFind.Domain.Entities;

namespace coFind.Application.Services;

public class UserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public UserService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<RegisterUserResponse> RegisterUserAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var emailExists = await _userRepository.EmailExistsAsync(email, cancellationToken);
        if (emailExists)
        {
            throw new InvalidOperationException("Email is already registered.");
        }

        var user = new User
        {
            Name = request.Name.Trim(),
            Email = email,
            WhatsappNumber = request.WhatsappNumber.Trim(),
            PasswordHash = _passwordHasher.Hash(request.Password)
        };

        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return new RegisterUserResponse(user.UserId, user.Name, user.Email, user.WhatsappNumber);
    }

    public async Task<LoginUserResponse> LoginUserAsync(
        LoginUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var token = _tokenService.GenerateToken(user);

        return new LoginUserResponse(user.UserId, user.Name, user.Email, token);
    }
}
