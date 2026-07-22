using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Prosoc.Data;

namespace Prosoc.Utilities
{
    /// <summary>
    /// Résout l'identifiant hôpital partenaire depuis le JWT ou la fiche utilisateur.
    /// </summary>
    public static class CurrentUserHopitalResolver
    {
        public const string AgentHopitalRoleName = "Agent Hôpital";

        public static bool IsAgentHopital(ClaimsPrincipal user) =>
            user.IsInRole(AgentHopitalRoleName);

        public static async Task<int> ResolveHopitalPartenaireIdAsync(
            ClaimsPrincipal user,
            ProsocDbContext db,
            CancellationToken ct = default)
        {
            var hopitalClaim = user.FindFirst("HopitalPartenaireId")?.Value;
            if (int.TryParse(hopitalClaim, out var fromClaim) && fromClaim > 0)
                return fromClaim;

            var userIdClaim = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? user.FindFirst("uid")?.Value
                ?? user.FindFirst("UserId")?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
                return 0;

            var hopitalId = await db.Utilisateurs.AsNoTracking()
                .Where(u => u.IdUtilisateur == userId)
                .Select(u => u.HopitalPartenaireId)
                .FirstOrDefaultAsync(ct);

            return hopitalId is > 0 ? hopitalId.Value : 0;
        }

        public static async Task<int> RequireHopitalPartenaireIdAsync(
            ClaimsPrincipal user,
            ProsocDbContext db,
            CancellationToken ct = default)
        {
            var hopitalId = await ResolveHopitalPartenaireIdAsync(user, db, ct);
            if (hopitalId > 0)
                return hopitalId;

            throw new UnauthorizedAccessException(
                "Aucun hôpital partenaire rattaché à cet utilisateur.");
        }
    }
}
