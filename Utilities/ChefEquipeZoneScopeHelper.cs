using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prosoc.Data;

namespace Prosoc.Utilities
{
    /// <summary>
    /// Périmètre Chef d'équipe : lecture des AT partageant la même ZoneSocialeId.
    /// </summary>
    public static class ChefEquipeZoneScopeHelper
    {
        public const string RoleName = "Chef d'équipe";

        public static bool IsChefEquipe(ClaimsPrincipal user) =>
            user.IsInRole(RoleName);

        public static async Task<int?> GetZoneSocialeIdAsync(
            ProsocDbContext db,
            int agentId,
            CancellationToken ct = default)
        {
            var titularZoneId = await db.ZonesSociales.AsNoTracking()
                .Where(z => z.ChefEquipeAgentId == agentId)
                .Select(z => (int?)z.IdZoneSociale)
                .FirstOrDefaultAsync(ct);
            if (titularZoneId.HasValue)
                return titularZoneId;

            return await db.Agents.AsNoTracking()
                .Where(a => a.IdAgent == agentId)
                .Select(a => a.ZoneSocialeId)
                .FirstOrDefaultAsync(ct);
        }

        public static async Task<bool> IsTitulaireChefEquipeAsync(
            ProsocDbContext db,
            int agentId,
            CancellationToken ct = default) =>
            await db.ZonesSociales.AsNoTracking()
                .AnyAsync(z => z.ChefEquipeAgentId == agentId, ct);

        /// <summary>AT actifs de la zone du chef (hors le chef lui-même).</summary>
        public static async Task<List<int>> GetAgentIdsAtDansZoneAsync(
            ProsocDbContext db,
            int chefAgentId,
            CancellationToken ct = default)
        {
            var zoneId = await GetZoneSocialeIdAsync(db, chefAgentId, ct);
            if (zoneId is null or <= 0)
                return new List<int>();

            var atRoleId = await db.Roles.AsNoTracking()
                .Where(r => r.Nom == CurrentUserAgentResolver.AgentAtRoleName)
                .Select(r => r.IdRole)
                .FirstOrDefaultAsync(ct);
            if (atRoleId == 0)
                return new List<int>();

            return await db.Agents.AsNoTracking()
                .Where(a => a.ZoneSocialeId == zoneId && a.Statut && a.IdAgent != chefAgentId)
                .Where(a => db.Utilisateurs.Any(u =>
                    u.AgentId == a.IdAgent
                    && u.Statut
                    && u.UserRoles.Any(ur => ur.RoleId == atRoleId && ur.Statut)))
                .Select(a => a.IdAgent)
                .ToListAsync(ct);
        }

        /// <summary>403 si le chef tente d'accéder à un agent hors zone (son propre agentId autorisé).</summary>
        public static async Task<ActionResult?> EnsureAgentDansMaZoneAsync(
            ClaimsPrincipal user,
            ProsocDbContext db,
            int targetAgentId,
            CancellationToken ct = default)
        {
            if (!IsChefEquipe(user))
                return null;

            var chefAgentId = await CurrentUserAgentResolver.ResolveAgentIdAsync(user, db, ct);
            if (chefAgentId <= 0)
            {
                return new UnauthorizedObjectResult(new
                {
                    message = "Aucun agent rattaché à cet utilisateur Chef d'équipe."
                });
            }

            if (targetAgentId == chefAgentId)
                return null;

            var zoneId = await GetZoneSocialeIdAsync(db, chefAgentId, ct);
            if (zoneId is null or <= 0)
            {
                return new ObjectResult(new
                {
                    message = "Accès refusé : aucune zone sociale affectée à votre fiche agent."
                })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }

            var isTitular = await db.ZonesSociales.AsNoTracking()
                .AnyAsync(z => z.ChefEquipeAgentId == chefAgentId && z.IdZoneSociale == zoneId, ct);
            if (!isTitular)
            {
                return new ObjectResult(new
                {
                    message = "Accès refusé : vous n'êtes pas le chef d'équipe titulaire de cette zone."
                })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }

            var dansZone = await db.Agents.AsNoTracking()
                .AnyAsync(a => a.IdAgent == targetAgentId
                    && a.ZoneSocialeId == zoneId
                    && a.Statut, ct);

            if (!dansZone)
            {
                return new ObjectResult(new
                {
                    message = "Accès refusé : cet agent n'appartient pas à votre zone."
                })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }

            return null;
        }
    }
}
