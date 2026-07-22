using ProsocAPI.Models.Authentication;

namespace Prosoc.Services
{
    public interface ISimpleJwtService
    {
        string GenerateToken(Utilisateur user);
        bool ValidateToken(string token);
        Utilisateur? GetUserFromToken(string token);
    }
}
