using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProsocAPI.Helpers;
using ProsocAPI.Models;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.Pagination;
using ProsocAPI.Services;
using Prosoc.Data;

namespace ProsocAPI.Services.Repositories
{
    public class DashboardPercepteurService : IDashboardPercepteurRepository
    {
        private const string CommissionCollecteSource = "COMM_COLLECTE";

        private readonly ProsocDbContext _db;
        private readonly IDeviseConversionService _deviseConversion;
        private readonly IPerceptionVirtuelleService _perceptionVirtuelleService;
        private readonly ILogger<DashboardPercepteurService> _logger;

        public DashboardPercepteurService(
            ProsocDbContext db,
            IDeviseConversionService deviseConversion,
            IPerceptionVirtuelleService perceptionVirtuelleService,
            ILogger<DashboardPercepteurService> logger)
        {
            _db = db;
            _deviseConversion = deviseConversion;
            _perceptionVirtuelleService = perceptionVirtuelleService;
            _logger = logger;
        }

        public async Task<PercepteurKpisDto> GetKpisPercepteurAsync(CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Récupération des KPIs du percepteur");

                var dateDebut = DateTime.Now.AddMonths(-12);
                var dateDebutPrecedente = dateDebut.AddMonths(-12);

                // Période actuelle
                var montantActuel = await GetMontantPerçuAsync(dateDebut, DateTime.Now, ct);
                var transactionsActuel = await GetNombreTransactionsAsync(dateDebut, DateTime.Now, ct);

                // Période précédente
                var montantPrecedent = await GetMontantPerçuAsync(dateDebutPrecedente, dateDebut, ct);
                var devisePrincipale = await GetDevisePrincipaleAsync(ct);
                var (montantVirtuelEnAttente, nombreVirtuelEnAttente) =
                    await _perceptionVirtuelleService.GetTotauxVirtuelsEnAttenteAsync(ct);

                var kpis = new PercepteurKpisDto
                {
                    MontantTotalPerçu = montantActuel,
                    MontantDuJour = await GetMontantPerçuAsync(DateTime.Today, DateTime.Now, ct),
                    MontantSemaine = await GetMontantPerçuAsync(DateTime.Today.AddDays(-7), DateTime.Now, ct),
                    MontantMois = await GetMontantPerçuAsync(DateTime.Today.AddDays(-30), DateTime.Now, ct),
                    MontantAnnee = montantActuel,
                    NombreTotalTransactions = transactionsActuel,
                    TransactionsDuJour = await GetNombreTransactionsAsync(DateTime.Today, DateTime.Now, ct),
                    TransactionsSemaine = await GetNombreTransactionsAsync(DateTime.Today.AddDays(-7), DateTime.Now, ct),
                    TransactionsMois = await GetNombreTransactionsAsync(DateTime.Today.AddDays(-30), DateTime.Now, ct),
                    MontantMoyenTransaction = transactionsActuel > 0 ? montantActuel / transactionsActuel : 0,
                    TauxCroissance = montantPrecedent > 0 ? ((montantActuel - montantPrecedent) / montantPrecedent) * 100 : 0,
                    ObjectifJournalier = 100000m, // Exemple
                    AtteinteObjectifJournalier = 0, // Calculé après
                    NombreAgentsActifs = await _db.Agents.CountAsync(ct),
                    TauxSucces = await GetTauxSuccesAsync(dateDebut, DateTime.Now, ct),
                    DevisePrincipaleCode = devisePrincipale.Code,
                    MontantVirtuelEnAttente = montantVirtuelEnAttente,
                    NombreCollectesVirtuellesEnAttente = nombreVirtuelEnAttente
                };

                // Calcul de l'atteinte d'objectif journalier
                kpis.AtteinteObjectifJournalier = kpis.ObjectifJournalier > 0 ? (kpis.MontantDuJour / kpis.ObjectifJournalier) * 100 : 0;
                kpis.RapportPerception = await GetRapportPerceptionSyntheseAsync(ct: ct);

                _logger.LogInformation("KPIs du percepteur récupérés avec succès");
                return kpis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des KPIs du percepteur");
                throw;
            }
        }

        public async Task<PercepteurGraphsDto> GetGraphsPercepteurAsync(CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Récupération des graphiques du percepteur");

                var graphs = new PercepteurGraphsDto
                {
                    PerformancesJournalieres = await GetPerformancesJournalieresAsync(30, ct),
                    ResumeMensuels = await GetResumeMensuelsAsync(12, ct),
                    TransactionsParType = await GetTransactionsParTypeAsync(ct),
                    PaiementsParMode = await GetPaiementsParModeAsync(ct),
                    Tendances = await GetTendancesTransactionsAsync(30, ct),
                    ResumeFrais = await GetResumeFraisAsync(ct)
                };

                _logger.LogInformation("Graphiques du percepteur récupérés avec succès");
                return graphs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des graphiques du percepteur");
                throw;
            }
        }

        public async Task<List<PercepteurTransactionDto>> GetTransactionsAsync(int limit = 50, CancellationToken ct = default)
        {
            try
            {
                var transactions = await _db.Collectes
                    .Include(c => c.Agent)
                    .Include(c => c.Affilie)
                    .OrderByDescending(c => c.DateCollecte)
                    .Take(limit)
                    .Select(c => new PercepteurTransactionDto
                    {
                        IdTransaction = c.IdCollecte,
                        DateTransaction = c.DateCollecte,
                        Montant = c.MontantDevisePrincipale ?? c.Montant,
                        TypeTransaction = "Collecte",
                        Statut = c.StatutPaiement ?? "En attente",
                        Reference = c.ReferencePaiement ?? "",
                        NomAgent = c.Agent.NomComplet ?? "",
                        NomAffilie = c.Affilie.Nom ?? "",
                        ModePaiement = c.ModePaiement ?? "",
                        Commission = 0,
                        Frais = (c.MontantDevisePrincipale ?? c.Montant) * 0.02m, // 2% de frais
                        NetAPercevoir = (c.MontantDevisePrincipale ?? c.Montant) * 0.73m, // 75% - 2%
                        Notes = c.Observation
                    })
                    .ToListAsync(ct);

                _logger.LogInformation("Transactions récupérées: {Count} transactions", transactions.Count);
                return transactions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des transactions");
                throw;
            }
        }

        public async Task<List<PerformanceJournaliereDto>> GetPerformancesJournalieresAsync(int jours = 30, CancellationToken ct = default)
        {
            try
            {
                var dateFin = DateTime.Now;
                var dateDebut = dateFin.AddDays(-jours);

                // ✅ ÉTAPE 1: Requête SQL pure (traduisible)
                var rawData = await _db.Collectes
                    .Where(c => c.DateCollecte >= dateDebut && c.DateCollecte <= dateFin)
                    .GroupBy(c => c.DateCollecte.Date)
                    .Select(g => new
                    {
                        Date = g.Key,  // ✅ Garder DateTime, pas de ToString()
                        MontantTotal = g.Sum(c => c.MontantDevisePrincipale ?? c.Montant),
                        NombreTransactions = g.Count(),
                        TransactionsReussies = g.Count(c => c.StatutPaiement != null && CollecteStatutPaiementRegles.ValeursSqlValideEtLegacy.Contains(c.StatutPaiement)),
                        TransactionsEchouees = g.Count(c => c.StatutPaiement == CollecteStatutPaiement.EnAttente)
                    })
                    .OrderBy(x => x.Date)
                    .ToListAsync(ct);

                // ✅ ÉTAPE 2: Traitement en C# (non traduit)
                var performances = rawData.Select(g => new PerformanceJournaliereDto
                {
                    Date = g.Date.ToString("yyyy-MM-dd"),  // ✅ Maintenant autorisé
                    MontantTotal = g.MontantTotal,
                    NombreTransactions = g.NombreTransactions,
                    MontantMoyen = g.NombreTransactions > 0 ? g.MontantTotal / g.NombreTransactions : 0,
                    ObjectifJournalier = 100000m, // Exemple
                    TransactionsReussies = g.TransactionsReussies,
                    TransactionsEchouees = g.TransactionsEchouees,
                    MontantCommissions = 0,
                    MontantFrais = g.MontantTotal * 0.02m          // ✅ Maintenant autorisé
                }).ToList();

                // ✅ ÉTAPE 3: Calculs finaux
                foreach (var perf in performances)
                {
                    perf.TauxSucces = perf.NombreTransactions > 0 ? (decimal)perf.TransactionsReussies / perf.NombreTransactions * 100 : 0;
                    perf.AtteinteObjectif = perf.ObjectifJournalier > 0 ? (perf.MontantTotal / perf.ObjectifJournalier) * 100 : 0;
                }

                return performances;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des performances journalières");
                throw;
            }
        }

        public async Task<List<ResumeMensuelDto>> GetResumeMensuelsAsync(int mois = 12, CancellationToken ct = default)
        {
            try
            {
                var dateFin = DateTime.Now;
                var dateDebut = dateFin.AddMonths(-mois);

                // ✅ ÉTAPE 1: Requête SQL pure (traduisible)
                var rawData = await _db.Collectes
                    .Where(c => c.DateCollecte >= dateDebut && c.DateCollecte <= dateFin)
                    .GroupBy(c => new { c.DateCollecte.Year, c.DateCollecte.Month })
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        MontantTotal = g.Sum(c => c.MontantDevisePrincipale ?? c.Montant),
                        NombreTransactions = g.Count(),
                        TransactionsReussies = g.Count(c => c.StatutPaiement != null && CollecteStatutPaiementRegles.ValeursSqlValideEtLegacy.Contains(c.StatutPaiement))
                    })
                    .OrderBy(x => x.Year)
                    .ThenBy(x => x.Month)
                    .ToListAsync(ct);

                // ✅ ÉTAPE 2: Traitement en C# (non traduit)
                var resumes = rawData.Select(g => new ResumeMensuelDto
                {
                    Mois = $"{g.Year}-{g.Month:D2}",  // ✅ Maintenant autorisé
                    MontantTotal = g.MontantTotal,
                    NombreTransactions = g.NombreTransactions,
                    MontantMoyen = g.NombreTransactions > 0 ? g.MontantTotal / g.NombreTransactions : 0,
                    ObjectifMensuel = 3000000m, // Exemple
                    TransactionsReussies = g.TransactionsReussies,
                    MontantCommissions = 0,
                    MontantFrais = g.MontantTotal * 0.02m,        // ✅ Maintenant autorisé
                    NetAPercevoir = g.MontantTotal * 0.73m,       // ✅ Maintenant autorisé
                    NombreAgentsActifs = 0 // Simplifié pour l'instant
                }).ToList();

                // ✅ ÉTAPE 3: Calculs finaux optimisés
                for (int i = 0; i < resumes.Count; i++)
                {
                    var resume = resumes[i];
                    
                    // Calcul atteinte objectif
                    resume.AtteinteObjectif = resume.ObjectifMensuel > 0 ? 
                        (resume.MontantTotal / resume.ObjectifMensuel) * 100 : 0;
                    
                    // Calcul taux de succès (optimisé)
                    resume.TauxSucces = resume.NombreTransactions > 0 ? 
                        (decimal)resume.TransactionsReussies / resume.NombreTransactions * 100 : 0;
                    
                    // Calcul croissance
                    if (i > 0)
                    {
                        var precedent = resumes[i - 1];
                        resume.Croissance = precedent.MontantTotal > 0 ? 
                            ((resume.MontantTotal - precedent.MontantTotal) / precedent.MontantTotal) * 100 : 0;
                    }
                }

                return resumes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des résumés mensuels");
                throw;
            }
        }

        public async Task<List<TopAgentPercepteurDto>> GetTopAgentsPerformanceAsync(int limit = 10, CancellationToken ct = default)
        {
            try
            {
                // ✅ ÉTAPE 1: Requête SQL pure avec syntaxe EF Core standard
                var agentStats = await _db.Agents
                    .Select(a => new
                    {
                        AgentId = a.IdAgent,
                        NomAgent = a.NomComplet ?? "",
                        // ✅ Utilisation de sous-requêtes corrélées (traduisible)
                        MontantTotal = _db.Collectes
                            .Where(c => c.AgentId == a.IdAgent)
                            .Sum(c => (decimal?)(c.MontantDevisePrincipale ?? c.Montant)) ?? 0,
                        NombreTransactions = _db.Collectes
                            .Where(c => c.AgentId == a.IdAgent)
                            .Count(),
                        TransactionsReussies = _db.Collectes
                            .Where(c => c.AgentId == a.IdAgent && c.StatutPaiement != null && CollecteStatutPaiementRegles.ValeursSqlValideEtLegacy.Contains(c.StatutPaiement))
                            .Count(),
                        DerniereTransaction = _db.Collectes
                            .Where(c => c.AgentId == a.IdAgent)
                            .Max(c => (DateTime?)c.DateCollecte)
                    })
                    .Where(a => a.MontantTotal > 0) // ✅ Filtrer les agents avec des transactions
                    .OrderByDescending(a => a.MontantTotal)
                    .Take(limit)
                    .ToListAsync(ct);

                // ✅ ÉTAPE 2: Traitement en C#
                var topAgents = agentStats.Select(stat => new TopAgentPercepteurDto
                {
                    AgentId = stat.AgentId,
                    NomAgent = stat.NomAgent,
                    MontantTotal = stat.MontantTotal,
                    NombreTransactions = stat.NombreTransactions,
                    MontantMoyen = stat.NombreTransactions > 0 ? stat.MontantTotal / stat.NombreTransactions : 0,
                    TauxSucces = stat.NombreTransactions > 0 ? (decimal)stat.TransactionsReussies / stat.NombreTransactions * 100 : 0,
                    MontantCommissions = 0,
                    NetAPercevoir = stat.MontantTotal * 0.73m,
                    Progression = 0, // Simplifié pour l'instant
                    DerniereTransaction = stat.DerniereTransaction
                }).ToList();

                // ✅ ÉTAPE 3: Calcul des rangs
                for (int i = 0; i < topAgents.Count; i++)
                {
                    topAgents[i].Rang = i + 1;
                }

                return topAgents;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des top agents");
                throw;
            }
        }

        public async Task<List<TransactionTypeDto>> GetTransactionsParTypeAsync(CancellationToken ct = default)
        {
            try
            {
                var types = new List<TransactionTypeDto>();

                // Transactions de collecte
                var collectes = await _db.Collectes.CountAsync(ct);
                var montantCollectes = await _db.Collectes.SumAsync(c => c.MontantDevisePrincipale ?? c.Montant, ct);
                types.Add(new TransactionTypeDto
                {
                    Type = "Collecte",
                    Montant = montantCollectes,
                    NombreTransactions = collectes,
                    Pourcentage = 100, // Sera recalculé après
                    MontantMoyen = collectes > 0 ? montantCollectes / collectes : 0,
                    TauxSucces = 0 // Simplifié
                });

                // Calcul des pourcentages (simplifié)
                var totalMontant = types.Sum(t => t.Montant);
                foreach (var type in types)
                {
                    type.Pourcentage = totalMontant > 0 ? (type.Montant / totalMontant) * 100 : 0;
                }

                return types;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des transactions par type");
                throw;
            }
        }

        public async Task<List<PaiementModeDto>> GetPaiementsParModeAsync(CancellationToken ct = default)
        {
            try
            {
                var modes = await _db.Collectes
                    .GroupBy(c => c.ModePaiement)
                    .Select(g => new PaiementModeDto
                    {
                        ModePaiement = g.Key ?? "Inconnu",
                        Montant = g.Sum(c => c.MontantDevisePrincipale ?? c.Montant),
                        NombreTransactions = g.Count(),
                        Pourcentage = 0, // Calculé après
                        Frais = g.Sum(c => c.MontantDevisePrincipale ?? c.Montant) * 0.02m,
                        NetAPercevoir = g.Sum(c => c.MontantDevisePrincipale ?? c.Montant) * 0.73m
                    })
                    .OrderByDescending(m => m.Montant)
                    .ToListAsync(ct);

                // Calcul des pourcentages
                var totalMontant = modes.Sum(m => m.Montant);
                foreach (var mode in modes)
                {
                    mode.Pourcentage = totalMontant > 0 ? (mode.Montant / totalMontant) * 100 : 0;
                }

                return modes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des paiements par mode");
                throw;
            }
        }

        public async Task<List<AgentStatsDto>> GetAgentsStatsAsync(CancellationToken ct = default)
        {
            try
            {
                // ✅ ÉTAPE 1: Requête SQL optimisée (1 seule requête)
                var agentStats = await _db.Agents
                    .Select(a => new
                    {
                        AgentId = a.IdAgent,
                        NomAgent = a.NomComplet ?? "",
                        // ✅ Utilisation de sous-requêtes corrélées (traduisible)
                        MontantTotal = _db.Collectes
                            .Where(c => c.AgentId == a.IdAgent)
                            .Sum(c => (decimal?)(c.MontantDevisePrincipale ?? c.Montant)) ?? 0,
                        NombreTransactions = _db.Collectes
                            .Where(c => c.AgentId == a.IdAgent)
                            .Count(),
                        TransactionsReussies = _db.Collectes
                            .Where(c => c.AgentId == a.IdAgent && c.StatutPaiement != null && CollecteStatutPaiementRegles.ValeursSqlValideEtLegacy.Contains(c.StatutPaiement))
                            .Count(),
                        DerniereTransaction = _db.Collectes
                            .Where(c => c.AgentId == a.IdAgent)
                            .Max(c => (DateTime?)c.DateCollecte)
                    })
                    .Where(a => a.MontantTotal > 0) // ✅ Filtrer les agents avec des transactions
                    .OrderByDescending(a => a.MontantTotal)
                    .ToListAsync(ct);

                // ✅ ÉTAPE 2: Traitement en C#
                var stats = agentStats.Select(stat => new AgentStatsDto
                {
                    AgentId = stat.AgentId,
                    NomAgent = stat.NomAgent,
                    MontantTotal = stat.MontantTotal,
                    NombreTransactions = stat.NombreTransactions,
                    MontantMoyen = stat.NombreTransactions > 0 ? stat.MontantTotal / stat.NombreTransactions : 0,
                    TauxSucces = stat.NombreTransactions > 0 ? (decimal)stat.TransactionsReussies / stat.NombreTransactions * 100 : 0,
                    MontantCommissions = 0,
                    MontantFrais = stat.MontantTotal * 0.02m,
                    NetAPercevoir = stat.MontantTotal * 0.73m,
                    Progression = 0, // Simplifié pour l'instant
                    DerniereTransaction = stat.DerniereTransaction,
                    NombreJoursActifs = 0, // Simplifié pour l'instant
                    PerformanceJournaliere = stat.NombreTransactions > 0 ? stat.MontantTotal / 30 : 0 // Approximation
                }).ToList();

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des statistiques des agents");
                throw;
            }
        }

        public async Task<List<TendanceTransactionDto>> GetTendancesTransactionsAsync(int jours = 30, CancellationToken ct = default)
        {
            try
            {
                var dateFin = DateTime.Now;
                var dateDebut = dateFin.AddDays(-jours);

                // ✅ ÉTAPE 1: Requête SQL pure (traduisible)
                var rawData = await _db.Collectes
                    .Where(c => c.DateCollecte >= dateDebut && c.DateCollecte <= dateFin)
                    .GroupBy(c => c.DateCollecte.Date)
                    .Select(g => new
                    {
                        Date = g.Key,  // ✅ DateTime pur
                        MontantTotal = g.Sum(c => c.MontantDevisePrincipale ?? c.Montant),
                        NombreTransactions = g.Count(),
                        TransactionsReussies = g.Count(c => c.StatutPaiement != null && CollecteStatutPaiementRegles.ValeursSqlValideEtLegacy.Contains(c.StatutPaiement))
                    })
                    .OrderBy(x => x.Date)
                    .ToListAsync(ct);

                // ✅ ÉTAPE 2: Traitement en C# (non traduit)
                var tendances = rawData.Select(g => new TendanceTransactionDto
                {
                    Periode = g.Date.ToString("yyyy-MM-dd"),  // ✅ Maintenant autorisé
                    MontantTotal = g.MontantTotal,
                    NombreTransactions = g.NombreTransactions,
                    MontantMoyen = g.NombreTransactions > 0 ? g.MontantTotal / g.NombreTransactions : 0,
                    MontantCommissions = 0,
                    MontantFrais = g.MontantTotal * 0.02m,        // ✅ Maintenant autorisé
                    NetAPercevoir = g.MontantTotal * 0.73m       // ✅ Maintenant autorisé
                }).ToList();

                // ✅ ÉTAPE 3: Calcul des taux finaux
                for (int i = 1; i < tendances.Count; i++)
                {
                    var actuel = tendances[i];
                    var precedent = tendances[i - 1];
                    var rawDataActuel = rawData[i];

                    // Calcul taux de croissance
                    actuel.TauxCroissance = precedent.MontantTotal > 0 ? 
                        ((actuel.MontantTotal - precedent.MontantTotal) / precedent.MontantTotal) * 100 : 0;
                    
                    // ✅ Calcul taux de succès corrigé
                    actuel.TauxSucces = rawDataActuel.NombreTransactions > 0 ? 
                        (decimal)rawDataActuel.TransactionsReussies / rawDataActuel.NombreTransactions * 100 : 0;
                }

                // ✅ Gérer le premier élément (pas de croissance)
                if (tendances.Count > 0)
                {
                    tendances[0].TauxCroissance = 0;
                    tendances[0].TauxSucces = rawData[0].NombreTransactions > 0 ? 
                        (decimal)rawData[0].TransactionsReussies / rawData[0].NombreTransactions * 100 : 0;
                }

                return tendances;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des tendances des transactions");
                throw;
            }
        }

        public async Task<List<ObjectifPercepteurDto>> GetObjectifsPercepteurAsync(CancellationToken ct = default)
        {
            try
            {
                var objectifs = new List<ObjectifPercepteurDto>();

                // Objectif journalier
                var montantJournalier = await GetMontantPerçuAsync(DateTime.Today, DateTime.Now, ct);
                var objectifJournalier = 100000m; // Exemple
                objectifs.Add(new ObjectifPercepteurDto
                {
                    TypeObjectif = "Journalier",
                    Objectif = objectifJournalier,
                    Realise = montantJournalier,
                    Atteinte = objectifJournalier > 0 ? (montantJournalier / objectifJournalier) * 100 : 0,
                    Restant = Math.Max(0, objectifJournalier - montantJournalier),
                    Periode = "Jour",
                    ProgressionPrecedente = 0, // Simplifié
                    DateLimite = DateTime.Today.AddDays(1),
                    EstAtteint = montantJournalier >= objectifJournalier
                });

                // Objectif mensuel
                var montantMensuel = await GetMontantPerçuAsync(DateTime.Today.AddDays(-30), DateTime.Now, ct);
                var objectifMensuel = 3000000m; // Exemple
                objectifs.Add(new ObjectifPercepteurDto
                {
                    TypeObjectif = "Mensuel",
                    Objectif = objectifMensuel,
                    Realise = montantMensuel,
                    Atteinte = objectifMensuel > 0 ? (montantMensuel / objectifMensuel) * 100 : 0,
                    Restant = Math.Max(0, objectifMensuel - montantMensuel),
                    Periode = "Mois",
                    ProgressionPrecedente = 0, // Simplifié
                    DateLimite = DateTime.Today.AddMonths(1),
                    EstAtteint = montantMensuel >= objectifMensuel
                });

                return objectifs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des objectifs du percepteur");
                throw;
            }
        }

        public async Task<List<ResumeFraisDto>> GetResumeFraisAsync(CancellationToken ct = default)
        {
            try
            {
                var frais = new List<ResumeFraisDto>();

                // Frais de transaction (2%)
                var totalFrais = await _db.Collectes.SumAsync(c => c.MontantDevisePrincipale ?? c.Montant, ct) * 0.02m;
                frais.Add(new ResumeFraisDto
                {
                    TypeFrais = "Frais de transaction",
                    MontantTotal = totalFrais,
                    Pourcentage = 100, // Simplifié
                    NombreTransactions = await _db.Collectes.CountAsync(ct),
                    MontantMoyen = totalFrais / await _db.Collectes.CountAsync(ct),
                    Croissance = 0 // Simplifié
                });

                return frais;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du résumé des frais");
                throw;
            }
        }

        public async Task<DashboardPercepteurDto> GetDashboardPercepteurAsync(CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Récupération du dashboard percepteur complet");

                var dashboard = new DashboardPercepteurDto
                {
                    Kpis = await GetKpisPercepteurAsync(ct),
                    Graphs = await GetGraphsPercepteurAsync(ct),
                    TopAgents = await GetTopAgentsPerformanceAsync(10, ct),
                    AgentsStats = await GetAgentsStatsAsync(ct),
                    Objectifs = await GetObjectifsPercepteurAsync(ct),
                    TransactionsRecentes = await GetTransactionsAsync(20, ct),
                    DerniereMiseAJour = DateTime.Now,
                    SoldeAPercevoir = await GetSoldeAPercevoirAsync(ct),
                    MontantEnAttente = await GetMontantEnAttenteAsync(ct),
                    TransactionsEnAttente = await GetTransactionsEnAttenteAsync(ct),
                    RapportPerception = await GetRapportPerceptionSyntheseAsync(ct: ct)
                };

                _logger.LogInformation("Dashboard percepteur récupéré avec succès");
                return dashboard;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du dashboard percepteur");
                throw;
            }
        }

        // Méthodes utilitaires privées
        public async Task<decimal> GetMontantPerçuAsync(DateTime debut, DateTime fin, CancellationToken ct = default)
        {
            var collectes = await _db.Collectes
                .AsNoTracking()
                .Where(c => c.DateCollecte >= debut && c.DateCollecte <= fin)
                .ToListAsync(ct);
            return DashboardDeviseConsolidation.SommerCollectesEnDevisePrincipale(collectes);
        }

        public async Task<int> GetNombreTransactionsAsync(DateTime debut, DateTime fin, CancellationToken ct = default)
        {
            return await _db.Collectes
                .Where(c => c.DateCollecte >= debut && c.DateCollecte <= fin)
                .CountAsync(ct);
        }

        public async Task<decimal> GetMontantMoyenAsync(DateTime debut, DateTime fin, CancellationToken ct = default)
        {
            var transactions = await _db.Collectes
                .Where(c => c.DateCollecte >= debut && c.DateCollecte <= fin)
                .ToListAsync(ct);
            
            return transactions.Any()
                ? DashboardDeviseConsolidation.SommerCollectesEnDevisePrincipale(transactions) / transactions.Count
                : 0;
        }

        public async Task<decimal> GetTauxSuccesAsync(DateTime debut, DateTime fin, CancellationToken ct = default)
        {
            var total = await _db.Collectes
                .Where(c => c.DateCollecte >= debut && c.DateCollecte <= fin)
                .CountAsync(ct);
            
            var reussies = await _db.Collectes
                .Where(c => c.DateCollecte >= debut && c.DateCollecte <= fin && c.StatutPaiement != null && CollecteStatutPaiementRegles.ValeursSqlValideEtLegacy.Contains(c.StatutPaiement))
                .CountAsync(ct);
            
            return total > 0 ? (decimal)reussies / total * 100 : 0;
        }

        public async Task<decimal> GetMontantCommissionsAsync(DateTime debut, DateTime fin, CancellationToken ct = default)
        {
            var devisePrincipale = await GetDevisePrincipaleAsync(ct);
            var mouvements = await _db.WalletMouvements
                .AsNoTracking()
                .Where(m => m.Source == CommissionCollecteSource
                    && m.DateOperation >= debut
                    && m.DateOperation <= fin)
                .Select(m => new { m.Montant, m.DeviseId, m.DateOperation })
                .ToListAsync(ct);

            if (devisePrincipale.Id == null)
                return mouvements.Sum(m => m.Montant);

            return await DashboardDeviseConsolidation.SommerMouvementsEnDevisePrincipaleAsync(
                _deviseConversion,
                mouvements.Select(m => (m.Montant, m.DeviseId, m.DateOperation)),
                devisePrincipale.Id.Value,
                ct);
        }

        public async Task<decimal> GetMontantFraisAsync(DateTime debut, DateTime fin, CancellationToken ct = default)
        {
            var collectes = await GetMontantPerçuAsync(debut, fin, ct);
            return collectes * 0.02m;
        }

        public async Task<decimal> GetNetAPercevoirAsync(DateTime debut, DateTime fin, CancellationToken ct = default)
        {
            var collectes = await GetMontantPerçuAsync(debut, fin, ct);
            return collectes * 0.73m;
        }

        public async Task<List<PercepteurTransactionDto>> GetTransactionsParStatutAsync(string statut, CancellationToken ct = default)
        {
            return await _db.Collectes
                .Include(c => c.Agent)
                .Include(c => c.Affilie)
                .Where(c => c.StatutPaiement == statut)
                .OrderByDescending(c => c.DateCollecte)
                .Take(50)
                .Select(c => new PercepteurTransactionDto
                {
                    IdTransaction = c.IdCollecte,
                    DateTransaction = c.DateCollecte,
                    Montant = c.MontantDevisePrincipale ?? c.Montant,
                    TypeTransaction = "Collecte",
                    Statut = c.StatutPaiement ?? "",
                    Reference = c.ReferencePaiement ?? "",
                    NomAgent = c.Agent.NomComplet ?? "",
                    NomAffilie = c.Affilie.Nom ?? "",
                    ModePaiement = c.ModePaiement ?? "",
                    Commission = 0,
                    Frais = (c.MontantDevisePrincipale ?? c.Montant) * 0.02m,
                    NetAPercevoir = (c.MontantDevisePrincipale ?? c.Montant) * 0.73m,
                    Notes = c.Observation
                })
                .ToListAsync(ct);
        }

        public async Task<List<PercepteurTransactionDto>> GetTransactionsParAgentAsync(int agentId, CancellationToken ct = default)
        {
            return await _db.Collectes
                .Include(c => c.Agent)
                .Include(c => c.Affilie)
                .Where(c => c.AgentId == agentId)
                .OrderByDescending(c => c.DateCollecte)
                .Take(50)
                .Select(c => new PercepteurTransactionDto
                {
                    IdTransaction = c.IdCollecte,
                    DateTransaction = c.DateCollecte,
                    Montant = c.MontantDevisePrincipale ?? c.Montant,
                    TypeTransaction = "Collecte",
                    Statut = c.StatutPaiement ?? "",
                    Reference = c.ReferencePaiement ?? "",
                    NomAgent = c.Agent.NomComplet ?? "",
                    NomAffilie = c.Affilie.Nom ?? "",
                    ModePaiement = c.ModePaiement ?? "",
                    Commission = 0,
                    Frais = (c.MontantDevisePrincipale ?? c.Montant) * 0.02m,
                    NetAPercevoir = (c.MontantDevisePrincipale ?? c.Montant) * 0.73m,
                    Notes = c.Observation
                })
                .ToListAsync(ct);
        }

        public async Task<AgentStatsDto> GetPerformanceAgentAsync(int agentId, CancellationToken ct = default)
        {
            var devisePrincipale = await GetDevisePrincipaleAsync(ct);
            var collectesAgent = await _db.Collectes
                .AsNoTracking()
                .Where(c => c.AgentId == agentId)
                .ToListAsync(ct);

            var montantTotal = DashboardDeviseConsolidation.SommerCollectesEnDevisePrincipale(collectesAgent);
            var nombreTransactions = collectesAgent.Count;

            var mouvements = await (
                from m in _db.WalletMouvements.AsNoTracking()
                join w in _db.WalletsAgents.AsNoTracking() on m.WalletId equals w.IdWalletAgent
                where m.Source == CommissionCollecteSource && w.AgentId == agentId
                select new { m.Montant, m.DeviseId, m.DateOperation }
            ).ToListAsync(ct);

            decimal montantCommissions = 0;
            foreach (var mouvement in mouvements)
            {
                montantCommissions += await DashboardDeviseConsolidation.MontantMouvementEnDevisePrincipaleAsync(
                    _deviseConversion,
                    mouvement.Montant,
                    mouvement.DeviseId,
                    devisePrincipale.Id,
                    mouvement.DateOperation,
                    ct);
            }

            var agent = await _db.Agents.AsNoTracking()
                .Where(a => a.IdAgent == agentId)
                .Select(a => new { a.NomComplet })
                .FirstOrDefaultAsync(ct);

            if (agent == null)
                return new AgentStatsDto();

            var transactionsReussies = collectesAgent.Count(c => c.StatutPaiement != null && CollecteStatutPaiementRegles.ValeursSqlValideEtLegacy.Contains(c.StatutPaiement));
            var derniereTransaction = collectesAgent.Count > 0
                ? collectesAgent.Max(c => c.DateCollecte)
                : (DateTime?)null;

            var stats = new AgentStatsDto
            {
                AgentId = agentId,
                NomAgent = agent.NomComplet ?? "",
                MontantTotal = montantTotal,
                NombreTransactions = nombreTransactions,
                MontantCommissions = montantCommissions,
                MontantFrais = montantTotal * 0.02m,
                NetAPercevoir = montantTotal * 0.73m,
                DerniereTransaction = derniereTransaction,
                MontantMoyen = nombreTransactions > 0 ? montantTotal / nombreTransactions : 0,
                TauxSucces = nombreTransactions > 0
                    ? (decimal)transactionsReussies / nombreTransactions * 100
                    : 0
            };

            return stats;
        }

        public async Task<List<TendanceTransactionDto>> GetEvolutionTransactionsAsync(int mois = 12, CancellationToken ct = default)
        {
            return await GetTendancesTransactionsAsync(mois * 30, ct);
        }

        public async Task<List<PerformanceJournaliereDto>> GetResumeJournalierAsync(DateTime debut, DateTime fin, CancellationToken ct = default)
        {
            try
            {
                // ✅ ÉTAPE 1: Requête SQL pure (traduisible)
                var rawData = await _db.Collectes
                    .Where(c => c.DateCollecte >= debut && c.DateCollecte <= fin)
                    .GroupBy(c => c.DateCollecte.Date)
                    .Select(g => new
                    {
                        Date = g.Key,  // ✅ DateTime pur
                        MontantTotal = g.Sum(c => c.MontantDevisePrincipale ?? c.Montant),
                        NombreTransactions = g.Count(),
                        TransactionsReussies = g.Count(c => c.StatutPaiement != null && CollecteStatutPaiementRegles.ValeursSqlValideEtLegacy.Contains(c.StatutPaiement)),
                        TransactionsEchouees = g.Count(c => c.StatutPaiement == CollecteStatutPaiement.EnAttente)
                    })
                    .OrderBy(x => x.Date)
                    .ToListAsync(ct);

                // ✅ ÉTAPE 2: Traitement en C# (non traduit)
                var performances = rawData.Select(g => new PerformanceJournaliereDto
                {
                    Date = g.Date.ToString("yyyy-MM-dd"),  // ✅ Maintenant autorisé
                    MontantTotal = g.MontantTotal,
                    NombreTransactions = g.NombreTransactions,
                    MontantMoyen = g.NombreTransactions > 0 ? g.MontantTotal / g.NombreTransactions : 0,
                    ObjectifJournalier = 100000m,
                    TransactionsReussies = g.TransactionsReussies,
                    TransactionsEchouees = g.TransactionsEchouees,
                    MontantCommissions = 0,
                    MontantFrais = g.MontantTotal * 0.02m        // ✅ Maintenant autorisé
                }).ToList();

                // ✅ ÉTAPE 3: Calcul des taux finaux
                foreach (var perf in performances)
                {
                    perf.TauxSucces = perf.NombreTransactions > 0 ? 
                        (decimal)perf.TransactionsReussies / perf.NombreTransactions * 100 : 0;
                    perf.AtteinteObjectif = perf.ObjectifJournalier > 0 ? 
                        (perf.MontantTotal / perf.ObjectifJournalier) * 100 : 0;
                }

                return performances;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du résumé journalier");
                throw;
            }
        }

        public async Task<decimal> GetSoldeAPercevoirAsync(CancellationToken ct = default)
        {
            var (montant, _) = await _perceptionVirtuelleService.GetTotauxVirtuelsEnAttenteAsync(ct);
            return montant;
        }

        public async Task<decimal> GetMontantEnAttenteAsync(CancellationToken ct = default)
        {
            var (montant, _) = await _perceptionVirtuelleService.GetTotauxVirtuelsEnAttenteAsync(ct);
            return montant;
        }

        public async Task<int> GetTransactionsEnAttenteAsync(CancellationToken ct = default)
        {
            var (_, nombre) = await _perceptionVirtuelleService.GetTotauxVirtuelsEnAttenteAsync(ct);
            return nombre;
        }

        public async Task<PerceptionRapportSyntheseDto> GetRapportPerceptionSyntheseAsync(
            DateTime? dateDebut = null,
            DateTime? dateFin = null,
            string? origine = null,
            string? statut = null,
            int? agentId = null,
            int? affilieId = null,
            CancellationToken ct = default)
        {
            var lignes = await BuildRapportLignesAsync(
                dateDebut, dateFin, origine, statut, agentId, affilieId, ct);
            var (_, deviseCode) = await GetDevisePrincipaleAsync(ct);
            return BuildSynthese(lignes, deviseCode);
        }

        public async Task<PerceptionRapportResponseDto> GetRapportPerceptionAsync(
            DateTime? dateDebut = null,
            DateTime? dateFin = null,
            string? origine = null,
            string? statut = null,
            int? agentId = null,
            int? affilieId = null,
            PaginationRequest? pagination = null,
            CancellationToken ct = default)
        {
            var lignes = await BuildRapportLignesAsync(
                dateDebut, dateFin, origine, statut, agentId, affilieId, ct);
            var (_, deviseCode) = await GetDevisePrincipaleAsync(ct);

            return new PerceptionRapportResponseDto
            {
                Synthese = BuildSynthese(lignes, deviseCode),
                Lignes = PaginateRapportLignes(lignes, pagination ?? new PaginationRequest())
            };
        }

        private async Task<List<PerceptionRapportLigneDto>> BuildRapportLignesAsync(
            DateTime? dateDebut,
            DateTime? dateFin,
            string? origine,
            string? statut,
            int? agentId,
            int? affilieId,
            CancellationToken ct)
        {
            var origineFiltre = PerceptionOrigineHelper.NormalizeOrigineFiltre(origine);
            var statutFiltre = PerceptionOrigineHelper.NormalizeStatutFiltre(statut);
            var agentParAffilie = await LoadAgentParAffilieAsync(ct);
            var debitParCollecte = await LoadDebitVirtuelParCollecteAsync(ct);
            var collecteIdsAvecDebit = debitParCollecte.Keys.ToHashSet();

            var query = _db.Collectes
                .AsNoTracking()
                .Include(c => c.Agent)
                .Include(c => c.Affilie)
                .Include(c => c.Devise)
                .Include(c => c.PercepteurUtilisateur)
                .Include(c => c.PerceptionVirtuelle)
                    .ThenInclude(p => p!.PercepteurUtilisateur)
                .Where(c => c.Statut
                    && c.ModePaiement != null
                    && CollecteStatutPaiementRegles.ValeursSqlValideEtLegacy.Contains(c.StatutPaiement!));

            if (dateDebut.HasValue)
                query = query.Where(c => c.DateCollecte >= dateDebut.Value);
            if (dateFin.HasValue)
                query = query.Where(c => c.DateCollecte <= dateFin.Value);
            if (affilieId.HasValue)
                query = query.Where(c => c.AffilieId == affilieId.Value);

            var collectes = await query
                .OrderByDescending(c => c.DateCollecte)
                .ToListAsync(ct);

            var lignes = new List<PerceptionRapportLigneDto>();

            foreach (var collecte in collectes)
            {
                var hasDebit = collecteIdsAvecDebit.Contains(collecte.IdCollecte);
                var origineCollecte = PerceptionOrigineHelper.ResolveOrigine(collecte, hasDebit);
                if (origineCollecte == null)
                    continue;

                var isOrigineAgent = origineCollecte == PerceptionOrigineHelper.OrigineAgent;
                var statutCollecte = PerceptionOrigineHelper.ResolveStatutPerception(collecte, isOrigineAgent);

                if (!PerceptionOrigineHelper.MatchesOrigineFiltre(origineCollecte, origineFiltre))
                    continue;
                if (!PerceptionOrigineHelper.MatchesStatutFiltre(statutCollecte, statutFiltre))
                    continue;

                var agentEffectif = isOrigineAgent
                    ? PerceptionVirtuelleService.ResolveAgentIdEffectif(collecte, agentParAffilie)
                    : collecte.AgentId ?? 0;

                if (agentId.HasValue && isOrigineAgent && agentEffectif != agentId.Value)
                    continue;

                lignes.Add(MapRapportLigne(collecte, origineCollecte, statutCollecte, agentEffectif, debitParCollecte));
            }

            return lignes;
        }

        private static PerceptionRapportSyntheseDto BuildSynthese(
            IReadOnlyList<PerceptionRapportLigneDto> lignes,
            string? deviseCode)
        {
            var agentLignes = lignes
                .Where(l => l.OriginePerception == PerceptionOrigineHelper.OrigineAgent)
                .ToList();
            var affilieLignes = lignes
                .Where(l => l.OriginePerception == PerceptionOrigineHelper.OrigineAffilie)
                .ToList();

            var agentEnAttente = agentLignes
                .Where(l => l.StatutPerception == PerceptionOrigineHelper.StatutEnAttente)
                .ToList();
            var agentPercu = agentLignes
                .Where(l => l.StatutPerception == PerceptionOrigineHelper.StatutPercu)
                .ToList();

            return new PerceptionRapportSyntheseDto
            {
                DeviseCode = deviseCode,
                Agent = new PerceptionRapportCanalDto
                {
                    MontantEnAttente = agentEnAttente.Sum(l => l.MontantDevisePrincipale),
                    NombreEnAttente = agentEnAttente.Count,
                    MontantPerçu = agentPercu.Sum(l => l.MontantDevisePrincipale),
                    NombrePerçu = agentPercu.Count
                },
                Affilie = new PerceptionRapportCanalDto
                {
                    MontantPerçu = affilieLignes.Sum(l => l.MontantDevisePrincipale),
                    NombrePerçu = affilieLignes.Count
                },
                TotalPerçu = agentPercu.Sum(l => l.MontantDevisePrincipale)
                    + affilieLignes.Sum(l => l.MontantDevisePrincipale)
            };
        }

        private static PerceptionRapportLigneDto MapRapportLigne(
            Collecte collecte,
            string origineCollecte,
            string statutCollecte,
            int agentEffectif,
            IReadOnlyDictionary<int, int> debitParCollecte)
        {
            var montantPrincipal = collecte.MontantDevisePrincipale ?? collecte.Montant;
            debitParCollecte.TryGetValue(collecte.IdCollecte, out var mouvementId);
            return new PerceptionRapportLigneDto
            {
                OriginePerception = origineCollecte,
                StatutPerception = statutCollecte,
                IdCollecte = collecte.IdCollecte,
                Montant = collecte.Montant,
                MontantDevisePrincipale = montantPrincipal,
                DeviseCode = collecte.Devise?.Code,
                AffilieId = collecte.AffilieId,
                AffilieNom = collecte.Affilie?.Nom,
                AgentId = agentEffectif > 0 ? agentEffectif : collecte.AgentId,
                AgentNom = collecte.Agent?.NomComplet,
                AgentMatricule = collecte.Agent?.Matricule,
                ModePaiement = collecte.ModePaiement,
                DateCollecte = collecte.DateCollecte,
                DatePerception = collecte.DatePerception,
                PerceptionVirtuelleId = collecte.PerceptionVirtuelleId,
                WalletVirtuelMouvementId = mouvementId > 0 ? mouvementId : null,
                PercepteurNom = collecte.PercepteurUtilisateur?.NomUtilisateur
                    ?? collecte.PerceptionVirtuelle?.PercepteurUtilisateur?.NomUtilisateur,
                ReferencePaiement = collecte.ReferencePaiement,
                Observation = collecte.Observation ?? collecte.PerceptionVirtuelle?.Observation
            };
        }

        private static PaginatedResponse<PerceptionRapportLigneDto> PaginateRapportLignes(
            List<PerceptionRapportLigneDto> items,
            PaginationRequest pagination)
        {
            var page = pagination.Page <= 0 ? 1 : pagination.Page;
            var pageSize = pagination.PageSize <= 0 ? 20 : pagination.PageSize;
            var total = items.Count;
            var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);
            var data = items.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return new PaginatedResponse<PerceptionRapportLigneDto>
            {
                Data = data,
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = total,
                TotalPages = totalPages,
                HasNextPage = page < totalPages,
                HasPreviousPage = page > 1
            };
        }

        private async Task<Dictionary<int, int?>> LoadAgentParAffilieAsync(CancellationToken ct) =>
            await _db.Adhesions
                .AsNoTracking()
                .Where(a => a.Statut)
                .ToDictionaryAsync(a => a.AffilieId, a => a.AgentId, ct);

        private async Task<Dictionary<int, int>> LoadDebitVirtuelParCollecteAsync(CancellationToken ct)
        {
            var mouvements = await _db.WalletVirtuelMouvements
                .AsNoTracking()
                .Where(m => m.TypeOperation == "DEBIT"
                    && m.Source == WalletVirtuelMouvementSources.CollecteCompteVirtuel
                    && m.ReferenceExterne != null
                    && m.Statut)
                .Select(m => new { CollecteId = m.ReferenceExterne!.Value, m.IdWalletVirtuelMouvement })
                .ToListAsync(ct);

            return mouvements
                .GroupBy(m => m.CollecteId)
                .ToDictionary(g => g.Key, g => g.First().IdWalletVirtuelMouvement);
        }

        private async Task<HashSet<int>> LoadCollecteIdsAvecDebitVirtuelAsync(CancellationToken ct)
        {
            var map = await LoadDebitVirtuelParCollecteAsync(ct);
            return map.Keys.ToHashSet();
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
    }
}
