using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Prosoc.Data;

namespace Prosoc.Utilities
{
    /// <summary>
    /// Résout l'identifiant assureur partenaire depuis le JWT ou la fiche utilisateur.
    /// </summary>
    public static class CurrentUserAssureurResolver
    {
        public const string AssureurRoleName = "Assureur";

        public static bool IsAssureur(ClaimsPrincipal user) =>
            user.IsInRole(AssureurRoleName);

        public static async Task<int> ResolveAssureurIdAsync(
            ClaimsPrincipal user,
            ProsocDbContext db,
            CancellationToken ct = default)
        {
            var assureurClaim = user.FindFirst("AssureurId")?.Value;
            if (int.TryParse(assureurClaim, out var fromClaim) && fromClaim > 0)
                return fromClaim;

            var userIdClaim = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? user.FindFirst("uid")?.Value
                ?? user.FindFirst("UserId")?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
                return 0;

            var assureurId = await db.Utilisateurs.AsNoTracking()
                .Where(u => u.IdUtilisateur == userId)
                .Select(u => u.AssureurId)
                .FirstOrDefaultAsync(ct);

            return assureurId is > 0 ? assureurId.Value : 0;
        }

        public static async Task<int> RequireAssureurIdAsync(
            ClaimsPrincipal user,
            ProsocDbContext db,
            CancellationToken ct = default)
        {
            var assureurId = await ResolveAssureurIdAsync(user, db, ct);
            if (assureurId > 0)
                return assureurId;

            throw new UnauthorizedAccessException(
                "Aucun assureur partenaire rattaché à cet utilisateur.");
        }
    }
}
