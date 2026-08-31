namespace coFind.Application.DTOs;

public record CreateOfferResponse(
    int OfferId,
    int UserId,
    string Title,
    string Description,
    string Role,
    ICollection<string> Skills,
    string Industry,
    bool IsAvailable,
    string? Location,
    bool IsActive,
    DateTime CreatedAt
);
