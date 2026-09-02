using System.ComponentModel.DataAnnotations;

namespace coFind.Application.DTOs;

public record UpdateOfferRequest(
    [Required, MinLength(3), MaxLength(150)] string Title,
    [Required, MinLength(10), MaxLength(2000)] string Description,
    [Required, MinLength(2), MaxLength(100)] string Role,
    [Required, MinLength(1)] ICollection<string> Skills,
    [Required, MinLength(2), MaxLength(100)] string Industry,
    bool IsAvailable,
    [MaxLength(150)] string? Location
);
