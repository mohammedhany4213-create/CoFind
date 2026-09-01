using System.ComponentModel.DataAnnotations;

namespace coFind.Application.DTOs;

public record LoginUserRequest(
    [Required, EmailAddress, MaxLength(100)] string Email,
    [Required, MinLength(8), MaxLength(100)] string Password
);
