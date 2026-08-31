namespace coFind.Application.DTOs;

public record LoginUserRequest(
    string Email,
    string Password
);
