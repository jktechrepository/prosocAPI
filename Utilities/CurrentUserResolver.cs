using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Prosoc.Utilities
{
    /// <summary>
    /// Résout l'identifiant utilisateur depuis le JWT (claims Sub, uid, UserId, IdUtilisateur).
    /// </summary>
    public static class CurrentUserResolver
    {
        public static int? TryGetCurrentUtilisateurId(ClaimsPrincipal? user)
        {
            if (user == null)
                return null;

            var userIdClaim = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? user.FindFirst("uid")?.Value
                ?? user.FindFirst("UserId")?.Value
                ?? user.FindFirst("IdUtilisateur")?.Value;

            return int.TryParse(userIdClaim, out var userId) && userId > 0 ? userId : null;
        }

        public static int GetCurrentUtilisateurId(ClaimsPrincipal user)
        {
            var id = TryGetCurrentUtilisateurId(user);
            if (id is > 0)
                return id.Value;

            throw new UnauthorizedAccessException("Utilisateur non identifié.");
        }
    }
}
