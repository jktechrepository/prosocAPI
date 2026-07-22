using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.Pagination;
using ProsocAPI.Services;

namespace ProsocAPI.Services.Repositories
{
    public class DashboardCaissierService : IDashboardCaissierRepository
    {
        private readonly ProsocDbContext _db;
        private readonly ILogger<DashboardCaissierService> _logger;

        public DashboardCaissierService(ProsocDbContext db, ILogger<DashboardCaissierService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<CaissierKpisDto> GetKpisAsync(int utilisateurId, CancellationToken ct = default)
        {
            var today = DateTime.Today;
            var weekStart = today.AddDays(-7);
            var monthStart = today.AddDays(-30);

            var devisePrincipaleCode = await _db.Devises
                .AsNoTracking()
                .Where(d => d.EstDevisePrincipale && d.Statut)
                .Select(d => d.Code)
                .FirstOrDefaultAsync(ct);

            var collectesQuery = _db.Collectes.AsNoTracking()
                .Where(c => c.OperateurUtilisateurId == utilisateurId);

            var collectesDuJour = collectesQuery.Where(c => c.DateCollecte >= today);
            var collectesMois = collectesQuery.Where(c => c.DateCollecte >= monthStart);

            var montantDuJour = await SumMontantAsync(collectesDuJour, ct);
            var montantSemaine = await SumMontantAsync(
                collectesQuery.Where(c => c.DateCollecte >= weekStart), ct);
            var montantMois = await SumMontantAsync(collectesMois, ct);

            var nombreDuJour = await collectesDuJour.CountAsync(ct);
            var nombreMois = await collectesMois.CountAsync(ct);

            var totalMois = nombreMois;
            var tauxSucces = totalMois > 0
                ? await collectesMois.CountAsync(
                    c => c.StatutPaiement != null
                         && CollecteStatutPaiementRegles.ValeursSqlValideEtLegacy.Contains(c.StatutPaiement), ct) * 100m / totalMois
                : 0m;

            var adhesionsDuJour = await _db.Adhesions.AsNoTracking()
                .CountAsync(a => a.UtilisateurId == utilisateurId && a.DateCreation >= today, ct);

            var sortiesDuJour = await _db.MouvementsCaisses.AsNoTracking()
                .Where(m => m.UtilisateurId == utilisateurId
                    && m.TypeOperation == MouvementCaisseTypes.Sortie
                    && m.Source == MouvementCaisseSources.RetraitAgent
                    && m.DateOperation >= today
                    && m.Statut)
                .ToListAsync(ct);

            return new CaissierKpisDto
            {
                MontantDuJour = montantDuJour,
                MontantSemaine = montantSemaine,
                MontantMois = montantMois,
                NombreCollectesDuJour = nombreDuJour,
                NombreCollectesMois = nombreMois,
                MontantMoyen = nombreMois > 0 ? montantMois / nombreMois : 0,
                TauxSucces = tauxSucces,
                NombreAdhesionsDuJour = adhesionsDuJour,
                NombreSortiesDuJour = sortiesDuJour.Count,
                MontantSortiesDuJour = sortiesDuJour.Sum(m => m.Montant),
                DevisePrincipaleCode = devisePrincipaleCode
            };
        }

        public async Task<List<CaissierCollecteDto>> GetCollectesRecentesAsync(
            int utilisateurId, int limit = 50, CancellationToken ct = default)
        {
            if (limit <= 0) limit = 50;

            return await _db.Collectes.AsNoTracking()
                .Include(c => c.Affilie)
                .Where(c => c.OperateurUtilisateurId == utilisateurId)
                .OrderByDescending(c => c.DateCollecte)
                .Take(limit)
                .Select(c => new CaissierCollecteDto
                {
                    IdCollecte = c.IdCollecte,
                    DateCollecte = c.DateCollecte,
                    Montant = c.MontantDevisePrincipale ?? c.Montant,
                    TypeCollecte = c.TypeCollecte.ToString(),
                    Statut = c.StatutPaiement ?? "En attente",
                    Reference = c.ReferencePaiement ?? string.Empty,
                    NomAffilie = c.Affilie.NomComplet ?? string.Empty,
                    ModePaiement = c.ModePaiement ?? string.Empty,
                    Notes = c.Observation
                })
                .ToListAsync(ct);
        }

        public async Task<PaginatedResponse<CaissierCollecteDto>> GetCollectesHistoriqueAsync(
            int utilisateurId,
            GuichetCollecteHistoriqueFiltreDto filtres,
            PaginationRequest pagination,
            CancellationToken ct = default)
        {
            if (pagination.Page <= 0) pagination.Page = 1;
            if (pagination.PageSize <= 0) pagination.PageSize = 20;

            var query = BuildCollectesOperateurQuery(utilisateurId, filtres);

            var totalItems = await query.CountAsync(ct);
            var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pagination.PageSize);

            var data = await query
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(c => new CaissierCollecteDto
                {
                    IdCollecte = c.IdCollecte,
                    DateCollecte = c.DateCollecte,
                    Montant = c.MontantDevisePrincipale ?? c.Montant,
                    TypeCollecte = c.TypeCollecte.ToString(),
                    Statut = c.StatutPaiement ?? "En attente",
                    Reference = c.ReferencePaiement ?? string.Empty,
                    NomAffilie = c.Affilie!.NomComplet ?? string.Empty,
                    ModePaiement = c.ModePaiement ?? string.Empty,
                    Notes = c.Observation
                })
                .ToListAsync(ct);

            return new PaginatedResponse<CaissierCollecteDto>
            {
                Data = data,
                CurrentPage = pagination.Page,
                PageSize = pagination.PageSize,
                TotalItems = totalItems,
                TotalPages = totalPages,
                HasNextPage = pagination.Page < totalPages,
                HasPreviousPage = pagination.Page > 1
            };
        }

        private IQueryable<Collecte> BuildCollectesOperateurQuery(
            int utilisateurId,
            GuichetCollecteHistoriqueFiltreDto filtres)
        {
            var query = _db.Collectes.AsNoTracking()
                .Include(c => c.Affilie)
                .Where(c => c.OperateurUtilisateurId == utilisateurId);

            if (filtres.DateDebut.HasValue)
                query = query.Where(c => c.DateCollecte >= filtres.DateDebut.Value);
            if (filtres.DateFin.HasValue)
                query = query.Where(c => c.DateCollecte <= filtres.DateFin.Value);
            if (!string.IsNullOrWhiteSpace(filtres.ModePaiement))
            {
                var mode = filtres.ModePaiement.Trim().ToUpperInvariant();
                query = query.Where(c => c.ModePaiement != null && c.ModePaiement.ToUpper() == mode);
            }

            return query.OrderByDescending(c => c.DateCollecte);
        }

        public async Task<List<CaissierRepartitionDto>> GetRepartitionParTypeAsync(
            int utilisateurId, CancellationToken ct = default)
        {
            var today = DateTime.Today;
            var rows = await _db.Collectes.AsNoTracking()
                .Where(c => c.OperateurUtilisateurId == utilisateurId && c.DateCollecte >= today)
                .GroupBy(c => c.TypeCollecte)
                .Select(g => new
                {
                    Type = g.Key,
                    Montant = g.Sum(c => (double)(c.MontantDevisePrincipale ?? c.Montant)),
                    Nombre = g.Count()
                })
                .ToListAsync(ct);

            var total = rows.Sum(r => r.Montant);
            return rows.Select(r => new CaissierRepartitionDto
            {
                Libelle = r.Type.ToString(),
                Montant = (decimal)r.Montant,
                Nombre = r.Nombre,
                Pourcentage = total > 0 ? (decimal)r.Montant / (decimal)total * 100 : 0
            }).ToList();
        }

        public async Task<List<CaissierRepartitionDto>> GetRepartitionParModeAsync(
            int utilisateurId, CancellationToken ct = default)
        {
            var today = DateTime.Today;
            var rows = await _db.Collectes.AsNoTracking()
                .Where(c => c.OperateurUtilisateurId == utilisateurId && c.DateCollecte >= today)
                .GroupBy(c => c.ModePaiement ?? "INCONNU")
                .Select(g => new
                {
                    Mode = g.Key,
                    Montant = g.Sum(c => (double)(c.MontantDevisePrincipale ?? c.Montant)),
                    Nombre = g.Count()
                })
                .ToListAsync(ct);

            var total = rows.Sum(r => r.Montant);
            return rows.Select(r => new CaissierRepartitionDto
            {
                Libelle = r.Mode,
                Montant = (decimal)r.Montant,
                Nombre = r.Nombre,
                Pourcentage = total > 0 ? (decimal)r.Montant / (decimal)total * 100 : 0
            }).ToList();
        }

        public async Task<List<CaissierAdhesionDuJourDto>> GetAdhesionsDuJourAsync(
            int utilisateurId, CancellationToken ct = default)
        {
            var today = DateTime.Today;

            return await _db.Adhesions.AsNoTracking()
                .Include(a => a.Affilie)
                .Where(a => a.UtilisateurId == utilisateurId && a.DateCreation >= today)
                .OrderByDescending(a => a.DateCreation)
                .Select(a => new CaissierAdhesionDuJourDto
                {
                    IdAdhesion = a.IdAdhesion,
                    AffilieId = a.AffilieId,
                    NomAffilie = a.Affilie.NomComplet ?? string.Empty,
                    StatutDossier = a.StatutDossier,
                    DateCreation = a.DateCreation
                })
                .ToListAsync(ct);
        }

        public async Task<DashboardCaissierDto> GetDashboardSummaryAsync(int utilisateurId, CancellationToken ct = default)
        {
            _logger.LogInformation("Dashboard caissier pour utilisateur {UtilisateurId}", utilisateurId);

            var kpis = await GetKpisAsync(utilisateurId, ct);
            var collectesRecentes = await GetCollectesRecentesAsync(utilisateurId, 20, ct);

            return new DashboardCaissierDto
            {
                Kpis = kpis,
                CollectesRecentes = collectesRecentes,
                RepartitionParType = await GetRepartitionParTypeAsync(utilisateurId, ct),
                RepartitionParMode = await GetRepartitionParModeAsync(utilisateurId, ct),
                AdhesionsDuJour = await GetAdhesionsDuJourAsync(utilisateurId, ct),
                DerniereMiseAJour = DateTime.Now
            };
        }

        private static async Task<decimal> SumMontantAsync(
            IQueryable<Collecte> query, CancellationToken ct)
        {
            var collectes = await query.ToListAsync(ct);
            return DashboardDeviseConsolidation.SommerCollectesEnDevisePrincipale(collectes);
        }
    }
}
