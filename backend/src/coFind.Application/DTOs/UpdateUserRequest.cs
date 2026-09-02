using System.ComponentModel.DataAnnotations;

namespace coFind.Application.DTOs;

public record UpdateUserRequest(
    [Required, MinLength(2), MaxLength(50)] string Name,
    [Required, Phone, MaxLength(15)] string WhatsappNumber
);