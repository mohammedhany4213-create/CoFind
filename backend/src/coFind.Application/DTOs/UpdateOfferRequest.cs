namespace coFind.Application.DTOs;

public record UpdateOfferRequest(
    string Title,
    string Description,
    string Role,
    ICollection<string> Skills,
    string Industry,
    bool IsAvailable,
    string? Location
);
