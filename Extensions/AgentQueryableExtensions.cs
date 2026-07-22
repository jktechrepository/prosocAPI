using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using Prosoc.Utilities;
using ProsocAPI.Models.Core;

namespace ProsocAPI.Extensions
{
    /// <summary>
    /// Visibilité des agents selon <c>Role.Niveau</c> :
    /// plus le chiffre est petit, plus le rôle est haut.
    /// Un caller ne voit que les agents dont MIN(Niveau) >= son propre MIN(Niveau).
    /// </summary>
    public static class AgentQueryableExtensions
    {
        public const int DefaultJuniorNiveau = 999;

        /// <summary>
        /// Résout le niveau hiérarchique du caller (MIN des Niveau des rôles JWT croisés avec Roles).
        /// </summary>
        public static async Task<int> ResolveCallerMinNiveauAsync(
            ProsocDbContext db,
            ClaimsPrincipal user,
            CancellationToken ct = default)
        {
            var roleNames = user.FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct(StringComparer.Ordinal)
                .ToList();

            if (roleNames.Count == 0)
                return DefaultJuniorNiveau;

            var niveaux = await db.Roles
                .AsNoTracking()
                .Where(r => r.Statut && roleNames.Contains(r.Nom))
                .Select(r => r.Niveau ?? DefaultJuniorNiveau)
                .ToListAsync(ct);

            return niveaux.Count == 0 ? DefaultJuniorNiveau : niveaux.Min();
        }

        /// <summary>
        /// Filtre les agents visibles pour <paramref name="callerMinNiveau"/>.
        /// SuperAdmin (niveau 0) : aucun filtre (inclut agents sans rôle).
        /// Sinon : agents ayant au moins un rôle lié, avec MIN(Niveau) &gt;= callerMinNiveau.
        /// </summary>
        public static IQueryable<Agent> ApplyRoleNiveauVisibility(
            this IQueryable<Agent> query,
            ProsocDbContext db,
            int callerMinNiveau)
        {
            if (callerMinNiveau <= 0)
                return query;

            return query.Where(a =>
                db.UserRoles.Any(ur =>
                    ur.Statut
                    && ur.Utilisateur.Statut
                    && ur.Utilisateur.AgentId == a.IdAgent
                    && ur.Role.Statut)
                && db.UserRoles
                    .Where(ur =>
                        ur.Statut
                        && ur.Utilisateur.Statut
                        && ur.Utilisateur.AgentId == a.IdAgent
                        && ur.Role.Statut)
                    .Min(ur => ur.Role.Niveau ?? DefaultJuniorNiveau) >= callerMinNiveau);
        }

        /// <summary>
        /// Indique si un agent donné est visible pour le niveau caller (même règle que la liste).
        /// </summary>
        public static async Task<bool> IsAgentVisibleAsync(
            ProsocDbContext db,
            int agentId,
            int callerMinNiveau,
            CancellationToken ct = default)
        {
            return await db.Agents
                .AsNoTracking()
                .Where(a => a.IdAgent == agentId)
                .ApplyRoleNiveauVisibility(db, callerMinNiveau)
                .AnyAsync(ct);
        }

        /// <summary>
        /// MIN(Role.Niveau) des rôles actifs liés à l'agent via Utilisateur / UserRoles.
        /// <c>null</c> si aucun rôle lié.
        /// </summary>
        public static async Task<int?> ResolveAgentMinNiveauAsync(
            ProsocDbContext db,
            int agentId,
            CancellationToken ct = default)
        {
            var niveaux = await db.UserRoles
                .AsNoTracking()
                .Where(ur =>
                    ur.Statut
                    && ur.Utilisateur.Statut
                    && ur.Utilisateur.AgentId == agentId
                    && ur.Role.Statut)
                .Select(ur => ur.Role.Niveau ?? DefaultJuniorNiveau)
                .ToListAsync(ct);

            return niveaux.Count == 0 ? null : niveaux.Min();
        }

        /// <summary>
        /// AgentId du caller JWT (via Utilisateur.AgentId), ou null si compte sans fiche agent.
        /// </summary>
        public static async Task<int?> ResolveCallerAgentIdAsync(
            ProsocDbContext db,
            ClaimsPrincipal user,
            CancellationToken ct = default)
        {
            var utilisateurId = CurrentUserResolver.TryGetCurrentUtilisateurId(user);
            if (utilisateurId is not > 0)
                return null;

            return await db.Utilisateurs
                .AsNoTracking()
                .Where(u => u.IdUtilisateur == utilisateurId.Value && u.Statut)
                .Select(u => u.AgentId)
                .FirstOrDefaultAsync(ct);
        }

        /// <summary>
        /// Autorise la recharge / le crédit manuel du wallet virtuel d'un agent cible :
        /// SuperAdmin (niveau &lt;= 0) : oui ;
        /// auto-recharge : non ;
        /// cible sans rôle : non ;
        /// sinon uniquement si targetMinNiveau &gt; callerMinNiveau (strictement plus junior).
        /// </summary>
        public static async Task<bool> CanRechargeWalletVirtuelAsync(
            ProsocDbContext db,
            ClaimsPrincipal user,
            int targetAgentId,
            CancellationToken ct = default)
        {
            var callerMinNiveau = await ResolveCallerMinNiveauAsync(db, user, ct);
            if (callerMinNiveau <= 0)
                return true;

            var callerAgentId = await ResolveCallerAgentIdAsync(db, user, ct);
            if (callerAgentId.HasValue && callerAgentId.Value == targetAgentId)
                return false;

            var targetMinNiveau = await ResolveAgentMinNiveauAsync(db, targetAgentId, ct);
            if (!targetMinNiveau.HasValue)
                return false;

            return targetMinNiveau.Value > callerMinNiveau;
        }
    }
}
