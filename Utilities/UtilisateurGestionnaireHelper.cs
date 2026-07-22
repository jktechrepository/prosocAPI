using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prosoc.Data;
using ProsocAPI.Models.DTOs.Authentication;

namespace ProsocAPI.Utilities
{
    public static class UtilisateurGestionnaireHelper
    {
        public static async Task EnrichGestionnaireAffilieAsync(
            UtilisateurDto dto,
            ProsocDbContext db,
            int? affilieId,
            ILogger? logger = null,
            CancellationToken ct = default)
        {
            if (!affilieId.HasValue)
                return;

            var adhesion = await db.Adhesions
                .AsNoTracking()
                .Include(a => a.AgentCreateur)
                .FirstOrDefaultAsync(a => a.AffilieId == affilieId.Value, ct);

            if (adhesion == null)
            {
                logger?.LogWarning(
                    "Aucune adhésion trouvée pour l'affilié {AffilieId} lors de l'enrichissement utilisateur",
                    affilieId.Value);
                return;
            }

            dto.IdAgentGestionnaireCompte = adhesion.AgentId;
            dto.NomAgentGestionnaireCompte = adhesion.AgentCreateur?.NomComplet;
            dto.MatriculeAgentGestionnaireCompte = adhesion.AgentCreateur?.Matricule;
        }
    }
}
