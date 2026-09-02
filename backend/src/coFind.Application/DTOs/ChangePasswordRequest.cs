using System.ComponentModel.DataAnnotations;

namespace coFind.Application.DTOs;

public record ChangePasswordRequest(
    [Required, MinLength(8), MaxLength(100)] string CurrentPassword,
    [Required, MinLength(8), MaxLength(100)] string NewPassword
);