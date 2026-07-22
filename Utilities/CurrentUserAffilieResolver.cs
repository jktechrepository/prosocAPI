using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Prosoc.Data;

namespace Prosoc.Utilities
{
    /// <summary>
    /// Résout l'identifiant affilié depuis le JWT ou la fiche utilisateur.
    /// </summary>
    public static class CurrentUserAffilieResolver
    {
        public static async Task<int> ResolveAffilieIdAsync(
            ClaimsPrincipal user,
            ProsocDbContext db,
            CancellationToken ct = default)
        {
            var affilieIdClaim = user.FindFirst("AffilieId")?.Value;
            if (int.TryParse(affilieIdClaim, out var fromClaim) && fromClaim > 0)
                return fromClaim;

            var userIdClaim = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? user.FindFirst("uid")?.Value
                ?? user.FindFirst("UserId")?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
                return 0;

            var affilieId = await db.Utilisateurs.AsNoTracking()
                .Where(u => u.IdUtilisateur == userId)
                .Select(u => u.AffilieId)
                .FirstOrDefaultAsync(ct);

            return affilieId is > 0 ? affilieId.Value : 0;
        }
    }
}
