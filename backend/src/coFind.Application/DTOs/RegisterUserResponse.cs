namespace coFind.Application.DTOs;

public record RegisterUserResponse(
    int UserId,
    string Name,
    string Email,
    string WhatsappNumber
);