using System.ComponentModel.DataAnnotations;

namespace coFind.Application.DTOs;

public record UpdateUserRequest(
    [property: Required, MinLength(2), MaxLength(50)] string Name,
    [property: Required, Phone, MaxLength(15)] string WhatsappNumber
);