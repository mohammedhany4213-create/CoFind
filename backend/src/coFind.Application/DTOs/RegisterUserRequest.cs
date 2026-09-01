using System.ComponentModel.DataAnnotations;

namespace coFind.Application.DTOs;

public record RegisterUserRequest(
    [Required, MinLength(2), MaxLength(50)] string Name,
    [Required, EmailAddress, MaxLength(100)] string Email,
    [Required, MinLength(8), MaxLength(100)] string Password,
    [Required, Phone, MaxLength(15)] string WhatsappNumber
);