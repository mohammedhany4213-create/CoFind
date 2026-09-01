namespace coFind.Application.DTOs;

public record OfferListItemResponse(
    int OfferId,
    string Title,
    string Description,
    string Role,
    ICollection<string> Skills,
    string Industry,
    bool IsAvailable,
    string? Location,
    bool IsActive,
    DateTime CreatedAt,
    OfferOwnerResponse Owner
);

public record OfferOwnerResponse(
    int UserId,
    string Name
);
