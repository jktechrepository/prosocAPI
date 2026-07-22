using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.DashboardAdmin;
using ProsocAPI.Services.Repositories;

namespace ProsocAPI.Services
{
    public class DashboardAdminService : IDashboardAdminRepository
    {
        private readonly ProsocDbContext _db;
        private readonly IDeviseConversionService _deviseConversion;
        private readonly ILogger<DashboardAdminService> _logger;

        public DashboardAdminService(
            ProsocDbContext db,
            IDeviseConversionService deviseConversion,
            ILogger<DashboardAdminService> logger)
        {
            _db = db;
            _deviseConversion = deviseConversion;
            _logger = logger;
        }

        public async Task<DashboardAdminKpisDto> GetKpisAsync(CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Calcul des KPIs du dashboard admin");

                var maintenant = DateTime.Now;
                var debutMois = new DateTime(maintenant.Year, maintenant.Month, 1);
                var debutMoisPrecedent = debutMois.AddMonths(-1);
                var finMoisPrecedent = debutMois.AddDays(-1);

                // KPIs principaux
                var totalAffilies = await _db.Affilies.CountAsync(a => a.Statut, ct);
                var totalAgents = await _db.Agents.CountAsync(a => a.Statut, ct);
                var agentsInactifs = await _db.Agents.CountAsync(a => !a.Statut, ct);
                var affiliesInactifs = await _db.Affilies.CountAsync(a => !a.Statut, ct);

                // Collectes du mois en cours (tous agents, montant en devise principale)
                var collectesMois = await _db.Collectes
                    .AsNoTracking()
                    .Where(c => c.DateCollecte >= debutMois && c.Statut)
                    .ToListAsync(ct);

                var devisePrincipale = await _db.Devises
                    .AsNoTracking()
                    .Where(d => d.EstDevisePrincipale && d.Statut)
                    .Select(d => new { d.IdDevise, d.Code })
                    .FirstOrDefaultAsync(ct);

                var totalCollectesMois = collectesMois.Sum(CollecteStatutPaiementRegles.MontantEnDevisePrincipale);
                var nombreCollectesMois = collectesMois.Count;

                var mouvementsCommissionsMois = await _db.WalletMouvements
                    .AsNoTracking()
                    .Where(m => m.Source == "COMM_COLLECTE"
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

                // Collectes du mois précédent pour calculer la progression
                var collectesMoisPrecedent = await _db.Collectes
                    .AsNoTracking()
                    .Where(c => c.DateCollecte >= debutMoisPrecedent && c.DateCollecte < debutMois && c.Statut)
                    .ToListAsync(ct);

                var totalCollectesMoisPrecedent = collectesMoisPrecedent.Sum(CollecteStatutPaiementRegles.MontantEnDevisePrincipale);
                var progressionCollectesMois = CollecteStatutPaiementRegles.CalculerProgressionCollectesMois(
                    totalCollectesMois,
                    totalCollectesMoisPrecedent);

                // Nouvelles adhésions aujourd'hui (actives uniquement)
                var debutAujourdhui = maintenant.Date;
                var nouvellesAdhesionsAujourdhui = await _db.Adhesions
                    .CountAsync(a => a.Statut && a.DateCreation.Date == debutAujourdhui, ct);

                // Collectes en attente de paiement FlexPay (EN_ATTENTE uniquement)
                var statutsPaiementActifs = await _db.Collectes
                    .AsNoTracking()
                    .Where(c => c.Statut)
                    .Select(c => c.StatutPaiement)
                    .ToListAsync(ct);
                var collectesEnAttente = statutsPaiementActifs.Count(
                    s => CollecteStatutPaiementRegles.EstEnAttente(s));

                // Dernière collecte
                var derniereCollecte = await _db.Collectes
                    .Where(c => c.Statut)
                    .OrderByDescending(c => c.DateCollecte)
                    .Select(c => (DateTime?)c.DateCollecte)
                    .FirstOrDefaultAsync(ct);

                var kpis = new DashboardAdminKpisDto
                {
                    TotalAffilies = totalAffilies,
                    TotalAgents = totalAgents,
                    TotalCollectesMois = Round2(totalCollectesMois),
                    DevisePrincipaleCode = devisePrincipale?.Code,
                    TotalCommissionsMois = Round2(totalCommissionsMois),
                    NouvellesAdhesionsAujourdhui = nouvellesAdhesionsAujourdhui,
                    CollectesEnAttente = collectesEnAttente,
                    NombreCollectesMois = nombreCollectesMois,
                    ProgressionCollectesMois = Round2(progressionCollectesMois),
                    AgentsInactifs = agentsInactifs,
                    DerniereCollecte = derniereCollecte,
                    AffiliesInactifs = affiliesInactifs
                };

                _logger.LogInformation("KPIs calculés avec succès: {TotalAffilies} affilies, {TotalAgents} agents, {TotalCollectesMois} collectes ce mois", 
                    kpis.TotalAffilies, kpis.TotalAgents, kpis.TotalCollectesMois);

                return kpis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du calcul des KPIs du dashboard admin");
                throw;
            }
        }

        public async Task<DashboardAdminGraphsDto> GetGraphsAsync(int mois = 12, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Récupération des graphiques pour {Mois} mois", mois);

                var graphs = new DashboardAdminGraphsDto
                {
                    CollectesMensuelles = await GetCollectesMensuellesAsync(mois, ct),
                    AdhesionsMensuelles = await GetAdhesionsMensuellesAsync(mois, ct),
                    TopAgents = await GetTopAgentsAsync(10, ct),
                    RepartitionAdhesions = await GetRepartitionAdhesionsAsync(ct)
                };

                _logger.LogInformation("Graphiques récupérés avec succès");
                return graphs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des graphiques");
                throw;
            }
        }

        public async Task<List<PerformanceAgentsDto>> GetTopAgentsAsync(int limit = 10, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Récupération des {Limit} meilleurs agents", limit);

                var agents = await _db.Agents
                    .AsNoTracking()
                    .Where(a => a.Statut)
                    .Select(a => new { a.IdAgent, NomAgent = a.NomComplet ?? "" })
                    .ToListAsync(ct);

                var collectes = await _db.Collectes
                    .AsNoTracking()
                    .Where(c => c.Statut && c.AgentId != null)
                    .ToListAsync(ct);

                var collectesParAgent = collectes
                    .GroupBy(c => c.AgentId!.Value)
                    .ToDictionary(
                        g => g.Key,
                        g => new
                        {
                            TotalCollectes = DashboardDeviseConsolidation.SommerCollectesEnDevisePrincipale(g),
                            NombreCollectes = g.Count()
                        });

                var adhesionCounts = await _db.Adhesions
                    .AsNoTracking()
                    .Where(ad => ad.Statut && ad.AgentId != null)
                    .GroupBy(ad => ad.AgentId!.Value)
                    .Select(g => new { AgentId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.AgentId, x => x.Count, ct);

                var topAgents = agents
                    .Select(a =>
                    {
                        collectesParAgent.TryGetValue(a.IdAgent, out var stats);
                        var totalCollectes = stats?.TotalCollectes ?? 0;
                        var nombreCollectes = stats?.NombreCollectes ?? 0;
                        return new PerformanceAgentsDto
                        {
                            AgentId = a.IdAgent,
                            NomAgent = a.NomAgent,
                            TotalAffilies = adhesionCounts.GetValueOrDefault(a.IdAgent),
                            TotalCollectes = Round2(totalCollectes),
                            NombreCollectes = nombreCollectes,
                            MontantMoyenCollecte = nombreCollectes > 0 ? Round2(totalCollectes / nombreCollectes) : 0,
                            ScorePerformance = 0
                        };
                    })
                    .OrderByDescending(a => a.TotalCollectes)
                    .Take(limit)
                    .ToList();

                // Calcul des scores de performance
                foreach (var agent in topAgents)
                {
                    var scoreAffilies = Math.Min((agent.TotalAffilies * 10), 40);
                    var scoreMontant = Math.Min((agent.TotalCollectes / 100000) * 30, 30);
                    var scoreMoyen = Math.Min((agent.MontantMoyenCollecte / 10000) * 20, 20);
                    var scoreFrequence = Math.Min((agent.NombreCollectes / 10) * 10, 10);

                    agent.ScorePerformance = Round2(scoreAffilies + scoreMontant + scoreMoyen + scoreFrequence);
                }

                _logger.LogInformation("Top agents récupérés: {NbAgents} agents", topAgents.Count);
                return topAgents;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des top agents");
                throw;
            }
        }

        public async Task<bool> ValidateCollecteAsync(int collecteId, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Validation de la collecte {CollecteId}", collecteId);

                var collecte = await _db.Collectes.FirstOrDefaultAsync(c => c.IdCollecte == collecteId, ct);
                if (collecte == null)
                {
                    _logger.LogWarning("Collecte {CollecteId} non trouvée", collecteId);
                    return false;
                }

                if (CollecteStatutPaiementRegles.EstValide(collecte.StatutPaiement))
                {
                    _logger.LogInformation("Collecte {CollecteId} déjà validée par l'admin", collecteId);
                    return true;
                }

                collecte.StatutPaiement = CollecteStatutPaiement.Valide;
                collecte.DateModification = DateTime.Now;

                await _db.SaveChangesAsync(ct);

                _logger.LogInformation("Collecte {CollecteId} validée avec succès", collecteId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la validation de la collecte {CollecteId}", collecteId);
                throw;
            }
        }

        public async Task<bool> ToggleAgentStatusAsync(int agentId, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Changement de statut pour l'agent {AgentId}", agentId);

                var agent = await _db.Agents.FirstOrDefaultAsync(a => a.IdAgent == agentId, ct);
                if (agent == null)
                {
                    _logger.LogWarning("Agent {AgentId} non trouvé", agentId);
                    return false;
                }

                agent.Statut = !agent.Statut;
                agent.DateModification = DateTime.Now;

                await _db.SaveChangesAsync(ct);

                _logger.LogInformation("Statut de l'agent {AgentId} changé en {Statut}", agentId, agent.Statut);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du changement de statut pour l'agent {AgentId}", agentId);
                throw;
            }
        }

        public async Task<List<CollecteEnAttenteDto>> GetCollectesEnAttenteAsync(CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Récupération des collectes en attente");

                // Phase 1: Requête SQL-translatable sans calculs complexes
                var collectesData = await _db.Collectes
                    .Where(c => c.Statut)
                    .Include(c => c.Affilie)
                    .Include(c => c.Agent)
                    .OrderByDescending(c => c.DateCollecte)
                    .Select(c => new
                    {
                        IdCollecte = c.IdCollecte,
                        NomAffilie = c.Affilie != null ? c.Affilie.Nom : "Inconnu",
                        PrenomAffilie = c.Affilie != null ? c.Affilie.Prenom : "",
                        NomAgent = c.Agent != null ? c.Agent.NomComplet : "Agent non assigné",
                        Montant = c.MontantDevisePrincipale ?? c.Montant,
                        ReferencePaiement = c.ReferencePaiement ?? "",
                        ModePaiement = c.ModePaiement ?? "",
                        DateCollecte = c.DateCollecte,
                        StatutPaiement = c.StatutPaiement ?? ""
                    })
                    .ToListAsync(ct);

                collectesData = collectesData
                    .Where(c => CollecteStatutPaiementRegles.EstEnAttente(c.StatutPaiement))
                    .ToList();

                // Phase 2: Calculs en mémoire C# (client-side)
                var collectes = collectesData.Select(c => new CollecteEnAttenteDto
                {
                    IdCollecte = c.IdCollecte,
                    NomAffilie = c.NomAffilie,
                    PrenomAffilie = c.PrenomAffilie,
                    NomAgent = c.NomAgent,
                    Montant = Round2(c.Montant),
                    ReferencePaiement = c.ReferencePaiement,
                    ModePaiement = c.ModePaiement,
                    DateCollecte = c.DateCollecte,
                    StatutPaiement = c.StatutPaiement,
                    // ✅ Calculs C# purs (client-side)
                    HeuresAttente = Round2((decimal)(DateTime.Now - c.DateCollecte).TotalHours),
                    Priorite = (DateTime.Now - c.DateCollecte).TotalHours > 24 ? 1 : 
                               (DateTime.Now - c.DateCollecte).TotalHours > 12 ? 2 : 3
                }).ToList();

                _logger.LogInformation("Collectes en attente récupérées: {NbCollectes} collectes", collectes.Count);
                return collectes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des collectes en attente");
                throw;
            }
        }

        private async Task<List<CollecteMensuelleDto>> GetCollectesMensuellesAsync(int mois, CancellationToken ct)
        {
            var debutPeriode = DateTime.Now.AddMonths(-mois + 1);
            debutPeriode = new DateTime(debutPeriode.Year, debutPeriode.Month, 1);

            var collectes = await _db.Collectes
                .Where(c => c.DateCollecte >= debutPeriode && c.Statut)
                .GroupBy(c => new { c.DateCollecte.Year, c.DateCollecte.Month })
                .Select(g => new CollecteMensuelleDto
                {
                    Mois = $"{new DateTime(g.Key.Year, g.Key.Month, 1):MMM yyyy}",
                    Montant = g.Sum(c => c.Montant),
                    Nombre = g.Count(),
                    Progression = 0 // Sera calculé après
                })
                .OrderBy(c => c.Mois)
                .ToListAsync(ct);

            // Calcul des progressions
            for (int i = 1; i < collectes.Count; i++)
            {
                var montantPrecedent = collectes[i - 1].Montant;
                collectes[i].Progression = montantPrecedent > 0
                    ? Round2(((collectes[i].Montant - montantPrecedent) / montantPrecedent) * 100)
                    : 0;
            }

            foreach (var collecte in collectes)
                collecte.Montant = Round2(collecte.Montant);

            return collectes;
        }

        private async Task<List<AdhesionMensuelleDto>> GetAdhesionsMensuellesAsync(int mois, CancellationToken ct)
        {
            var debutPeriode = DateTime.Now.AddMonths(-mois + 1);
            debutPeriode = new DateTime(debutPeriode.Year, debutPeriode.Month, 1);

            var adhesions = await _db.Adhesions
                .Where(a => a.DateCreation >= debutPeriode && a.Statut)
                .GroupBy(a => new { a.DateCreation.Year, a.DateCreation.Month })
                .Select(g => new AdhesionMensuelleDto
                {
                    Mois = $"{new DateTime(g.Key.Year, g.Key.Month, 1):MMM yyyy}",
                    Nombre = g.Count(),
                    Progression = 0 // Sera calculé après
                })
                .OrderBy(a => a.Mois)
                .ToListAsync(ct);

            // Calcul des progressions
            for (int i = 1; i < adhesions.Count; i++)
            {
                var nombrePrecedent = adhesions[i - 1].Nombre;
                adhesions[i].Progression = nombrePrecedent > 0
                    ? Round2(((decimal)(adhesions[i].Nombre - nombrePrecedent) / nombrePrecedent) * 100)
                    : 0;
            }

            return adhesions;
        }

        private async Task<List<RepartitionAdhesionsDto>> GetRepartitionAdhesionsAsync(CancellationToken ct)
        {
            var totalAdhesions = await _db.Adhesions.CountAsync(a => a.Statut, ct);

            var repartition = await _db.Adhesions
                .Where(a => a.Statut)
                .GroupBy(a => a.TypeAdhesion.Libelle)
                .Select(g => new RepartitionAdhesionsDto
                {
                    TypeAdhesion = g.Key,
                    Nombre = g.Count(),
                    Pourcentage = totalAdhesions > 0 ? ((decimal)g.Count() / totalAdhesions) * 100 : 0
                })
                .OrderByDescending(r => r.Nombre)
                .ToListAsync(ct);

            foreach (var item in repartition)
                item.Pourcentage = Round2(item.Pourcentage);

            return repartition;
        }

        private static decimal Round2(decimal value) =>
            Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}
