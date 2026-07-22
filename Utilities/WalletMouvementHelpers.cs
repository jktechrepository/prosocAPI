using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;

namespace Prosoc.Utilities
{
    public static class WalletMouvementHelpers
    {
        public static async Task<List<WalletMouvementReadDto>> ToReadDtosAsync(
            ProsocDbContext db,
            IReadOnlyList<WalletMouvement> mouvements,
            CancellationToken ct = default)
        {
            if (mouvements.Count == 0)
                return new List<WalletMouvementReadDto>();

            var collecteIds = mouvements
                .Where(m => WalletMouvementSources.IsCommissionCollecteSource(m.Source))
                .Select(m => WalletMouvementDescriptionBuilder.TryExtractCollecteId(m.Description))
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();

            var collectesById = collecteIds.Count == 0
                ? new Dictionary<int, Collecte>()
                : await db.Collectes
                    .Include(c => c.Affilie)
                    .Where(c => collecteIds.Contains(c.IdCollecte))
                    .ToDictionaryAsync(c => c.IdCollecte, ct);

            return mouvements
                .Select(m => ToReadDto(m, ResolveCollecte(m, collectesById)))
                .ToList();
        }

        public static WalletMouvementReadDto ToReadDto(
            WalletMouvement m,
            Collecte? collecte = null) => new()
        {
            IdWalletMouvement = m.IdWalletMouvement,
            WalletId = m.WalletId,
            Montant = m.Montant,
            TypeOperation = m.TypeOperation,
            Source = m.Source,
            Description = WalletMouvementDescriptionBuilder.BuildDisplayDescription(m, collecte),
            DateOperation = m.DateOperation,
            WalletAgentId = m.Wallet?.AgentId,
            AgentNom = m.Wallet?.Agent?.NomComplet,
            AgentMatricule = m.Wallet?.Agent?.Matricule,
            DeviseId = m.DeviseId,
            DeviseCode = m.Devise?.Code,
            DeviseNom = m.Devise?.Nom,
            DeviseSymbole = m.Devise?.Symbole
        };

        private static Collecte? ResolveCollecte(
            WalletMouvement mouvement,
            IReadOnlyDictionary<int, Collecte> collectesById)
        {
            if (!WalletMouvementSources.IsCommissionCollecteSource(mouvement.Source))
                return null;

            var collecteId = WalletMouvementDescriptionBuilder.TryExtractCollecteId(mouvement.Description);
            if (!collecteId.HasValue)
                return null;

            collectesById.TryGetValue(collecteId.Value, out var collecte);
            return collecte;
        }
    }
}
