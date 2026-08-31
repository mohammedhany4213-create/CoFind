namespace coFind.Application.DTOs;

public record UserProfileResponse(
    int UserId,
    string Name,
    string Email,
    string WhatsappNumber,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
