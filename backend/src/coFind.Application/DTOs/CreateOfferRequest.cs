using System.ComponentModel.DataAnnotations;

namespace coFind.Application.DTOs;

public record CreateOfferRequest(
    [property: Required, MinLength(3), MaxLength(150)] string Title,
    [property: Required, MinLength(10), MaxLength(2000)] string Description,
    [property: Required, MinLength(2), MaxLength(100)] string Role,
    [property: Required, MinLength(1)] ICollection<string> Skills,
    [property: Required, MinLength(2), MaxLength(100)] string Industry,
    bool IsAvailable,
    [property: MaxLength(150)] string? Location
);
