using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prosoc.Data;

namespace Prosoc.Utilities
{
    /// <summary>
    /// Périmètre espace membre : l'affilié connecté ne voit que son dossier, ses dépendants et sa personne de contact.
    /// </summary>
    public static class AffilieMemberScopeHelper
    {
        public const string RoleName = "Affilié";

        public static bool IsMembreAffilie(ClaimsPrincipal user) =>
            user.IsInRole(RoleName)
            && !user.IsInRole("Admin")
            && !user.IsInRole("SuperAdmin")
            && !user.IsInRole("IT");

        /// <summary>403 si l'utilisateur est un membre affilié (pas de listes globales).</summary>
        public static ActionResult? DenyListAccessForMembre(ClaimsPrincipal user, string resourceLabel)
        {
            if (!IsMembreAffilie(user))
                return null;

            return new ObjectResult(new
            {
                message = $"Accès refusé : la liste {resourceLabel} est réservée au personnel autorisé."
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        /// <summary>403/401 si le membre tente d'accéder aux données d'un autre affilié.</summary>
        public static async Task<ActionResult?> EnsureOwnAffilieScopeAsync(
            ClaimsPrincipal user,
            ProsocDbContext db,
            int targetAffilieId,
            CancellationToken ct = default)
        {
            if (!IsMembreAffilie(user))
                return null;

            var ownAffilieId = await CurrentUserAffilieResolver.ResolveAffilieIdAsync(user, db, ct);
            if (ownAffilieId <= 0)
            {
                return new UnauthorizedObjectResult(new
                {
                    message = "Utilisateur non authentifié ou non rattaché à un affilié."
                });
            }

            if (ownAffilieId != targetAffilieId)
            {
                return new ObjectResult(new
                {
                    message = "Accès refusé : vous ne pouvez consulter que votre propre dossier."
                })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }

            return null;
        }

        public static async Task<(int AffilieId, ActionResult? Error)> RequireOwnAffilieIdAsync(
            ClaimsPrincipal user,
            ProsocDbContext db,
            CancellationToken ct = default)
        {
            var affilieId = await CurrentUserAffilieResolver.ResolveAffilieIdAsync(user, db, ct);
            if (affilieId <= 0)
            {
                return (0, new UnauthorizedObjectResult(new
                {
                    message = "Utilisateur non authentifié ou non rattaché à un affilié."
                }));
            }

            return (affilieId, null);
        }

        /// <summary>403 si le membre tente d'accéder à une adhésion d'un autre affilié.</summary>
        public static async Task<ActionResult?> EnsureOwnAdhesionScopeAsync(
            ClaimsPrincipal user,
            ProsocDbContext db,
            int adhesionId,
            CancellationToken ct = default)
        {
            if (!IsMembreAffilie(user))
                return null;

            var affilieId = await db.Adhesions.AsNoTracking()
                .Where(a => a.IdAdhesion == adhesionId)
                .Select(a => (int?)a.AffilieId)
                .FirstOrDefaultAsync(ct);

            if (affilieId is null or <= 0)
                return null;

            return await EnsureOwnAffilieScopeAsync(user, db, affilieId.Value, ct);
        }

        /// <summary>403 si le membre tente d'accéder à un dépendant d'un autre affilié.</summary>
        public static async Task<ActionResult?> EnsureOwnDependantScopeAsync(
            ClaimsPrincipal user,
            ProsocDbContext db,
            int dependantId,
            CancellationToken ct = default)
        {
            if (!IsMembreAffilie(user))
                return null;

            var affilieId = await db.Dependants.AsNoTracking()
                .Where(d => d.IdDependant == dependantId)
                .Select(d => (int?)d.AffilieId)
                .FirstOrDefaultAsync(ct);

            if (affilieId is null or <= 0)
                return null;

            return await EnsureOwnAffilieScopeAsync(user, db, affilieId.Value, ct);
        }

        /// <summary>403 si le membre tente d'accéder à un antécédent d'un autre affilié.</summary>
        public static async Task<ActionResult?> EnsureOwnAntecedentScopeAsync(
            ClaimsPrincipal user,
            ProsocDbContext db,
            int antecedentId,
            CancellationToken ct = default)
        {
            if (!IsMembreAffilie(user))
                return null;

            var affilieId = await db.Antecedants.AsNoTracking()
                .Where(a => a.IdAntecedant == antecedentId)
                .Select(a => (int?)a.AffilieId)
                .FirstOrDefaultAsync(ct);

            if (affilieId is null or <= 0)
                return null;

            return await EnsureOwnAffilieScopeAsync(user, db, affilieId.Value, ct);
        }

        /// <summary>403 si l'utilisateur est un membre affilié (réservé au personnel).</summary>
        public static ActionResult? DenyStaffOnlyForMembre(ClaimsPrincipal user, string actionLabel)
        {
            if (!IsMembreAffilie(user))
                return null;

            return new ObjectResult(new
            {
                message = $"Accès refusé : {actionLabel} est réservé au personnel autorisé."
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }
}
