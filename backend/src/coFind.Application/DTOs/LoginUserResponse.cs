namespace coFind.Application.DTOs;

public record LoginUserResponse(
    int UserId,
    string Name,
    string Email,
    string Token,
    string RefreshToken
);