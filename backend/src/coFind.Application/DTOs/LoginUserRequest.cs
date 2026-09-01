using System.ComponentModel.DataAnnotations;

namespace coFind.Application.DTOs;

public record LoginUserRequest(
    [property: Required, EmailAddress, MaxLength(100)] string Email,
    [property: Required, MinLength(8), MaxLength(100)] string Password
);
