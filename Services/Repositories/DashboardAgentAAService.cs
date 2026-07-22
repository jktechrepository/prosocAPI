using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.DashboardAgentAa;
using ProsocAPI.Services;

namespace ProsocAPI.Services.Repositories
{
    public class DashboardAgentAAService : IDashboardAgentAARepository
    {
        private readonly ProsocDbContext _db;
        private readonly IDeviseConversionService _deviseConversion;
        private readonly ILogger<DashboardAgentAAService> _logger;

        public DashboardAgentAAService(
            ProsocDbContext db,
            IDeviseConversionService deviseConversion,
            ILogger<DashboardAgentAAService> logger)
        {
            _db = db;
            _deviseConversion = deviseConversion;
            _logger = logger;
        }

        public async Task<AgentAaKpisDto> GetKpisAsync(int agentId, CancellationToken ct = default)
        {
            var maintenant = DateTime.Now;
            var debutMois = new DateTime(maintenant.Year, maintenant.Month, 1);
            var affilieIds = await QueryAffilieIdsForAgent(agentId).ToListAsync(ct);

            var devisePrincipale = await _db.Devises
                .AsNoTracking()
                .Where(d => d.EstDevisePrincipale && d.Statut)
                .Select(d => new { d.IdDevise, d.Code })
                .FirstOrDefaultAsync(ct);

            var collectesMois = affilieIds.Count == 0
                ? new List<Collecte>()
                : await _db.Collectes
                    .AsNoTracking()
                    .Where(c => affilieIds.Contains(c.AffilieId)
                        && c.DateCollecte >= debutMois
                        && c.DateCollecte <= maintenant
                        && c.Statut)
                    .ToListAsync(ct);

            var totalCollectesMois = DashboardDeviseConsolidation.SommerCollectesEnDevisePrincipale(collectesMois);

            var mouvementsCommissionsMois = await _db.WalletMouvements
                .AsNoTracking()
                .Where(m => m.Wallet.AgentId == agentId
                    && m.Source == "COMM_COLLECTE"
                    && m.DateOperation >= debutMois
                    && m.DateOperation <= maintenant)
                .Select(m => new { m.Montant, m.DeviseId, m.DateOperation })
                .ToListAsync(ct);

            var totalCommissionsMois = devisePrincipale != null
                ? await DashboardDeviseConsolidation.SommerMouvementsEnDevisePrincipaleAsync(
                    _deviseConversion,
                    mouvementsCommissionsMois.Select(m => (m.Montant, m.DeviseId, m.DateOperation)),
                    devisePrincipale.IdDevise,
                    ct)
                : mouvementsCommissionsMois.Sum(m => m.Montant);

            var adhesions = await QueryAdhesionsForAgent(agentId).ToListAsync(ct);
            var totalDossiers = adhesions.Count;
            var dossiersValides = adhesions.Count(a => EstDossierValide(a.StatutDossier));
            var dossiersEnAttente = totalDossiers - dossiersValides;
            var dossiersValidesMois = adhesions.Count(a =>
                EstDossierValide(a.StatutDossier)
                && a.DateModification >= debutMois);

            var tauxCompletion = totalDossiers > 0
                ? Math.Round((decimal)dossiersValides / totalDossiers * 100, 2)
                : 0;

            var totalDependants = await _db.Dependants.AsNoTracking()
                .CountAsync(d => d.Statut && affilieIds.Contains(d.AffilieId), ct);

            var dependantsAjoutesMois = await _db.Dependants.AsNoTracking()
                .CountAsync(d =>
                    d.Statut
                    && d.DateCreation >= debutMois
                    && affilieIds.Contains(d.AffilieId), ct);

            var totalAntecedents = await _db.Antecedants.AsNoTracking()
                .CountAsync(a => a.Statut && affilieIds.Contains(a.AffilieId), ct);

            var antecedentsAjoutesMois = await _db.Antecedants.AsNoTracking()
                .CountAsync(a =>
                    a.Statut
                    && a.DateCreation >= debutMois
                    && affilieIds.Contains(a.AffilieId), ct);

            var demandesBonEnAttente = await _db.DemandesBonEnvoi.AsNoTracking()
                .CountAsync(d =>
                    d.Statut
                    && d.StatutDemande == "EN_ATTENTE"
                    && affilieIds.Contains(d.AffilieId), ct);

            return new AgentAaKpisDto
            {
                TotalDossiers = totalDossiers,
                DossiersEnAttente = dossiersEnAttente,
                DossiersValides = dossiersValides,
                DossiersValidesMois = dossiersValidesMois,
                TauxCompletion = tauxCompletion,
                TotalDependants = totalDependants,
                DependantsAjoutesMois = dependantsAjoutesMois,
                TotalAntecedents = totalAntecedents,
                AntecedentsAjoutesMois = antecedentsAjoutesMois,
                DemandesBonEnAttente = demandesBonEnAttente,
                TotalCollectesMois = totalCollectesMois,
                TotalCommissionsMois = totalCommissionsMois,
                DevisePrincipaleCode = devisePrincipale?.Code
            };
        }

        public async Task<List<AgentAaDossierDto>> GetDossiersATraiterAsync(
            int agentId, int limit = 50, CancellationToken ct = default)
        {
            if (limit <= 0) limit = 50;

            var adhesions = await QueryAdhesionsForAgent(agentId)
                .Include(a => a.Affilie)
                .Include(a => a.TypeAdhesion)
                .OrderByDescending(a => a.DateCreation)
                .Take(limit * 3)
                .ToListAsync(ct);

            var dossiersEnAttente = adhesions
                .Where(a => !EstDossierValide(a.StatutDossier))
                .Take(limit)
                .ToList();

            if (dossiersEnAttente.Count == 0)
                return new List<AgentAaDossierDto>();

            var affilieIds = dossiersEnAttente.Select(a => a.AffilieId).Distinct().ToList();

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

            return dossiersEnAttente.Select(a => new AgentAaDossierDto
            {
                IdAdhesion = a.IdAdhesion,
                IdAffilie = a.AffilieId,
                CodeAdhesion = a.Affilie.CodeAdhesion,
                NomComplet = a.Affilie.NomComplet,
                Telephone = a.Affilie.Telephone,
                StatutDossier = a.StatutDossier,
                TypeAdhesion = a.TypeAdhesion?.Libelle ?? string.Empty,
                DateAdhesion = a.DateCreation,
                DateModification = a.DateModification,
                NombreDependants = dependantsParAffilie.GetValueOrDefault(a.AffilieId),
                NombreAntecedents = antecedentsParAffilie.GetValueOrDefault(a.AffilieId),
                EstValide = false
            }).ToList();
        }

        public async Task<List<AgentAaDependantRecentDto>> GetDependantsRecentsAsync(
            int agentId, int limit = 50, CancellationToken ct = default)
        {
            if (limit <= 0) limit = 50;

            var affilieIds = QueryAffilieIdsForAgent(agentId);

            return await _db.Dependants.AsNoTracking()
                .Include(d => d.Affilie)
                .Where(d => d.Statut && affilieIds.Contains(d.AffilieId))
                .OrderByDescending(d => d.DateCreation)
                .Take(limit)
                .Select(d => new AgentAaDependantRecentDto
                {
                    IdDependant = d.IdDependant,
                    AffilieId = d.AffilieId,
                    AffilieNomComplet = d.Affilie.NomComplet,
                    CodeAdhesion = d.Affilie.CodeAdhesion,
                    Nom = d.Nom,
                    LienParente = d.LienParente,
                    DateNaissance = d.DateNaissance,
                    DateCreation = d.DateCreation
                })
                .ToListAsync(ct);
        }

        public async Task<List<AgentAaAntecedentRecentDto>> GetAntecedentsRecentsAsync(
            int agentId, int limit = 50, CancellationToken ct = default)
        {
            if (limit <= 0) limit = 50;

            var affilieIds = QueryAffilieIdsForAgent(agentId);

            return await _db.Antecedants.AsNoTracking()
                .Include(a => a.Affilie)
                .Where(a => a.Statut && affilieIds.Contains(a.AffilieId))
                .OrderByDescending(a => a.DateCreation)
                .Take(limit)
                .Select(a => new AgentAaAntecedentRecentDto
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

        public async Task<List<AgentAaRepartitionStatutDto>> GetRepartitionStatutsAsync(
            int agentId, CancellationToken ct = default)
        {
            return await QueryAdhesionsForAgent(agentId)
                .GroupBy(a => a.StatutDossier)
                .Select(g => new AgentAaRepartitionStatutDto
                {
                    StatutDossier = g.Key,
                    Nombre = g.Count()
                })
                .OrderByDescending(r => r.Nombre)
                .ToListAsync(ct);
        }

        public async Task<DashboardAgentAaDto> GetDashboardSummaryAsync(int agentId, CancellationToken ct = default)
        {
            _logger.LogInformation("Dashboard agent AA {AgentId}", agentId);

            var nomAgent = await _db.Agents.AsNoTracking()
                .Where(a => a.IdAgent == agentId)
                .Select(a => a.NomComplet)
                .FirstOrDefaultAsync(ct) ?? string.Empty;

            var kpis = await GetKpisAsync(agentId, ct);

            return new DashboardAgentAaDto
            {
                AgentId = agentId,
                NomAgent = nomAgent,
                Kpis = kpis,
                RepartitionStatuts = await GetRepartitionStatutsAsync(agentId, ct),
                DossiersATraiter = await GetDossiersATraiterAsync(agentId, 20, ct),
                DependantsRecents = await GetDependantsRecentsAsync(agentId, 20, ct),
                AntecedentsRecents = await GetAntecedentsRecentsAsync(agentId, 20, ct),
                DerniereMiseAJour = DateTime.Now,
                DevisePrincipaleCode = kpis.DevisePrincipaleCode
            };
        }

        private IQueryable<int> QueryAffilieIdsForAgent(int agentId) =>
            QueryAdhesionsForAgent(agentId).Select(a => a.AffilieId).Distinct();

        private IQueryable<Adhesion> QueryAdhesionsForAgent(int agentId) =>
            _db.Adhesions.AsNoTracking().Where(a => a.AgentId == agentId && a.Statut);

        private static bool EstDossierValide(string? statutDossier)
        {
            if (string.IsNullOrWhiteSpace(statutDossier))
                return false;

            return string.Equals(statutDossier, AdhesionNiveau2Regles.StatutValide, StringComparison.OrdinalIgnoreCase)
                || string.Equals(statutDossier, "VALIDE", StringComparison.OrdinalIgnoreCase);
        }
    }
}
