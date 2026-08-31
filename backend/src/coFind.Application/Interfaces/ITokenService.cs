using coFind.Domain.Entities;

namespace coFind.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}
