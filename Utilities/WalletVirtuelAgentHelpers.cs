using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;

namespace Prosoc.Utilities
{
    public static class WalletVirtuelAgentHelpers
    {
        public static WalletVirtuelAgentReadDto ToReadDto(WalletVirtuelAgent w) => new()
        {
            IdWalletVirtuelAgent = w.IdWalletVirtuelAgent,
            AgentId = w.AgentId,
            DeviseId = w.DeviseId,
            DeviseCode = w.Devise?.Code,
            DeviseNom = w.Devise?.Nom,
            DeviseSymbole = w.Devise?.Symbole,
            SoldeVirtuel = w.SoldeVirtuel,
            DateCreation = w.DateCreation,
            DateModification = w.DateModification,
            Statut = w.Statut,
            AgentNom = w.Agent?.NomComplet,
            AgentMatricule = w.Agent?.Matricule
        };

        public static async Task<int> ResolveDeviseIdAsync(
            ProsocDbContext db,
            int? deviseId,
            CancellationToken ct = default)
        {
            if (deviseId is > 0)
            {
                var devise = await db.Devises.AsNoTracking()
                    .FirstOrDefaultAsync(d => d.IdDevise == deviseId.Value && d.Statut, ct);
                if (devise == null)
                    throw new ArgumentException($"Devise {deviseId} introuvable ou inactive.");

                return devise.IdDevise;
            }

            var principale = await db.Devises.AsNoTracking()
                .FirstOrDefaultAsync(d => d.EstDevisePrincipale && d.Statut, ct)
                ?? throw new InvalidOperationException(
                    "Aucune devise principale active configurée. Précisez DeviseId.");

            return principale.IdDevise;
        }
    }
}
