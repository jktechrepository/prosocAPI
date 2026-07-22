using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prosoc.Data;

namespace Prosoc.Utilities
{
    public static class AntecedentDependantValidationHelper
    {
        /// <summary>
        /// Valide que le dépendant existe et appartient à l'affilié indiqué.
        /// Retourne null si dependantId est absent ou si la validation réussit.
        /// </summary>
        public static async Task<ActionResult?> ValidateDependantForAffilieAsync(
            ProsocDbContext db,
            int affilieId,
            int? dependantId,
            CancellationToken ct = default)
        {
            if (!dependantId.HasValue || dependantId.Value <= 0)
                return null;

            var dependantAffilieId = await db.Dependants
                .AsNoTracking()
                .Where(d => d.IdDependant == dependantId.Value)
                .Select(d => (int?)d.AffilieId)
                .FirstOrDefaultAsync(ct);

            if (dependantAffilieId is null)
            {
                return new BadRequestObjectResult(new
                {
                    message = $"Dépendant {dependantId.Value} introuvable."
                });
            }

            if (dependantAffilieId.Value != affilieId)
            {
                return new BadRequestObjectResult(new
                {
                    message = "Le dépendant n'appartient pas à cet affilié."
                });
            }

            return null;
        }
    }
}
