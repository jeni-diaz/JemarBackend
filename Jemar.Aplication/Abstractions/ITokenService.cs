using Jemar.Domain.Entities;

namespace Jemar.Aplication.Abstractions
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}
