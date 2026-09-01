using System.ComponentModel.DataAnnotations;

namespace coFind.Application.DTOs;

public record UpdateUserRequest(
    [property: Required, MinLength(2), MaxLength(100)] string Name,
    [property: Required, MaxLength(30)] string WhatsappNumber
);
