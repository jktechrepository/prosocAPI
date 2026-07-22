using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.DashboardAssureur;

namespace ProsocAPI.Services.Repositories
{
    public class DashboardAssureurService : IDashboardAssureurRepository
    {
        private readonly ProsocDbContext _db;
        private readonly ILogger<DashboardAssureurService> _logger;

        public DashboardAssureurService(ProsocDbContext db, ILogger<DashboardAssureurService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<AssureurKpisDto> GetKpisAsync(int assureurId, CancellationToken ct = default)
        {
            var debutMois = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var affilieIds = QueryAffilieIdsForAssureur(assureurId);

            var nombreAffilies = await affilieIds.CountAsync(ct);

            var nombreDependants = await _db.Dependants.AsNoTracking()
                .CountAsync(d => d.Statut && affilieIds.Contains(d.AffilieId), ct);

            var nombreAntecedents = await _db.Antecedants.AsNoTracking()
                .CountAsync(a => a.Statut && affilieIds.Contains(a.AffilieId), ct);

            var souscriptionsQuery = QuerySouscriptionsForAssureur(assureurId);
            var nombreSouscriptionsActives = await souscriptionsQuery.CountAsync(s => s.Statut, ct);
            var nouvellesSouscriptionsMois = await souscriptionsQuery
                .CountAsync(s => s.DateCreation >= debutMois, ct);

            var montantCollectesMois = await SumCollectesMoisAsync(assureurId, debutMois, ct);

            var prestationIds = QueryPrestationIdsForAssureur(assureurId);

            var bonsMois = _db.BonsEnvoi.AsNoTracking()
                .Where(b => b.Statut && b.DateEmission >= debutMois && prestationIds.Contains(b.PrestationId));

            var bonsEmisMois = await bonsMois.CountAsync(ct);
            var bonsUtilisesMois = await bonsMois.CountAsync(b => b.EstUtilise, ct);

            var demandesEnAttente = await _db.DemandesBonEnvoi.AsNoTracking()
                .CountAsync(d =>
                    d.Statut &&
                    d.StatutDemande == "EN_ATTENTE" &&
                    prestationIds.Contains(d.PrestationId), ct);

            return new AssureurKpisDto
            {
                NombreAffilies = nombreAffilies,
                NombreDependants = nombreDependants,
                NombreAntecedents = nombreAntecedents,
                NombreProduitsActifs = await _db.ProduitsAssureurs.CountAsync(
                    p => p.AssureurId == assureurId && p.Statut, ct),
                NombreSouscriptionsActives = nombreSouscriptionsActives,
                NouvellesSouscriptionsMois = nouvellesSouscriptionsMois,
                MontantCollectesMois = montantCollectesMois,
                BonsEmisMois = bonsEmisMois,
                BonsUtilisesMois = bonsUtilisesMois,
                DemandesBonEnAttente = demandesEnAttente
            };
        }

        public async Task<List<AssureurAffilieDto>> GetAffiliesAsync(
            int assureurId, int limit = 50, CancellationToken ct = default)
        {
            if (limit <= 0) limit = 50;

            var affilieIds = await QueryAffilieIdsForAssureur(assureurId).ToListAsync(ct);
            if (affilieIds.Count == 0)
                return new List<AssureurAffilieDto>();

            var souscriptionsParAffilie = await QuerySouscriptionsForAssureur(assureurId)
                .Where(s => s.Statut && affilieIds.Contains(s.AffilieId))
                .GroupBy(s => s.AffilieId)
                .Select(g => new { AffilieId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.AffilieId, x => x.Count, ct);

            var dependantsParAffilie = await _db.Dependants.AsNoTracking()
                .Where(d => d.Statut && affilieIds.Contains(d.AffilieId))
                .GroupBy(d => d.AffilieId)
                .Select(g => new { AffilieId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.AffilieId, x => x.Count, ct);

            var antecedentsParAffilie = await _db.Antecedants.AsNoTracking()
                .Where(a => a.Statut && affilieIds.Contains(a.AffilieId))
                .GroupBy(a => a.AffilieId)
                .Select(g => new { AffilieId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.AffilieId, x => x.Count, ct);

            var affilies = await _db.Affilies.AsNoTracking()
                .Where(a => affilieIds.Contains(a.IdAffilie))
                .OrderByDescending(a => a.DateCreation)
                .Take(limit)
                .ToListAsync(ct);

            return affilies.Select(a => new AssureurAffilieDto
            {
                IdAffilie = a.IdAffilie,
                CodeAdhesion = a.CodeAdhesion,
                NomComplet = a.NomComplet,
                Telephone = a.Telephone,
                DateNaissance = a.DateNaissance,
                Statut = a.Statut,
                NombreDependants = dependantsParAffilie.GetValueOrDefault(a.IdAffilie),
                NombreAntecedents = antecedentsParAffilie.GetValueOrDefault(a.IdAffilie),
                NombreSouscriptionsActives = souscriptionsParAffilie.GetValueOrDefault(a.IdAffilie)
            }).ToList();
        }

        public async Task<List<AssureurDependantDto>> GetDependantsAsync(
            int assureurId, int limit = 100, CancellationToken ct = default)
        {
            if (limit <= 0) limit = 100;

            var affilieIds = QueryAffilieIdsForAssureur(assureurId);

            return await _db.Dependants.AsNoTracking()
                .Include(d => d.Affilie)
                .Where(d => d.Statut && affilieIds.Contains(d.AffilieId))
                .OrderByDescending(d => d.DateCreation)
                .Take(limit)
                .Select(d => new AssureurDependantDto
                {
                    IdDependant = d.IdDependant,
                    AffilieId = d.AffilieId,
                    AffilieNomComplet = d.Affilie.NomComplet,
                    CodeAdhesion = d.Affilie.CodeAdhesion,
                    Nom = d.Nom,
                    LienParente = d.LienParente,
                    DateNaissance = d.DateNaissance,
                    Telephone = d.Telephone,
                    Statut = d.Statut
                })
                .ToListAsync(ct);
        }

        public async Task<List<AssureurAntecedentDto>> GetAntecedentsAsync(
            int assureurId, int limit = 100, CancellationToken ct = default)
        {
            if (limit <= 0) limit = 100;

            var affilieIds = QueryAffilieIdsForAssureur(assureurId);

            return await _db.Antecedants.AsNoTracking()
                .Include(a => a.Affilie)
                .Where(a => a.Statut && affilieIds.Contains(a.AffilieId))
                .OrderByDescending(a => a.DateCreation)
                .Take(limit)
                .Select(a => new AssureurAntecedentDto
                {
                    IdAntecedant = a.IdAntecedant,
                    AffilieId = a.AffilieId,
                    AffilieNomComplet = a.Affilie.NomComplet,
                    CodeAdhesion = a.Affilie.CodeAdhesion,
                    Description = a.Description,
                    DateCreation = a.DateCreation,
                    Statut = a.Statut
                })
                .ToListAsync(ct);
        }

        public async Task<List<AssureurRepartitionProduitDto>> GetRepartitionProduitsAsync(
            int assureurId, CancellationToken ct = default)
        {
            var debutMois = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            var produits = await _db.ProduitsAssureurs.AsNoTracking()
                .Where(p => p.AssureurId == assureurId && p.Statut)
                .Select(p => new { p.IdProduit, p.Nom })
                .ToListAsync(ct);

            var result = new List<AssureurRepartitionProduitDto>();

            foreach (var produit in produits)
            {
                var prestationIds = await _db.Prestations.AsNoTracking()
                    .Where(pr => pr.ProduitAssureurId == produit.IdProduit && pr.Statut)
                    .Select(pr => pr.IdPrestation)
                    .ToListAsync(ct);

                var nombreSouscriptions = await _db.SouscriptionsPrestations.AsNoTracking()
                    .CountAsync(s => s.Statut && prestationIds.Contains(s.PrestationId), ct);

                var montant = await _db.Collectes.AsNoTracking()
                    .Where(c =>
                        c.Statut &&
                        c.DateCollecte >= debutMois &&
                        c.SouscriptionPrestationId != null &&
                        c.SouscriptionPrestationRef != null &&
                        c.SouscriptionPrestationRef.Prestation.ProduitAssureurId == produit.IdProduit)
                    .SumAsync(c => (double)(c.MontantDevisePrincipale ?? c.Montant), ct);

                result.Add(new AssureurRepartitionProduitDto
                {
                    ProduitAssureurId = produit.IdProduit,
                    NomProduit = produit.Nom,
                    NombreSouscriptions = nombreSouscriptions,
                    MontantCollecteMois = (decimal)montant
                });
            }

            return result.OrderByDescending(r => r.NombreSouscriptions).ToList();
        }

        public async Task<DashboardAssureurDto> GetDashboardSummaryAsync(int assureurId, CancellationToken ct = default)
        {
            _logger.LogInformation("Dashboard assureur {AssureurId}", assureurId);

            var nomAssureur = await _db.Assureurs.AsNoTracking()
                .Where(a => a.IdAssureur == assureurId)
                .Select(a => a.Nom)
                .FirstOrDefaultAsync(ct) ?? string.Empty;

            return new DashboardAssureurDto
            {
                NomAssureur = nomAssureur,
                Kpis = await GetKpisAsync(assureurId, ct),
                RepartitionProduits = await GetRepartitionProduitsAsync(assureurId, ct),
                AffiliesRecents = await GetAffiliesAsync(assureurId, 20, ct),
                Dependants = await GetDependantsAsync(assureurId, 50, ct),
                Antecedents = await GetAntecedentsAsync(assureurId, 50, ct),
                DerniereMiseAJour = DateTime.Now
            };
        }

        private IQueryable<int> QueryAffilieIdsForAssureur(int assureurId) =>
            QuerySouscriptionsForAssureur(assureurId)
                .Select(s => s.AffilieId)
                .Distinct();

        private IQueryable<SouscriptionPrestation> QuerySouscriptionsForAssureur(int assureurId) =>
            _db.SouscriptionsPrestations.AsNoTracking()
                .Where(s =>
                    s.Prestation.ProduitAssureurId != null &&
                    s.Prestation.ProduitAssureur!.AssureurId == assureurId);

        private IQueryable<int> QueryPrestationIdsForAssureur(int assureurId) =>
            _db.Prestations.AsNoTracking()
                .Where(p => p.ProduitAssureurId != null && p.ProduitAssureur!.AssureurId == assureurId)
                .Select(p => p.IdPrestation);

        private async Task<decimal> SumCollectesMoisAsync(int assureurId, DateTime debutMois, CancellationToken ct)
        {
            var sum = await _db.Collectes.AsNoTracking()
                .Where(c =>
                    c.Statut &&
                    c.DateCollecte >= debutMois &&
                    c.SouscriptionPrestationId != null &&
                    c.SouscriptionPrestationRef != null &&
                    c.SouscriptionPrestationRef.Prestation.ProduitAssureurId != null &&
                    c.SouscriptionPrestationRef.Prestation.ProduitAssureur!.AssureurId == assureurId)
                .SumAsync(c => (double)(c.MontantDevisePrincipale ?? c.Montant), ct);

            return (decimal)sum;
        }
    }
}
