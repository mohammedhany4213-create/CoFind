using System.Security.Claims;

namespace coFind.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static bool TryGetUserId(this ClaimsPrincipal user, out int userId)
        => int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
}
