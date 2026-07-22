using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;

namespace Prosoc.Utilities
{
    public static class WalletVirtuelMouvementHelpers
    {
        public static IQueryable<WalletVirtuelMouvement> MouvementsQuery(ProsocDbContext db) =>
            db.WalletVirtuelMouvements
                .Include(m => m.WalletVirtuel)
                .ThenInclude(w => w!.Agent)
                .Include(m => m.WalletVirtuel)
                .ThenInclude(w => w!.Devise)
                .Include(m => m.Devise)
                .Include(m => m.OperateurUtilisateur)
                .ThenInclude(u => u!.Agent)
                .AsNoTracking();

        public static IQueryable<WalletVirtuelMouvement> ApplyFiltres(
            IQueryable<WalletVirtuelMouvement> query,
            WalletVirtuelMouvementFiltreDto? filtres)
        {
            if (filtres == null)
                return query;

            if (!string.IsNullOrWhiteSpace(filtres.TypeOperation))
                query = query.Where(m => m.TypeOperation == filtres.TypeOperation);

            if (!string.IsNullOrWhiteSpace(filtres.Source))
                query = query.Where(m => m.Source == filtres.Source);

            if (filtres.DateDebut.HasValue)
                query = query.Where(m => m.DateOperation >= filtres.DateDebut.Value);

            if (filtres.DateFin.HasValue)
                query = query.Where(m => m.DateOperation <= filtres.DateFin.Value);

            return query;
        }

        public static async Task<List<WalletVirtuelMouvementReadDto>> ToReadDtosAsync(
            ProsocDbContext db,
            IReadOnlyList<WalletVirtuelMouvement> mouvements,
            CancellationToken ct = default)
        {
            if (mouvements.Count == 0)
                return new List<WalletVirtuelMouvementReadDto>();

            var collecteIds = mouvements
                .Where(m => m.Source == WalletVirtuelMouvementSources.CollecteCompteVirtuel && m.ReferenceExterne.HasValue)
                .Select(m => m.ReferenceExterne!.Value)
                .Distinct()
                .ToList();

            var collectesById = collecteIds.Count == 0
                ? new Dictionary<int, Collecte>()
                : await db.Collectes
                    .Include(c => c.Affilie)
                    .Where(c => collecteIds.Contains(c.IdCollecte))
                    .ToDictionaryAsync(c => c.IdCollecte, ct);

            return mouvements.Select(m => ToReadDto(m, collectesById)).ToList();
        }

        public static WalletVirtuelMouvementReadDto ToReadDto(
            WalletVirtuelMouvement m,
            IReadOnlyDictionary<int, Collecte>? collectesById = null)
        {
            var devise = m.Devise ?? m.WalletVirtuel?.Devise;
            Collecte? collecte = null;
            if (m.Source == WalletVirtuelMouvementSources.CollecteCompteVirtuel
                && m.ReferenceExterne.HasValue
                && collectesById != null)
            {
                collectesById.TryGetValue(m.ReferenceExterne.Value, out collecte);
            }

            return new WalletVirtuelMouvementReadDto
            {
                IdWalletVirtuelMouvement = m.IdWalletVirtuelMouvement,
                WalletVirtuelId = m.WalletVirtuelId,
                Montant = m.Montant,
                TypeOperation = m.TypeOperation,
                Source = m.Source,
                SourceLibelle = WalletVirtuelMouvementSources.GetLibelle(m.Source),
                Description = m.Description,
                ReferenceExterne = m.ReferenceExterne,
                DateOperation = m.DateOperation,
                AgentId = m.WalletVirtuel?.AgentId,
                AgentNom = m.WalletVirtuel?.Agent?.NomComplet,
                AgentMatricule = m.WalletVirtuel?.Agent?.Matricule,
                DeviseId = m.DeviseId ?? devise?.IdDevise,
                DeviseCode = devise?.Code,
                DeviseNom = devise?.Nom,
                DeviseSymbole = devise?.Symbole,
                SoldeAvant = m.SoldeAvant,
                SoldeApres = m.SoldeApres,
                OperateurUtilisateurId = m.OperateurUtilisateurId,
                OperateurNom = m.OperateurUtilisateur?.NomUtilisateur,
                IdAgentFrom = m.OperateurUtilisateur?.AgentId,
                NomAgentFrom = m.OperateurUtilisateur?.Agent?.NomComplet,
                CollecteId = collecte?.IdCollecte ?? (m.Source == WalletVirtuelMouvementSources.CollecteCompteVirtuel ? m.ReferenceExterne : null),
                AffilieNom = collecte?.Affilie?.NomComplet,
                AffilieCode = collecte?.Affilie?.CodeAdhesion
            };
        }
    }
}
