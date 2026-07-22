using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProsocAPI.Models;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Services;
using Prosoc.Data;
using Prosoc.Utilities;

namespace ProsocAPI.Services.Repositories
{
    public class SuperviseurService : ISuperviseurRepository
    {
        private const string CommissionCollecteSource = "COMM_COLLECTE";

        private readonly ProsocDbContext _db;
        private readonly IDeviseConversionService _deviseConversion;
        private readonly ILogger<SuperviseurService> _logger;

        public SuperviseurService(
            ProsocDbContext db,
            IDeviseConversionService deviseConversion,
            ILogger<SuperviseurService> logger)
        {
            _db = db;
            _deviseConversion = deviseConversion;
            _logger = logger;
        }

        public async Task<SuperviseurStatsDto> GetStatsSuperviseurAsync(int superviseurId, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Récupération des statistiques du superviseur {SuperviseurId}", superviseurId);

                var superviseur = await _db.Agents
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.IdAgent == superviseurId, ct);

                if (superviseur == null)
                {
                    _logger.LogWarning("Superviseur {SuperviseurId} non trouvé", superviseurId);
                    return new SuperviseurStatsDto();
                }

                var idsAgents = await GetIdsAgentsDansHierarchieAsync(superviseurId, ct);
                var idsAgentsSansSuperviseur = idsAgents.Where(id => id != superviseurId).ToList();
                var dateDebut = DateTime.Now.AddMonths(-12);
                var devisePrincipale = await GetDevisePrincipaleAsync(ct);

                var stats = new SuperviseurStatsDto
                {
                    SuperviseurId = superviseurId,
                    NomSuperviseur = superviseur.NomComplet ?? "",
                    NombreAgentsDirects = idsAgentsSansSuperviseur.Count,
                    NombreAgentsTotal = idsAgentsSansSuperviseur.Count,
                    MontantTotalEquipe = await GetMontantTotalHierarchieAsync(superviseurId, ct),
                    PerformanceMoyenneEquipe = await GetPerformanceMoyenneHierarchieAsync(superviseurId, ct),
                    MontantTotalSuperviseur = await SommerCollectesAgentsAsync(
                        new[] { superviseurId }, dateDebut, null, ct),
                    NombreTransactionsSuperviseur = await _db.Collectes
                        .Where(c => c.AgentId == superviseurId && c.DateCollecte >= dateDebut)
                        .CountAsync(ct),
                    TauxSuccesEquipe = await GetTauxSuccesEquipeAsync(superviseurId, dateDebut, DateTime.Now, ct),
                    ObjectifEquipe = 3000000m, // Exemple
                    AtteinteObjectifEquipe = 0, // Calculé après
                    AgentsSupervises = (await GetPerformancesAgentsAsync(superviseurId, ct)).Select(p => new AgentPerformanceHierarchieDto
                    {
                        AgentId = p.AgentId,
                        NomAgent = p.NomAgent,
                        MontantTotal = p.MontantTotal,
                        NombreTransactions = p.NombreTransactions,
                        MontantMoyen = p.MontantMoyen,
                        TauxSucces = p.TauxSucces,
                        PerformanceMoyenne = p.PerformanceMoyenne,
                        ObjectifPersonnel = p.ObjectifPersonnel,
                        AtteinteObjectif = p.AtteinteObjectif,
                        RangEquipe = p.RangEquipe,
                        Progression = p.Progression,
                        DerniereActivite = p.DerniereActivite,
                        NombreJoursActifs = p.NombreJoursActifs,
                        MontantCommissions = p.MontantCommissions,
                        NetAPercevoir = p.NetAPercevoir
                    }).ToList(),
                    DerniereMiseAJour = DateTime.Now,
                    DevisePrincipaleCode = devisePrincipale.Code ?? ""
                };

                stats.AtteinteObjectifEquipe = stats.ObjectifEquipe > 0 ? (stats.MontantTotalEquipe / stats.ObjectifEquipe) * 100 : 0;

                _logger.LogInformation("Statistiques du superviseur récupérées avec succès");
                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des statistiques du superviseur");
                throw;
            }
        }

        public async Task<List<AgentPerformanceDto>> GetPerformancesAgentsAsync(int superviseurId, CancellationToken ct = default)
        {
            try
            {
                var idsAgents = await GetIdsAgentsDansHierarchieAsync(superviseurId, ct);
                var dateDebut = DateTime.Now.AddMonths(-12);

                var montantsParAgent = await SommerCollectesParAgentAsync(idsAgents, dateDebut, null, ct);
                var commissionsParAgent = await GrouperCommissionsParAgentAsync(idsAgents, dateDebut, null, ct);

                var statsTransactions = await _db.Collectes
                    .AsNoTracking()
                    .Where(c => c.AgentId.HasValue && idsAgents.Contains(c.AgentId.Value) && c.DateCollecte >= dateDebut)
                    .GroupBy(c => c.AgentId)
                    .Select(g => new
                    {
                        AgentId = g.Key,
                        NombreTransactions = g.Count(),
                        TransactionsReussies = g.Count(c => c.StatutPaiement != null && CollecteStatutPaiementRegles.ValeursSqlValideEtLegacy.Contains(c.StatutPaiement)),
                        DerniereActivite = g.Max(c => c.DateCollecte)
                    })
                    .ToListAsync(ct);

                var statsLookup = statsTransactions.ToDictionary(x => x.AgentId);

                var agents = await _db.Agents
                    .AsNoTracking()
                    .Where(a => idsAgents.Contains(a.IdAgent))
                    .Select(a => new { a.IdAgent, NomAgent = a.NomComplet ?? "" })
                    .ToListAsync(ct);

                var performances = agents.Select(a =>
                {
                    statsLookup.TryGetValue(a.IdAgent, out var stats);
                    var montantTotal = montantsParAgent.GetValueOrDefault(a.IdAgent);
                    var nombreTransactions = stats?.NombreTransactions ?? 0;
                    var montantCommissions = commissionsParAgent.GetValueOrDefault(a.IdAgent);

                    return new AgentPerformanceDto
                    {
                        AgentId = a.IdAgent,
                        NomAgent = a.NomAgent,
                        MontantTotal = montantTotal,
                        NombreTransactions = nombreTransactions,
                        MontantCommissions = montantCommissions,
                        NetAPercevoir = montantTotal * 0.73m,
                        DerniereActivite = stats?.DerniereActivite ?? DateTime.MinValue,
                        NombreJoursActifs = 30,
                        ObjectifPersonnel = 1000000m,
                        MontantMoyen = nombreTransactions > 0 ? montantTotal / nombreTransactions : 0,
                        TauxSucces = nombreTransactions > 0 && stats != null
                            ? (decimal)stats.TransactionsReussies / nombreTransactions * 100
                            : 0,
                        PerformanceMoyenne = montantTotal / 30,
                        AtteinteObjectif = montantTotal / 1000000m * 100
                    };
                }).ToList();

                var performancesClassees = performances
                    .OrderByDescending(p => p.MontantTotal)
                    .ToList();

                for (int i = 0; i < performancesClassees.Count; i++)
                {
                    performancesClassees[i].RangEquipe = i + 1;
                }

                return performancesClassees;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des performances des agents");
                throw;
            }
        }

        public async Task<AgentPerformanceDto> GetPerformanceAgentAsync(int superviseurId, int agentId, CancellationToken ct = default)
        {
            try
            {
                if (!await EstDansHierarchieAsync(superviseurId, agentId, ct))
                {
                    _logger.LogWarning("Agent {AgentId} n'est pas dans la hiérarchie du superviseur {SuperviseurId}", agentId, superviseurId);
                    return new AgentPerformanceDto();
                }

                var performances = await GetPerformancesAgentsAsync(superviseurId, ct);
                return performances.FirstOrDefault(p => p.AgentId == agentId) ?? new AgentPerformanceDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de la performance de l'agent");
                throw;
            }
        }

        public async Task<List<AgentPerformanceDto>> GetTopAgentsAsync(int superviseurId, int limit = 10, CancellationToken ct = default)
        {
            try
            {
                var performances = await GetPerformancesAgentsAsync(superviseurId, ct);
                return performances
                    .OrderByDescending(p => p.MontantTotal)
                    .Take(limit)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du top agents");
                throw;
            }
        }

        public async Task<HierarchieSuperviseurDto> GetHierarchieCompleteAsync(int superviseurId, CancellationToken ct = default)
        {
            try
            {
                var superviseur = await _db.Agents
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.IdAgent == superviseurId, ct);

                if (superviseur == null)
                {
                    return new HierarchieSuperviseurDto();
                }

                var idsAgents = await GetIdsAgentsDansHierarchieAsync(superviseurId, ct);
                var agentsHierarchie = await _db.Agents
                    .Where(a => idsAgents.Contains(a.IdAgent))
                    .ToListAsync(ct);

                var agentsDirects = agentsHierarchie.Where(a => a.IdAgent != superviseurId).ToList();
                var montantsParAgent = await SommerCollectesParAgentAsync(
                    agentsDirects.Select(a => a.IdAgent), null, null, ct);
                var transactionCounts = await _db.Collectes
                    .AsNoTracking()
                    .Where(c => c.AgentId.HasValue
                        && agentsDirects.Select(a => a.IdAgent).Contains(c.AgentId.Value))
                    .GroupBy(c => c.AgentId)
                    .Select(g => new { AgentId = g.Key, Count = g.Count() })
                    .ToListAsync(ct);
                var countsLookup = transactionCounts.ToDictionary(x => x.AgentId, x => x.Count);

                var hierarchie = new HierarchieSuperviseurDto
                {
                    SuperviseurId = superviseurId,
                    NomSuperviseur = superviseur.NomComplet ?? "",
                    NiveauHierarchique = 1,
                    AgentsSupervises = agentsDirects
                        .Select(a => new AgentHierarchieDto
                        {
                            AgentId = a.IdAgent,
                            NomAgent = a.NomComplet ?? "",
                            Matricule = a.Matricule ?? "",
                            Telephone = a.Phone ?? "",
                            Email = a.EmailAgent ?? "",
                            MontantTotal = montantsParAgent.GetValueOrDefault(a.IdAgent),
                            NombreTransactions = countsLookup.GetValueOrDefault(a.IdAgent),
                            PerformanceMoyenne = 0,
                            DateCreation = a.DateCreation,
                            Statut = a.Statut,
                            NiveauHierarchique = 2,
                            CheminHierarchique = $"{superviseur.NomComplet} > {a.NomComplet}"
                        })
                        .ToList(),
                    SousSuperviseurs = new List<HierarchieSuperviseurDto>(),
                    TotalAgentsDansHierarchie = idsAgents.Count,
                    MontantTotalHierarchie = await GetMontantTotalHierarchieAsync(superviseurId, ct)
                };

                // Calcul des performances moyennes
                foreach (var agent in hierarchie.AgentsSupervises)
                {
                    agent.PerformanceMoyenne = agent.NombreTransactions > 0 ? agent.MontantTotal / agent.NombreTransactions : 0;
                }

                return hierarchie;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de la hiérarchie");
                throw;
            }
        }

        public async Task<List<AgentHierarchieDto>> GetAgentsSupervisesDirectsAsync(int superviseurId, CancellationToken ct = default)
        {
            try
            {
                var idsAgents = await GetIdsAgentsDansHierarchieAsync(superviseurId, ct);
                var idsAgentsSansSuperviseur = idsAgents.Where(id => id != superviseurId).ToList();

                var agents = await _db.Agents
                    .AsNoTracking()
                    .Where(a => idsAgentsSansSuperviseur.Contains(a.IdAgent))
                    .ToListAsync(ct);

                var ids = agents.Select(a => a.IdAgent).ToList();
                var montantsParAgent = await SommerCollectesParAgentAsync(ids, null, null, ct);
                var transactionCounts = await _db.Collectes
                    .AsNoTracking()
                    .Where(c => c.AgentId.HasValue && ids.Contains(c.AgentId.Value))
                    .GroupBy(c => c.AgentId)
                    .Select(g => new { AgentId = g.Key, Count = g.Count() })
                    .ToListAsync(ct);
                var countsLookup = transactionCounts.ToDictionary(x => x.AgentId, x => x.Count);

                return agents.Select(a => new AgentHierarchieDto
                {
                    AgentId = a.IdAgent,
                    NomAgent = a.NomComplet ?? "",
                    Matricule = a.Matricule ?? "",
                    Telephone = a.Phone ?? "",
                    Email = a.EmailAgent ?? "",
                    MontantTotal = montantsParAgent.GetValueOrDefault(a.IdAgent),
                    NombreTransactions = countsLookup.GetValueOrDefault(a.IdAgent),
                    PerformanceMoyenne = countsLookup.GetValueOrDefault(a.IdAgent) > 0
                        ? montantsParAgent.GetValueOrDefault(a.IdAgent) / countsLookup[a.IdAgent]
                        : 0,
                    DateCreation = a.DateCreation,
                    Statut = a.Statut,
                    NiveauHierarchique = 2,
                    CheminHierarchique = ""
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des agents supervisés directs");
                throw;
            }
        }

        public async Task<List<AgentHierarchieDto>> GetTousAgentsHierarchieAsync(int superviseurId, CancellationToken ct = default)
        {
            try
            {
                var idsAgents = await GetIdsAgentsDansHierarchieAsync(superviseurId, ct);
                var agents = await _db.Agents
                    .AsNoTracking()
                    .Where(a => idsAgents.Contains(a.IdAgent))
                    .ToListAsync(ct);

                var montantsParAgent = await SommerCollectesParAgentAsync(idsAgents, null, null, ct);
                var transactionCounts = await _db.Collectes
                    .AsNoTracking()
                    .Where(c => c.AgentId.HasValue && idsAgents.Contains(c.AgentId.Value))
                    .GroupBy(c => c.AgentId)
                    .Select(g => new { AgentId = g.Key, Count = g.Count() })
                    .ToListAsync(ct);
                var countsLookup = transactionCounts.ToDictionary(x => x.AgentId, x => x.Count);

                return agents.Select(a => new AgentHierarchieDto
                {
                    AgentId = a.IdAgent,
                    NomAgent = a.NomComplet ?? "",
                    Matricule = a.Matricule ?? "",
                    Telephone = a.Phone ?? "",
                    Email = a.EmailAgent ?? "",
                    MontantTotal = montantsParAgent.GetValueOrDefault(a.IdAgent),
                    NombreTransactions = countsLookup.GetValueOrDefault(a.IdAgent),
                    PerformanceMoyenne = 0,
                    DateCreation = a.DateCreation,
                    Statut = a.Statut,
                    NiveauHierarchique = 0,
                    CheminHierarchique = ""
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de tous les agents de la hiérarchie");
                throw;
            }
        }

        public async Task<bool> EstDansHierarchieAsync(int superviseurId, int agentId, CancellationToken ct = default)
        {
            try
            {
                var idsAgents = await GetIdsAgentsDansHierarchieAsync(superviseurId, ct);
                return idsAgents.Contains(agentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification de l'appartenance à la hiérarchie");
                throw;
            }
        }

        public async Task<List<AffectationSuperviseurDto>> GetAffectationsRecentesAsync(int superviseurId, int limit = 20, CancellationToken ct = default)
        {
            try
            {
                // Simulé - nécessiterait une table d'affectations
                return new List<AffectationSuperviseurDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des affectations récentes");
                throw;
            }
        }

        public async Task<List<AffectationSuperviseurDto>> GetHistoriqueAffectationsAsync(int agentId, CancellationToken ct = default)
        {
            try
            {
                // Simulé - nécessiterait une table d'historique
                return new List<AffectationSuperviseurDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de l'historique des affectations");
                throw;
            }
        }

        public async Task<List<ObjectifEquipeDto>> GetObjectifsEquipeAsync(int superviseurId, CancellationToken ct = default)
        {
            try
            {
                // Simulé - nécessiterait une table d'objectifs
                return new List<ObjectifEquipeDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des objectifs d'équipe");
                throw;
            }
        }

        public async Task<ObjectifEquipeDto> CreerObjectifEquipeAsync(ObjectifEquipeDto objectif, CancellationToken ct = default)
        {
            try
            {
                // Simulé - nécessiterait une table d'objectifs
                _logger.LogInformation("Création d'objectif d'équipe simulée");
                return objectif;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de l'objectif d'équipe");
                throw;
            }
        }

        public async Task<bool> ModifierObjectifEquipeAsync(int objectifId, ObjectifEquipeDto objectif, CancellationToken ct = default)
        {
            try
            {
                // Simulé
                _logger.LogInformation("Modification d'objectif d'équipe simulée");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la modification de l'objectif d'équipe");
                throw;
            }
        }

        public async Task<bool> SupprimerObjectifEquipeAsync(int objectifId, CancellationToken ct = default)
        {
            try
            {
                // Simulé
                _logger.LogInformation("Suppression d'objectif d'équipe simulée");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de l'objectif d'équipe");
                throw;
            }
        }

        public async Task<RapportPerformanceEquipeDto> GetRapportPerformanceAsync(int superviseurId, DateTime debut, DateTime fin, CancellationToken ct = default)
        {
            try
            {
                var idsAgents = await GetIdsAgentsDansHierarchieAsync(superviseurId, ct);
                var superviseur = await _db.Agents.FindAsync(new object[] { superviseurId }, ct);

                var montantPeriode = await SommerCollectesAgentsAsync(idsAgents, debut, fin, ct);

                var rapport = new RapportPerformanceEquipeDto
                {
                    SuperviseurId = superviseurId,
                    NomSuperviseur = superviseur?.NomComplet ?? "",
                    DebutPeriode = debut,
                    FinPeriode = fin,
                    NombreAgents = idsAgents.Count,
                    MontantTotalEquipe = montantPeriode,
                    MontantMoyenParAgent = idsAgents.Count > 0 ? montantPeriode / idsAgents.Count : 0,
                    TotalTransactionsEquipe = await GetNombreTransactionsEquipeAsync(superviseurId, debut, fin, ct),
                    TauxSuccesEquipe = await GetTauxSuccesEquipeAsync(superviseurId, debut, fin, ct),
                    ObjectifEquipe = 3000000m, // Exemple
                    AtteinteObjectifEquipe = 0, // Calculé après
                    PerformancesAgents = (await GetPerformancesAgentsAsync(superviseurId, ct)).Select(p => new AgentPerformanceHierarchieDto
                    {
                        AgentId = p.AgentId,
                        NomAgent = p.NomAgent,
                        MontantTotal = p.MontantTotal,
                        NombreTransactions = p.NombreTransactions,
                        MontantMoyen = p.MontantMoyen,
                        TauxSucces = p.TauxSucces,
                        PerformanceMoyenne = p.PerformanceMoyenne,
                        ObjectifPersonnel = p.ObjectifPersonnel,
                        AtteinteObjectif = p.AtteinteObjectif,
                        RangEquipe = p.RangEquipe,
                        Progression = p.Progression,
                        DerniereActivite = p.DerniereActivite,
                        NombreJoursActifs = p.NombreJoursActifs,
                        MontantCommissions = p.MontantCommissions,
                        NetAPercevoir = p.NetAPercevoir
                    }).ToList(),
                    CroissanceParRapportPrecedent = 0, // Simplifié
                    RangParmiSuperviseurs = 0, // Simplifié
                    CommentairePerformance = "",
                    DateGenerationRapport = DateTime.Now
                };

                rapport.AtteinteObjectifEquipe = rapport.ObjectifEquipe > 0 ? (rapport.MontantTotalEquipe / rapport.ObjectifEquipe) * 100 : 0;

                return rapport;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la génération du rapport de performance");
                throw;
            }
        }

        public async Task<List<RapportPerformanceEquipeDto>> GetRapportsPeriodiquesAsync(int superviseurId, int mois = 6, CancellationToken ct = default)
        {
            try
            {
                var rapports = new List<RapportPerformanceEquipeDto>();
                var dateFin = DateTime.Now;
                
                for (int i = 0; i < mois; i++)
                {
                    var dateDebut = dateFin.AddMonths(-(i + 1));
                    var rapport = await GetRapportPerformanceAsync(superviseurId, dateDebut, dateFin, ct);
                    rapports.Add(rapport);
                }

                return rapports.OrderByDescending(r => r.DebutPeriode).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des rapports périodiques");
                throw;
            }
        }

        public async Task<byte[]> ExporterRapportPerformanceAsync(int superviseurId, DateTime debut, DateTime fin, CancellationToken ct = default)
        {
            try
            {
                // Simulé - nécessiterait une librairie d'export
                _logger.LogInformation("Export du rapport de performance simulé");
                return Array.Empty<byte>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'export du rapport de performance");
                throw;
            }
        }

        public async Task<ComparaisonEquipesDto> GetComparaisonEquipesAsync(List<int> superviseurIds, DateTime debut, DateTime fin, CancellationToken ct = default)
        {
            try
            {
                var equipes = new List<SuperviseurStatsDto>();
                
                foreach (var supId in superviseurIds)
                {
                    var stats = await GetStatsSuperviseurAsync(supId, ct);
                    equipes.Add(stats);
                }

                var meilleureEquipe = equipes.OrderByDescending(e => e.MontantTotalEquipe).FirstOrDefault();
                var equipeMoinsPerformante = equipes.OrderBy(e => e.MontantTotalEquipe).FirstOrDefault();

                return new ComparaisonEquipesDto
                {
                    Equipes = equipes,
                    DebutPeriode = debut,
                    FinPeriode = fin,
                    MontantTotalGeneral = equipes.Sum(e => e.MontantTotalEquipe),
                    NombreAgentsTotal = equipes.Sum(e => e.NombreAgentsTotal),
                    PerformanceMoyenneGenerale = equipes.Count > 0 ? equipes.Average(e => e.PerformanceMoyenneEquipe) : 0,
                    MeilleureEquipe = meilleureEquipe,
                    EquipeMoinsPerformante = equipeMoinsPerformante,
                    EcartPerformance = meilleureEquipe?.MontantTotalEquipe - (equipeMoinsPerformante?.MontantTotalEquipe ?? 0) ?? 0,
                    NombreEquipesComparees = superviseurIds.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la comparaison des équipes");
                throw;
            }
        }

        public async Task<List<SuperviseurStatsDto>> GetClassementSuperviseursAsync(DateTime debut, DateTime fin, CancellationToken ct = default)
        {
            try
            {
                var superviseurIds = await _db.Communes
                    .AsNoTracking()
                    .Where(c => c.SuperviseurAgentId.HasValue)
                    .Select(c => c.SuperviseurAgentId!.Value)
                    .Distinct()
                    .ToListAsync(ct);

                var classements = new List<SuperviseurStatsDto>();
                
                foreach (var superviseurId in superviseurIds)
                {
                    var stats = await GetStatsSuperviseurAsync(superviseurId, ct);
                    classements.Add(stats);
                }

                return classements
                    .OrderByDescending(c => c.MontantTotalEquipe)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du classement des superviseurs");
                throw;
            }
        }

        public async Task<List<TendanceEquipeDto>> GetTendancesEquipeAsync(int superviseurId, int mois = 12, CancellationToken ct = default)
        {
            try
            {
                var tendances = new List<TendanceEquipeDto>();
                var dateFin = DateTime.Now;
                var idsAgents = await GetIdsAgentsDansHierarchieAsync(superviseurId, ct);
                var nomSuperviseur = await _db.Agents
                    .AsNoTracking()
                    .Where(a => a.IdAgent == superviseurId)
                    .Select(a => a.NomComplet ?? "")
                    .FirstOrDefaultAsync(ct) ?? "";

                for (int i = 0; i < mois; i++)
                {
                    var dateDebut = dateFin.AddMonths(-(i + 1));
                    var dateFinMois = dateDebut.AddMonths(1).AddTicks(-1);

                    var montantPeriode = await SommerCollectesAgentsAsync(idsAgents, dateDebut, dateFinMois, ct);

                    var tendance = new TendanceEquipeDto
                    {
                        SuperviseurId = superviseurId,
                        NomSuperviseur = nomSuperviseur,
                        Periode = dateDebut.ToString("yyyy-MM"),
                        MontantPeriode = montantPeriode,
                        NombreAgentsPeriode = await GetNombreTotalAgentsHierarchieAsync(superviseurId, ct),
                        PerformanceMoyennePeriode = await GetPerformanceMoyenneHierarchieAsync(superviseurId, ct),
                        TauxSuccesPeriode = await GetTauxSuccesEquipeAsync(superviseurId, dateDebut, dateFinMois, ct),
                        Croissance = 0, // Calculé après
                        ObjectifPeriode = 250000m, // Exemple
                        AtteinteObjectifPeriode = 0, // Calculé après
                        NombreTransactionsPeriode = await GetNombreTransactionsEquipeAsync(superviseurId, dateDebut, dateFinMois, ct),
                        MontantCommissionsPeriode = await GetMontantCommissionsHierarchieAsync(superviseurId, dateDebut, dateFinMois, ct)
                    };
                    
                    tendance.AtteinteObjectifPeriode = tendance.ObjectifPeriode > 0 ? (tendance.MontantPeriode / tendance.ObjectifPeriode) * 100 : 0;
                    tendances.Add(tendance);
                }

                // Calcul des croissances
                for (int i = 1; i < tendances.Count; i++)
                {
                    var actuel = tendances[i];
                    var precedent = tendances[i - 1];
                    
                    if (precedent.MontantPeriode > 0)
                    {
                        actuel.Croissance = ((actuel.MontantPeriode - precedent.MontantPeriode) / precedent.MontantPeriode) * 100;
                    }
                }

                return tendances.OrderBy(t => t.Periode).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des tendances de l'équipe");
                throw;
            }
        }

        public async Task<List<TendanceEquipeDto>> GetTendancesGeneralesAsync(int mois = 12, CancellationToken ct = default)
        {
            try
            {
                // Simulé - tendances générales de tous les superviseurs
                return new List<TendanceEquipeDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des tendances générales");
                throw;
            }
        }

        public async Task<ActiviteSuperviseurDto> GetActiviteJournaliereAsync(int superviseurId, CancellationToken ct = default)
        {
            try
            {
                var idsAgents = await GetIdsAgentsDansHierarchieAsync(superviseurId, ct);
                var superviseur = await _db.Agents.FindAsync(new object[] { superviseurId }, ct);

                return new ActiviteSuperviseurDto
                {
                    SuperviseurId = superviseurId,
                    NomSuperviseur = superviseur?.NomComplet ?? "",
                    DateActivite = DateTime.Today,
                    NombreAgentsConnectes = idsAgents.Count,
                    NombreTransactionsEquipe = await GetNombreTransactionsEquipeAsync(superviseurId, DateTime.Today, DateTime.Now, ct),
                    MontantTransactionsEquipe = await SommerCollectesAgentsAsync(idsAgents, DateTime.Today, DateTime.Now, ct),
                    TauxSuccesEquipe = await GetTauxSuccesEquipeAsync(superviseurId, DateTime.Today, DateTime.Now, ct),
                    NombreNouveauxAgents = 0, // Simplifié
                    NombreAgentsDesactives = 0, // Simplifié
                    ActionsRealisees = new List<string>(), // Simplifié
                    TempsMoyenResponse = 0, // Simplifié
                    NombreAlertes = 0,
                    NombreProblemesResolus = 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de l'activité journalière");
                throw;
            }
        }

        public async Task<List<ActiviteSuperviseurDto>> GetActivitePeriodiqueAsync(int superviseurId, DateTime debut, DateTime fin, CancellationToken ct = default)
        {
            try
            {
                var activites = new List<ActiviteSuperviseurDto>();
                var jours = (int)(fin - debut).TotalDays;
                
                for (int i = 0; i < jours; i++)
                {
                    var dateJour = debut.AddDays(i);
                    var activite = await GetActiviteJournaliereAsync(superviseurId, ct);
                    activite.DateActivite = dateJour;
                    activites.Add(activite);
                }

                return activites;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de l'activité périodique");
                throw;
            }
        }

        public async Task<PermissionSuperviseurDto> GetPermissionsSuperviseurAsync(int superviseurId, CancellationToken ct = default)
        {
            try
            {
                var superviseur = await _db.Agents.FindAsync(new object[] { superviseurId }, ct);

                return new PermissionSuperviseurDto
                {
                    SuperviseurId = superviseurId,
                    NomSuperviseur = superviseur?.NomComplet ?? "",
                    PeutVoirTousAgents = true,
                    PeutModifierAgents = true,
                    PeutAssignerObjectifs = true,
                    PeutVoirRapports = true,
                    PeutExporterDonnees = true,
                    AgentsAccessibles = await GetIdsAgentsDansHierarchieAsync(superviseurId, ct),
                    PermissionsSpecifiques = new List<string> { "LECTURE_AGENTS", "MODIFICATION_AGENTS", "RAPPORTS", "EXPORT" },
                    DateModificationPermissions = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des permissions du superviseur");
                throw;
            }
        }

        public async Task<bool> ModifierPermissionsSuperviseurAsync(int superviseurId, PermissionSuperviseurDto permissions, CancellationToken ct = default)
        {
            try
            {
                // Simulé - nécessiterait une table de permissions
                _logger.LogInformation("Modification des permissions du superviseur simulée");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la modification des permissions du superviseur");
                throw;
            }
        }

        public async Task<DashboardSuperviseurDto> GetDashboardSuperviseurAsync(int superviseurId, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Récupération du dashboard superviseur {SuperviseurId}", superviseurId);

                var dashboard = new DashboardSuperviseurDto
                {
                    StatsSuperviseur = await GetStatsSuperviseurAsync(superviseurId, ct),
                    TopAgents = await GetTopAgentsAsync(superviseurId, 5, ct),
                    TendancesEquipe = await GetTendancesEquipeAsync(superviseurId, 6, ct),
                    ObjectifsEquipe = await GetObjectifsEquipeAsync(superviseurId, ct),
                    RapportPerformance = await GetRapportPerformanceAsync(superviseurId, DateTime.Now.AddMonths(-1), DateTime.Now, ct),
                    HierarchieComplete = await GetHierarchieCompleteAsync(superviseurId, ct),
                    AffectationsRecentes = await GetAffectationsRecentesAsync(superviseurId, 10, ct),
                    DerniereMiseAJour = DateTime.Now,
                    MontantTotalHierarchie = await GetMontantTotalHierarchieAsync(superviseurId, ct),
                    NombreTotalAgentsHierarchie = await GetNombreTotalAgentsHierarchieAsync(superviseurId, ct),
                    PerformanceMoyenneHierarchie = await GetPerformanceMoyenneHierarchieAsync(superviseurId, ct)
                };
                dashboard.DevisePrincipaleCode = dashboard.StatsSuperviseur.DevisePrincipaleCode;

                _logger.LogInformation("Dashboard superviseur récupéré avec succès");
                return dashboard;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du dashboard superviseur");
                throw;
            }
        }

        // Méthodes utilitaires privées
        public async Task<List<int>> GetIdsAgentsDansHierarchieAsync(int superviseurId, CancellationToken ct = default)
        {
            var communeId = await SuperviseurTerritoryScopeHelper.GetCommuneIdForSuperviseurAsync(_db, superviseurId, ct);
            if (communeId.HasValue)
            {
                var ids = await SuperviseurTerritoryScopeHelper.GetAgentIdsDansCommuneAsync(_db, communeId.Value, ct);
                if (!ids.Contains(superviseurId))
                    ids.Add(superviseurId);
                return ids;
            }
            
            throw new ProsocAPI.Exceptions.SuperviseurSansCommuneTitulaireException(superviseurId);
        }

        public async Task<int> GetNombreTotalAgentsHierarchieAsync(int superviseurId, CancellationToken ct = default)
        {
            return (await GetIdsAgentsDansHierarchieAsync(superviseurId, ct)).Count;
        }

        public async Task<decimal> GetMontantTotalHierarchieAsync(int superviseurId, CancellationToken ct = default)
        {
            var idsAgents = await GetIdsAgentsDansHierarchieAsync(superviseurId, ct);
            var dateDebut = DateTime.Now.AddMonths(-12);
            return await SommerCollectesAgentsAsync(idsAgents, dateDebut, null, ct);
        }

        public async Task<decimal> GetPerformanceMoyenneHierarchieAsync(int superviseurId, CancellationToken ct = default)
        {
            var montantTotal = await GetMontantTotalHierarchieAsync(superviseurId, ct);
            var nombreAgents = await GetNombreTotalAgentsHierarchieAsync(superviseurId, ct);
            
            return nombreAgents > 0 ? montantTotal / nombreAgents : 0;
        }

        public async Task<bool> VerifierPermissionAgentAsync(int superviseurId, int agentId, CancellationToken ct = default)
        {
            return await EstDansHierarchieAsync(superviseurId, agentId, ct);
        }

        public async Task<decimal> GetTauxCroissanceEquipeAsync(int superviseurId, DateTime debut, DateTime fin, CancellationToken ct = default)
        {
            var idsAgents = await GetIdsAgentsDansHierarchieAsync(superviseurId, ct);

            var collectesPeriode1 = await _db.Collectes
                .AsNoTracking()
                .Where(c => c.AgentId.HasValue && idsAgents.Contains(c.AgentId.Value) && c.DateCollecte >= debut && c.DateCollecte < fin)
                .ToListAsync(ct);
            var montantPeriode1 = DashboardDeviseConsolidation.SommerCollectesEnDevisePrincipale(collectesPeriode1);

            var debutPrecedent = debut.AddMonths(-1);
            var collectesPeriode2 = await _db.Collectes
                .AsNoTracking()
                .Where(c => c.AgentId.HasValue && idsAgents.Contains(c.AgentId.Value) && c.DateCollecte >= debutPrecedent && c.DateCollecte < debut)
                .ToListAsync(ct);
            var montantPeriode2 = DashboardDeviseConsolidation.SommerCollectesEnDevisePrincipale(collectesPeriode2);

            return montantPeriode2 > 0 ? ((montantPeriode1 - montantPeriode2) / montantPeriode2) * 100 : 0;
        }

        public async Task<decimal> GetTauxSuccesEquipeAsync(int superviseurId, DateTime debut, DateTime fin, CancellationToken ct = default)
        {
            var idsAgents = await GetIdsAgentsDansHierarchieAsync(superviseurId, ct);
            var totalTransactions = await _db.Collectes
                .Where(c => c.AgentId.HasValue && idsAgents.Contains(c.AgentId.Value) && c.DateCollecte >= debut && c.DateCollecte <= fin)
                .CountAsync(ct);

            var transactionsReussies = await _db.Collectes
                .Where(c => c.AgentId.HasValue && idsAgents.Contains(c.AgentId.Value) && c.DateCollecte >= debut && c.DateCollecte <= fin && c.StatutPaiement != null && CollecteStatutPaiementRegles.ValeursSqlValideEtLegacy.Contains(c.StatutPaiement))
                .CountAsync(ct);

            return totalTransactions > 0 ? (decimal)transactionsReussies / totalTransactions * 100 : 0;
        }

        public async Task<decimal> GetMontantMoyenParAgentAsync(int superviseurId, DateTime debut, DateTime fin, CancellationToken ct = default)
        {
            var montantTotal = await GetMontantTotalHierarchieAsync(superviseurId, ct);
            var nombreAgents = await GetNombreTotalAgentsHierarchieAsync(superviseurId, ct);
            
            return nombreAgents > 0 ? montantTotal / nombreAgents : 0;
        }

        private async Task<decimal> GetMontantCommissionsHierarchieAsync(int superviseurId, DateTime debut, DateTime fin, CancellationToken ct)
        {
            var idsAgents = await GetIdsAgentsDansHierarchieAsync(superviseurId, ct);
            var commissionsParAgent = await GrouperCommissionsParAgentAsync(idsAgents, debut, fin, ct);
            return commissionsParAgent.Values.Sum();
        }

        public async Task<int> GetNombreTransactionsEquipeAsync(int superviseurId, DateTime debut, DateTime fin, CancellationToken ct = default)
        {
            var idsAgents = await GetIdsAgentsDansHierarchieAsync(superviseurId, ct);
            return await _db.Collectes
                .Where(c => c.AgentId.HasValue && idsAgents.Contains(c.AgentId.Value) && c.DateCollecte >= debut && c.DateCollecte <= fin)
                .CountAsync(ct);
        }

        public async Task<List<string>> GetAlertesEquipeAsync(int superviseurId, CancellationToken ct = default)
        {
            try
            {
                // Simulé - nécessiterait une table d'alertes
                return new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des alertes de l'équipe");
                throw;
            }
        }

        public async Task<bool> CreerAlerteEquipeAsync(int superviseurId, string message, CancellationToken ct = default)
        {
            try
            {
                // Simulé
                _logger.LogInformation("Création d'alerte d'équipe simulée");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de l'alerte d'équipe");
                throw;
            }
        }

        public async Task<bool> MarquerAlerteLueAsync(int alerteId, CancellationToken ct = default)
        {
            try
            {
                // Simulé
                _logger.LogInformation("Alerte {AlerteId} marquée comme lue", alerteId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du marquage de l'alerte comme lue");
                throw;
            }
        }

        public async Task<byte[]> ExporterDonneesEquipeAsync(int superviseurId, DateTime debut, DateTime fin, string format = "Excel", CancellationToken ct = default)
        {
            try
            {
                // Simulé - nécessiterait une librairie d'export
                _logger.LogInformation("Export des données de l'équipe simulé");
                return Array.Empty<byte>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'export des données de l'équipe");
                throw;
            }
        }

        public async Task<byte[]> ExporterHierarchieAsync(int superviseurId, CancellationToken ct = default)
        {
            try
            {
                // Simulé
                _logger.LogInformation("Export de la hiérarchie simulé");
                return Array.Empty<byte>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'export de la hiérarchie");
                throw;
            }
        }

        public async Task<byte[]> ExporterPerformancesAgentsAsync(int superviseurId, DateTime debut, DateTime fin, CancellationToken ct = default)
        {
            try
            {
                // Simulé
                _logger.LogInformation("Export des performances des agents simulé");
                return Array.Empty<byte>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'export des performances des agents");
                throw;
            }
        }

        private async Task<(int? Id, string? Code)> GetDevisePrincipaleAsync(CancellationToken ct)
        {
            var devise = await _db.Devises
                .AsNoTracking()
                .Where(d => d.EstDevisePrincipale && d.Statut)
                .Select(d => new { d.IdDevise, d.Code })
                .FirstOrDefaultAsync(ct);

            return devise == null ? (null, null) : (devise.IdDevise, devise.Code);
        }

        private async Task<decimal> SommerCollectesAgentsAsync(
            IEnumerable<int> idsAgents,
            DateTime? debut,
            DateTime? fin,
            CancellationToken ct)
        {
            var ids = idsAgents.ToList();
            if (ids.Count == 0)
                return 0;

            var query = _db.Collectes.AsNoTracking().Where(c => c.AgentId.HasValue && ids.Contains(c.AgentId.Value));
            if (debut.HasValue)
                query = query.Where(c => c.DateCollecte >= debut.Value);
            if (fin.HasValue)
                query = query.Where(c => c.DateCollecte <= fin.Value);

            var collectes = await query.ToListAsync(ct);
            return DashboardDeviseConsolidation.SommerCollectesEnDevisePrincipale(collectes);
        }

        private async Task<Dictionary<int, decimal>> SommerCollectesParAgentAsync(
            IEnumerable<int> idsAgents,
            DateTime? debut,
            DateTime? fin,
            CancellationToken ct)
        {
            var ids = idsAgents.ToList();
            if (ids.Count == 0)
                return new Dictionary<int, decimal>();

            var query = _db.Collectes.AsNoTracking().Where(c => c.AgentId.HasValue && ids.Contains(c.AgentId.Value));
            if (debut.HasValue)
                query = query.Where(c => c.DateCollecte >= debut.Value);
            if (fin.HasValue)
                query = query.Where(c => c.DateCollecte <= fin.Value);

            var collectes = await query.ToListAsync(ct);
            return collectes
                .GroupBy(c => c.AgentId!.Value)
                .ToDictionary(g => g.Key, g => DashboardDeviseConsolidation.SommerCollectesEnDevisePrincipale(g));
        }

        private async Task<Dictionary<int, decimal>> GrouperCommissionsParAgentAsync(
            IEnumerable<int> idsAgents,
            DateTime? debut,
            DateTime? fin,
            CancellationToken ct)
        {
            var ids = idsAgents.ToList();
            if (ids.Count == 0)
                return new Dictionary<int, decimal>();

            var query = from m in _db.WalletMouvements.AsNoTracking()
                        join w in _db.WalletsAgents.AsNoTracking() on m.WalletId equals w.IdWalletAgent
                        where m.Source == CommissionCollecteSource && ids.Contains(w.AgentId)
                        select new { w.AgentId, m.Montant, m.DeviseId, m.DateOperation };

            if (debut.HasValue)
                query = query.Where(x => x.DateOperation >= debut.Value);
            if (fin.HasValue)
                query = query.Where(x => x.DateOperation <= fin.Value);

            var mouvements = await query.ToListAsync(ct);
            var devisePrincipale = await GetDevisePrincipaleAsync(ct);
            var result = new Dictionary<int, decimal>();

            foreach (var group in mouvements.GroupBy(x => x.AgentId))
            {
                if (!devisePrincipale.Id.HasValue)
                {
                    result[group.Key] = group.Sum(x => x.Montant);
                    continue;
                }

                decimal total = 0;
                foreach (var mouvement in group)
                {
                    total += await DashboardDeviseConsolidation.MontantMouvementEnDevisePrincipaleAsync(
                        _deviseConversion,
                        mouvement.Montant,
                        mouvement.DeviseId,
                        devisePrincipale.Id,
                        mouvement.DateOperation,
                        ct);
                }

                result[group.Key] = total;
            }

            return result;
        }
    }
}
