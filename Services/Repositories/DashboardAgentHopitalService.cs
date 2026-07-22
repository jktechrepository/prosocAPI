using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prosoc.Data;
using Prosoc.Utilities;
using ProsocAPI.Models.DTOs.DashboardAgentHopital;
using ProsocAPI.Services;

namespace ProsocAPI.Services.Repositories
{
    public class DashboardAgentHopitalService : IDashboardAgentHopitalRepository
    {
        private readonly ProsocDbContext _db;
        private readonly IDeviseConversionService _deviseConversion;
        private readonly ILogger<DashboardAgentHopitalService> _logger;

        public DashboardAgentHopitalService(
            ProsocDbContext db,
            IDeviseConversionService deviseConversion,
            ILogger<DashboardAgentHopitalService> logger)
        {
            _db = db;
            _deviseConversion = deviseConversion;
            _logger = logger;
        }

        public async Task<HopitalKpisDto> GetKpisAsync(int hopitalPartenaireId, CancellationToken ct = default)
        {
            var debutMois = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var maintenant = DateTime.Now;
            var jetonsQuery = HopitalScopeHelper.QueryJetonsForHopital(_db, hopitalPartenaireId);
            var affilieIds = HopitalScopeHelper.QueryAffilieIdsForHopital(_db, hopitalPartenaireId);
            var bonsQuery = HopitalScopeHelper.QueryBonsForHopital(_db, hopitalPartenaireId);

            var jetons = await jetonsQuery
                .Select(j => new
                {
                    j.IdJeton,
                    j.DateEmission,
                    j.DateUtilisation,
                    j.EstUtilise
                })
                .ToListAsync(ct);

            var prestationsParJeton = await GetPrestationsParJetonAsync(
                jetons.Select(j => j.IdJeton).ToList(), ct);

            var bons = await bonsQuery
                .Include(b => b.Prestation)
                .Select(b => new
                {
                    b.DateEmission,
                    b.DateUtilisation,
                    b.EstUtilise,
                    b.Prestation.Montant,
                    b.Prestation.DeviseId
                })
                .ToListAsync(ct);

            var devisePrincipale = await GetDevisePrincipaleAsync(ct);

            var lignesJetonsTotal = new List<(decimal Montant, int DeviseId, DateTime DateOperation)>();
            var lignesJetonsMois = new List<(decimal Montant, int DeviseId, DateTime DateOperation)>();
            var lignesJetonsUtilisesMois = new List<(decimal Montant, int DeviseId, DateTime DateOperation)>();

            foreach (var jeton in jetons)
            {
                if (!prestationsParJeton.TryGetValue(jeton.IdJeton, out var prestation))
                    continue;

                var ligne = (prestation.Montant, prestation.DeviseId, jeton.DateEmission);
                lignesJetonsTotal.Add(ligne);

                if (jeton.DateEmission >= debutMois)
                    lignesJetonsMois.Add(ligne);

                if (jeton.EstUtilise && jeton.DateUtilisation >= debutMois)
                {
                    lignesJetonsUtilisesMois.Add((
                        prestation.Montant,
                        prestation.DeviseId,
                        jeton.DateUtilisation!.Value));
                }
            }

            var lignesBonsTotal = bons
                .Select(b => (b.Montant, b.DeviseId, b.DateEmission))
                .ToList();
            var lignesBonsMois = bons
                .Where(b => b.DateEmission >= debutMois)
                .Select(b => (b.Montant, b.DeviseId, b.DateEmission))
                .ToList();
            var lignesBonsUtilisesMois = bons
                .Where(b => b.EstUtilise && b.DateUtilisation >= debutMois)
                .Select(b => (b.Montant, b.DeviseId, b.DateUtilisation!.Value))
                .ToList();

            return new HopitalKpisDto
            {
                JetonsEmisTotal = jetons.Count,
                JetonsEmisMois = jetons.Count(j => j.DateEmission >= debutMois),
                JetonsUtilisesMois = jetons.Count(j => j.EstUtilise && j.DateUtilisation >= debutMois),
                JetonsValidesEnAttente = await jetonsQuery.CountAsync(j =>
                    j.EstValide
                    && !j.EstUtilise
                    && (!j.DateExpiration.HasValue || j.DateExpiration > maintenant), ct),
                JetonsExpires = await jetonsQuery.CountAsync(j =>
                    j.DateExpiration.HasValue && j.DateExpiration <= maintenant && !j.EstUtilise, ct),
                BonsLiesTotal = bons.Count,
                BonsUtilisesMois = bons.Count(b => b.EstUtilise && b.DateUtilisation >= debutMois),
                PatientsUniques = await affilieIds.CountAsync(ct),
                TotalDependants = await _db.Dependants.AsNoTracking()
                    .CountAsync(d => d.Statut && affilieIds.Contains(d.AffilieId), ct),
                TotalAntecedents = await _db.Antecedants.AsNoTracking()
                    .CountAsync(a => a.Statut && affilieIds.Contains(a.AffilieId), ct),
                ValeurPrestationsJetonsTotal = await SommerPrestationsAsync(lignesJetonsTotal, devisePrincipale.Id, ct),
                ValeurPrestationsJetonsMois = await SommerPrestationsAsync(lignesJetonsMois, devisePrincipale.Id, ct),
                ValeurPrestationsJetonsUtilisesMois = await SommerPrestationsAsync(lignesJetonsUtilisesMois, devisePrincipale.Id, ct),
                ValeurPrestationsBonsTotal = await SommerPrestationsAsync(lignesBonsTotal, devisePrincipale.Id, ct),
                ValeurPrestationsBonsMois = await SommerPrestationsAsync(lignesBonsMois, devisePrincipale.Id, ct),
                ValeurPrestationsBonsUtilisesMois = await SommerPrestationsAsync(lignesBonsUtilisesMois, devisePrincipale.Id, ct),
                DevisePrincipaleCode = devisePrincipale.Code
            };
        }

        public async Task<List<HopitalJetonEnAttenteDto>> GetJetonsEnAttenteAsync(
            int hopitalPartenaireId, int limit = 50, CancellationToken ct = default)
        {
            if (limit <= 0) limit = 50;

            var maintenant = DateTime.Now;
            var devisePrincipale = await GetDevisePrincipaleAsync(ct);

            var jetons = await HopitalScopeHelper.QueryJetonsForHopital(_db, hopitalPartenaireId)
                .Include(j => j.Affilie)
                .Where(j =>
                    j.EstValide
                    && !j.EstUtilise
                    && (!j.DateExpiration.HasValue || j.DateExpiration > maintenant))
                .OrderByDescending(j => j.DateEmission)
                .Take(limit)
                .Select(j => new HopitalJetonEnAttenteDto
                {
                    IdJeton = j.IdJeton,
                    CodeJeton = j.CodeJeton,
                    AffilieId = j.AffilieId,
                    AffilieNomComplet = j.Affilie.NomComplet,
                    CodeAdhesion = j.Affilie.CodeAdhesion,
                    DateEmission = j.DateEmission,
                    DateExpiration = j.DateExpiration
                })
                .ToListAsync(ct);

            if (jetons.Count == 0)
                return jetons;

            var prestationsParJeton = await GetPrestationsParJetonAsync(
                jetons.Select(j => j.IdJeton).ToList(), ct);

            foreach (var jeton in jetons)
            {
                if (!prestationsParJeton.TryGetValue(jeton.IdJeton, out var prestation))
                    continue;

                jeton.NomPrestation = prestation.Nom;
                jeton.MontantPrestation = await MontantPrestationEnDevisePrincipaleAsync(
                    prestation.Montant,
                    prestation.DeviseId,
                    jeton.DateEmission,
                    devisePrincipale.Id,
                    ct);
            }

            return jetons;
        }

        public async Task<List<HopitalBonRecentDto>> GetBonsRecentsAsync(
            int hopitalPartenaireId, int limit = 50, CancellationToken ct = default)
        {
            if (limit <= 0) limit = 50;

            var devisePrincipale = await GetDevisePrincipaleAsync(ct);

            var bons = await HopitalScopeHelper.QueryBonsForHopital(_db, hopitalPartenaireId)
                .Include(b => b.Affilie)
                .Include(b => b.Prestation)
                .OrderByDescending(b => b.DateEmission)
                .Take(limit)
                .Select(b => new
                {
                    b.IdBonEnvoi,
                    b.NumeroBon,
                    b.AffilieId,
                    AffilieNomComplet = b.Affilie.NomComplet,
                    CodeAdhesion = b.Affilie.CodeAdhesion,
                    NomPrestation = b.Prestation.NomPrestation,
                    b.Prestation.Montant,
                    b.Prestation.DeviseId,
                    b.DateEmission,
                    b.EstUtilise,
                    b.DateUtilisation
                })
                .ToListAsync(ct);

            var result = new List<HopitalBonRecentDto>();
            foreach (var bon in bons)
            {
                result.Add(new HopitalBonRecentDto
                {
                    IdBonEnvoi = bon.IdBonEnvoi,
                    NumeroBon = bon.NumeroBon,
                    AffilieId = bon.AffilieId,
                    AffilieNomComplet = bon.AffilieNomComplet,
                    CodeAdhesion = bon.CodeAdhesion,
                    NomPrestation = bon.NomPrestation,
                    MontantPrestation = await MontantPrestationEnDevisePrincipaleAsync(
                        bon.Montant,
                        bon.DeviseId,
                        bon.DateEmission,
                        devisePrincipale.Id,
                        ct),
                    DateEmission = bon.DateEmission,
                    EstUtilise = bon.EstUtilise,
                    DateUtilisation = bon.DateUtilisation
                });
            }

            return result;
        }

        public async Task<List<HopitalPatientDto>> GetPatientsAsync(
            int hopitalPartenaireId, int limit = 50, CancellationToken ct = default)
        {
            if (limit <= 0) limit = 50;

            var affilieIds = await HopitalScopeHelper.QueryAffilieIdsForHopital(_db, hopitalPartenaireId)
                .ToListAsync(ct);

            if (affilieIds.Count == 0)
                return new List<HopitalPatientDto>();

            var jetonsParAffilie = await HopitalScopeHelper.QueryJetonsForHopital(_db, hopitalPartenaireId)
                .GroupBy(j => j.AffilieId)
                .Select(g => new
                {
                    AffilieId = g.Key,
                    Count = g.Count(),
                    DernierJeton = g.Max(j => j.DateEmission)
                })
                .ToDictionaryAsync(x => x.AffilieId, ct);

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

            return affilies.Select(a =>
            {
                jetonsParAffilie.TryGetValue(a.IdAffilie, out var jetonInfo);
                return new HopitalPatientDto
                {
                    IdAffilie = a.IdAffilie,
                    CodeAdhesion = a.CodeAdhesion,
                    NomComplet = a.NomComplet,
                    Telephone = a.Telephone,
                    DateNaissance = a.DateNaissance,
                    NombreDependants = dependantsParAffilie.GetValueOrDefault(a.IdAffilie),
                    NombreAntecedents = antecedentsParAffilie.GetValueOrDefault(a.IdAffilie),
                    NombreJetons = jetonInfo?.Count ?? 0,
                    DernierJetonEmission = jetonInfo?.DernierJeton
                };
            }).ToList();
        }

        public async Task<List<HopitalDependantDto>> GetDependantsAsync(
            int hopitalPartenaireId, int limit = 100, CancellationToken ct = default)
        {
            if (limit <= 0) limit = 100;

            var affilieIds = HopitalScopeHelper.QueryAffilieIdsForHopital(_db, hopitalPartenaireId);

            return await _db.Dependants.AsNoTracking()
                .Include(d => d.Affilie)
                .Where(d => d.Statut && affilieIds.Contains(d.AffilieId))
                .OrderByDescending(d => d.DateCreation)
                .Take(limit)
                .Select(d => new HopitalDependantDto
                {
                    IdDependant = d.IdDependant,
                    AffilieId = d.AffilieId,
                    AffilieNomComplet = d.Affilie.NomComplet,
                    CodeAdhesion = d.Affilie.CodeAdhesion,
                    Nom = d.Nom,
                    LienParente = d.LienParente,
                    DateNaissance = d.DateNaissance,
                    Telephone = d.Telephone
                })
                .ToListAsync(ct);
        }

        public async Task<List<HopitalAntecedentDto>> GetAntecedentsAsync(
            int hopitalPartenaireId, int limit = 100, CancellationToken ct = default)
        {
            if (limit <= 0) limit = 100;

            var affilieIds = HopitalScopeHelper.QueryAffilieIdsForHopital(_db, hopitalPartenaireId);

            return await _db.Antecedants.AsNoTracking()
                .Include(a => a.Affilie)
                .Where(a => a.Statut && affilieIds.Contains(a.AffilieId))
                .OrderByDescending(a => a.DateCreation)
                .Take(limit)
                .Select(a => new HopitalAntecedentDto
                {
                    IdAntecedant = a.IdAntecedant,
                    AffilieId = a.AffilieId,
                    AffilieNomComplet = a.Affilie.NomComplet,
                    CodeAdhesion = a.Affilie.CodeAdhesion,
                    Description = a.Description,
                    DateCreation = a.DateCreation
                })
                .ToListAsync(ct);
        }

        public async Task<List<HopitalRepartitionPrestationDto>> GetRepartitionPrestationsAsync(
            int hopitalPartenaireId, CancellationToken ct = default)
        {
            var devisePrincipale = await GetDevisePrincipaleAsync(ct);

            var jetons = await HopitalScopeHelper.QueryJetonsForHopital(_db, hopitalPartenaireId)
                .Select(j => new { j.IdJeton, j.DateEmission })
                .ToListAsync(ct);

            var prestationsParJeton = await GetPrestationsParJetonAsync(
                jetons.Select(j => j.IdJeton).ToList(), ct);

            var montantsJetonsParPrestation = new Dictionary<int, List<(decimal Montant, int DeviseId, DateTime DateOperation)>>();
            var jetonsParPrestation = new Dictionary<int, int>();

            foreach (var jeton in jetons)
            {
                if (!prestationsParJeton.TryGetValue(jeton.IdJeton, out var prestation))
                    continue;

                jetonsParPrestation[prestation.PrestationId] = jetonsParPrestation.GetValueOrDefault(prestation.PrestationId) + 1;

                if (!montantsJetonsParPrestation.TryGetValue(prestation.PrestationId, out var lignes))
                {
                    lignes = new List<(decimal, int, DateTime)>();
                    montantsJetonsParPrestation[prestation.PrestationId] = lignes;
                }

                lignes.Add((prestation.Montant, prestation.DeviseId, jeton.DateEmission));
            }

            var bons = await HopitalScopeHelper.QueryBonsForHopital(_db, hopitalPartenaireId)
                .Include(b => b.Prestation)
                .Select(b => new
                {
                    b.PrestationId,
                    b.Prestation.NomPrestation,
                    b.Prestation.Montant,
                    b.Prestation.DeviseId,
                    b.DateEmission
                })
                .ToListAsync(ct);

            var bonsParPrestation = bons
                .GroupBy(b => b.PrestationId)
                .ToDictionary(g => g.Key, g => g.Count());

            var montantsBonsParPrestation = bons
                .GroupBy(b => b.PrestationId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(b => (b.Montant, b.DeviseId, b.DateEmission)).ToList());

            var prestationIds = jetonsParPrestation.Keys
                .Union(bonsParPrestation.Keys)
                .ToList();

            if (prestationIds.Count == 0)
                return new List<HopitalRepartitionPrestationDto>();

            var nomsPrestations = await _db.Prestations.AsNoTracking()
                .Where(p => prestationIds.Contains(p.IdPrestation))
                .ToDictionaryAsync(p => p.IdPrestation, p => p.NomPrestation, ct);

            var repartition = new List<HopitalRepartitionPrestationDto>();
            foreach (var prestationId in prestationIds)
            {
                montantsJetonsParPrestation.TryGetValue(prestationId, out var lignesJetons);
                montantsBonsParPrestation.TryGetValue(prestationId, out var lignesBons);

                repartition.Add(new HopitalRepartitionPrestationDto
                {
                    PrestationId = prestationId,
                    NomPrestation = nomsPrestations.GetValueOrDefault(prestationId) ?? string.Empty,
                    NombreJetons = jetonsParPrestation.GetValueOrDefault(prestationId),
                    NombreBons = bonsParPrestation.GetValueOrDefault(prestationId),
                    MontantTotalJetons = await SommerPrestationsAsync(lignesJetons ?? new List<(decimal, int, DateTime)>(), devisePrincipale.Id, ct),
                    MontantTotalBons = await SommerPrestationsAsync(lignesBons ?? new List<(decimal, int, DateTime)>(), devisePrincipale.Id, ct)
                });
            }

            return repartition
                .OrderByDescending(r => r.NombreJetons + r.NombreBons)
                .ToList();
        }

        public async Task<DashboardAgentHopitalDto> GetDashboardSummaryAsync(
            int hopitalPartenaireId, CancellationToken ct = default)
        {
            _logger.LogInformation("Dashboard agent hôpital {HopitalId}", hopitalPartenaireId);

            var nomHopital = await _db.HopitalPartenaires.AsNoTracking()
                .Where(h => h.IdHopital == hopitalPartenaireId)
                .Select(h => h.Nom)
                .FirstOrDefaultAsync(ct) ?? string.Empty;

            var kpis = await GetKpisAsync(hopitalPartenaireId, ct);

            return new DashboardAgentHopitalDto
            {
                NomHopital = nomHopital,
                Kpis = kpis,
                RepartitionPrestations = await GetRepartitionPrestationsAsync(hopitalPartenaireId, ct),
                JetonsEnAttente = await GetJetonsEnAttenteAsync(hopitalPartenaireId, 20, ct),
                BonsRecents = await GetBonsRecentsAsync(hopitalPartenaireId, 20, ct),
                Patients = await GetPatientsAsync(hopitalPartenaireId, 20, ct),
                Dependants = await GetDependantsAsync(hopitalPartenaireId, 20, ct),
                Antecedents = await GetAntecedentsAsync(hopitalPartenaireId, 20, ct),
                DerniereMiseAJour = DateTime.Now,
                DevisePrincipaleCode = kpis.DevisePrincipaleCode
            };
        }

        private async Task<(int? Id, string? Code)> GetDevisePrincipaleAsync(CancellationToken ct) =>
            await _db.Devises
                .AsNoTracking()
                .Where(d => d.EstDevisePrincipale && d.Statut)
                .Select(d => new ValueTuple<int?, string?>((int?)d.IdDevise, d.Code))
                .FirstOrDefaultAsync(ct);

        private async Task<Dictionary<int, (int PrestationId, decimal Montant, int DeviseId, string Nom)>> GetPrestationsParJetonAsync(
            ICollection<int> jetonIds,
            CancellationToken ct)
        {
            if (jetonIds.Count == 0)
                return new Dictionary<int, (int, decimal, int, string)>();

            var demandes = await _db.DemandesBonEnvoi.AsNoTracking()
                .Where(d => d.Statut && d.JetonMedicalId != null && jetonIds.Contains(d.JetonMedicalId.Value))
                .Select(d => new { JetonId = d.JetonMedicalId!.Value, d.PrestationId })
                .ToListAsync(ct);

            if (demandes.Count == 0)
                return new Dictionary<int, (int, decimal, int, string)>();

            var prestationIds = demandes.Select(d => d.PrestationId).Distinct().ToList();
            var prestations = await _db.Prestations.AsNoTracking()
                .Where(p => prestationIds.Contains(p.IdPrestation))
                .ToDictionaryAsync(
                    p => p.IdPrestation,
                    p => (p.Montant, p.DeviseId, p.NomPrestation),
                    ct);

            return demandes
                .GroupBy(d => d.JetonId)
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        var prestationId = g.First().PrestationId;
                        var p = prestations[prestationId];
                        return (prestationId, p.Montant, p.DeviseId, p.NomPrestation);
                    });
        }

        private async Task<decimal> MontantPrestationEnDevisePrincipaleAsync(
            decimal montant,
            int deviseId,
            DateTime dateOperation,
            int? devisePrincipaleId,
            CancellationToken ct)
        {
            if (!devisePrincipaleId.HasValue)
                return montant;

            return await DashboardDeviseConsolidation.MontantMouvementEnDevisePrincipaleAsync(
                _deviseConversion,
                montant,
                deviseId,
                devisePrincipaleId,
                dateOperation,
                ct);
        }

        private async Task<decimal> SommerPrestationsAsync(
            IEnumerable<(decimal Montant, int DeviseId, DateTime DateOperation)> lignes,
            int? devisePrincipaleId,
            CancellationToken ct)
        {
            var liste = lignes.ToList();
            if (liste.Count == 0)
                return 0;

            if (!devisePrincipaleId.HasValue)
                return liste.Sum(l => l.Montant);

            return await DashboardDeviseConsolidation.SommerMouvementsEnDevisePrincipaleAsync(
                _deviseConversion,
                liste.Select(l => (l.Montant, l.DeviseId, l.DateOperation)),
                devisePrincipaleId.Value,
                ct);
        }
    }
}
