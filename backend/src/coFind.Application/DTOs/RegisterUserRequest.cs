using System.ComponentModel.DataAnnotations;

namespace coFind.Application.DTOs;

public record RegisterUserRequest(
    [property: Required, MinLength(2), MaxLength(50)] string Name,
    [property: Required, EmailAddress, MaxLength(100)] string Email,
    [property: Required, MinLength(8), MaxLength(100)] string Password,
    [property: Required, Phone, MaxLength(15)] string WhatsappNumber
);