using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Prosoc.Data;

namespace Prosoc.Utilities
{
    /// <summary>
    /// Résout l'identifiant agent depuis le JWT ou la fiche utilisateur.
    /// </summary>
    public static class CurrentUserAgentResolver
    {
        public const string AgentAtRoleName = "Agent (AT)";
        public const string AgentAaRoleName = "Agent (AA)";

        public static async Task<int> ResolveAgentIdAsync(
            ClaimsPrincipal user,
            ProsocDbContext db,
            CancellationToken ct = default)
        {
            var agentIdClaim = user.FindFirst("AgentId")?.Value;
            if (int.TryParse(agentIdClaim, out var fromClaim) && fromClaim > 0)
                return fromClaim;

            var userId = CurrentUserResolver.TryGetCurrentUtilisateurId(user);
            if (userId is not > 0)
                return 0;

            var agentId = await db.Utilisateurs.AsNoTracking()
                .Where(u => u.IdUtilisateur == userId)
                .Select(u => u.AgentId)
                .FirstOrDefaultAsync(ct);

            return agentId is > 0 ? agentId.Value : 0;
        }

        public static async Task<int> RequireAgentIdAsync(
            ClaimsPrincipal user,
            ProsocDbContext db,
            CancellationToken ct = default)
        {
            var agentId = await ResolveAgentIdAsync(user, db, ct);
            if (agentId > 0)
                return agentId;

            throw new UnauthorizedAccessException(
                "Aucun agent rattaché à cet utilisateur.");
        }
    }
}
