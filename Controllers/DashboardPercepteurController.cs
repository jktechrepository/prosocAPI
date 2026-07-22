using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Prosoc.Utilities;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.Pagination;
using ProsocAPI.Services.Repositories;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Percepteur,Financier")]
    public class DashboardPercepteurController : ControllerBase
    {
        private readonly IDashboardPercepteurRepository _dashboardService;
        private readonly IDashboardCaissierRepository _caissierHistoriqueService;
        private readonly ILogger<DashboardPercepteurController> _logger;

        public DashboardPercepteurController(
            IDashboardPercepteurRepository dashboardService,
            IDashboardCaissierRepository caissierHistoriqueService,
            ILogger<DashboardPercepteurController> logger)
        {
            _dashboardService = dashboardService;
            _caissierHistoriqueService = caissierHistoriqueService;
            _logger = logger;
        }

        /// <summary>
        /// Récupère les KPIs du percepteur
        /// </summary>
        [HttpGet("kpis")]
        public async Task<ActionResult<PercepteurKpisDto>> GetKpisPercepteur(CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération des KPIs du percepteur");

                var kpis = await _dashboardService.GetKpisPercepteurAsync(ct);
                
                _logger.LogInformation("KPIs du percepteur récupérés avec succès");
                return Ok(kpis);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des KPIs du percepteur");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des KPIs du percepteur", ex);
            }
        }

        /// <summary>
        /// Récupère les graphiques du percepteur
        /// </summary>
        /*
        [HttpGet("graphs")]
        public async Task<ActionResult<PercepteurGraphsDto>> GetGraphsPercepteur(CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération des graphiques du percepteur");

                var graphs = await _dashboardService.GetGraphsPercepteurAsync(ct);
                
                _logger.LogInformation("Graphiques du percepteur récupérés avec succès");
                return Ok(graphs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des graphiques du percepteur");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des graphiques du percepteur", ex);
            }
        }
        */

        /// <summary>
        /// Récupère les transactions du percepteur
        /// </summary>
        /// <remarks>
        /// Vue **globale** (toutes les collectes récentes du système) — ne pas utiliser comme « mes encaissements guichet ».
        /// Pour l'historique personnel guichet : <c>GET mes-collectes-guichet</c>.
        /// Pour les perceptions VA : <c>GET /api/PerceptionVirtuelle/historique</c>.
        /// </remarks>
        [HttpGet("transactions")]
        public async Task<ActionResult<List<PercepteurTransactionDto>>> GetTransactions([FromQuery] int limit, CancellationToken ct)
        {
            try
            {
                // Par défaut, limit = 50
                if (limit == 0) limit = 50;
                
                _logger.LogInformation("Récupération des {Limit} transactions", limit);

                var transactions = await _dashboardService.GetTransactionsAsync(limit, ct);
                
                _logger.LogInformation("Transactions récupérées: {Count} transactions", transactions.Count);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des transactions");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des transactions", ex);
            }
        }

        /// <summary>
        /// Historique paginé des collectes guichet saisies par le percepteur connecté (OperateurUtilisateurId).
        /// </summary>
        [HttpGet("mes-collectes-guichet")]
        public async Task<ActionResult<PaginatedResponse<CaissierCollecteDto>>> GetMesCollectesGuichet(
            [FromQuery] DateTime? dateDebut,
            [FromQuery] DateTime? dateFin,
            [FromQuery] string? modePaiement,
            [FromQuery] PaginationRequest pagination,
            CancellationToken ct)
        {
            try
            {
                var utilisateurId = CurrentUserResolver.GetCurrentUtilisateurId(User);
                var filtres = new GuichetCollecteHistoriqueFiltreDto
                {
                    DateDebut = dateDebut,
                    DateFin = dateFin,
                    ModePaiement = modePaiement
                };
                return Ok(await _caissierHistoriqueService.GetCollectesHistoriqueAsync(
                    utilisateurId, filtres, pagination, ct));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur historique collectes guichet percepteur");
                return this.TechnicalErrorResponse(
                    "Erreur lors de la récupération de l'historique des collectes guichet", ex);
            }
        }

        /// <summary>
        /// Récupère les performances journalières
        /// </summary>
        [HttpGet("performances-journalieres")]
        public async Task<ActionResult<List<PerformanceJournaliereDto>>> GetPerformancesJournalieres([FromQuery] int jours, CancellationToken ct)
        {
            try
            {
                // Par défaut, jours = 30
                if (jours == 0) jours = 30;
                
                _logger.LogInformation("Récupération des performances sur {Jours} jours", jours);

                var performances = await _dashboardService.GetPerformancesJournalieresAsync(jours, ct);
                
                _logger.LogInformation("Performances journalières récupérées: {Count} jours", performances.Count);
                return Ok(performances);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des performances journalières");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des performances journalières", ex);
            }
        }

        /// <summary>
        /// Récupère le résumé mensuel
        /// </summary>
        [HttpGet("resume-mensuel")]
        public async Task<ActionResult<List<ResumeMensuelDto>>> GetResumeMensuels([FromQuery] int mois, CancellationToken ct)
        {
            try
            {
                // Par défaut, mois = 12
                if (mois == 0) mois = 12;
                
                _logger.LogInformation("Récupération du résumé sur {Mois} mois", mois);

                var resumes = await _dashboardService.GetResumeMensuelsAsync(mois, ct);
                
                _logger.LogInformation("Résumés mensuels récupérés: {Count} mois", resumes.Count);
                return Ok(resumes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du résumé mensuel");
                return this.TechnicalErrorResponse("Erreur lors de la récupération du résumé mensuel", ex);
            }
        }

        /// <summary>
        /// Récupère le top des agents par performance
        /// </summary>
        [HttpGet("top-agents")]
        public async Task<ActionResult<List<TopAgentPercepteurDto>>> GetTopAgentsPerformance([FromQuery] int limit, CancellationToken ct)
        {
            try
            {
                // Par défaut, limit = 10
                if (limit == 0) limit = 10;
                
                _logger.LogInformation("Récupération du top {Limit} agents par performance", limit);

                var topAgents = await _dashboardService.GetTopAgentsPerformanceAsync(limit, ct);
                
                _logger.LogInformation("Top agents récupérés: {Count} agents", topAgents.Count);
                return Ok(topAgents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du top agents");
                return this.TechnicalErrorResponse("Erreur lors de la récupération du top agents", ex);
            }
        }

        /// <summary>
        /// Récupère les transactions par type
        /// </summary>
        [HttpGet("transactions-type")]
        public async Task<ActionResult<List<TransactionTypeDto>>> GetTransactionsParType(CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération des transactions par type");

                var types = await _dashboardService.GetTransactionsParTypeAsync(ct);
                
                _logger.LogInformation("Transactions par type récupérées: {Count} types", types.Count);
                return Ok(types);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des transactions par type");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des transactions par type", ex);
            }
        }

        /// <summary>
        /// Récupère les paiements par mode
        /// </summary>
        [HttpGet("paiements-mode")]
        public async Task<ActionResult<List<PaiementModeDto>>> GetPaiementsParMode(CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération des paiements par mode");

                var modes = await _dashboardService.GetPaiementsParModeAsync(ct);
                
                _logger.LogInformation("Paiements par mode récupérés: {Count} modes", modes.Count);
                return Ok(modes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des paiements par mode");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des paiements par mode", ex);
            }
        }

        /// <summary>
        /// Récupère les statistiques des agents
        /// </summary>
        [HttpGet("agents-stats")]
        public async Task<ActionResult<List<AgentStatsDto>>> GetAgentsStats(CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération des statistiques des agents");

                var stats = await _dashboardService.GetAgentsStatsAsync(ct);
                
                _logger.LogInformation("Statistiques des agents récupérées: {Count} agents", stats.Count);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des statistiques des agents");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des statistiques des agents", ex);
            }
        }

        /// <summary>
        /// Récupère les tendances des transactions
        /// </summary>
        [HttpGet("tendances")]
        public async Task<ActionResult<List<TendanceTransactionDto>>> GetTendancesTransactions([FromQuery] int jours, CancellationToken ct)
        {
            try
            {
                // Par défaut, jours = 30
                if (jours == 0) jours = 30;
                
                _logger.LogInformation("Récupération des tendances sur {Jours} jours", jours);

                var tendances = await _dashboardService.GetTendancesTransactionsAsync(jours, ct);
                
                _logger.LogInformation("Tendances des transactions récupérées: {Count} jours", tendances.Count);
                return Ok(tendances);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des tendances des transactions");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des tendances des transactions", ex);
            }
        }

        /// <summary>
        /// Récupère les objectifs du percepteur
        /// </summary>
        [HttpGet("objectifs")]
        public async Task<ActionResult<List<ObjectifPercepteurDto>>> GetObjectifsPercepteur(CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération des objectifs du percepteur");

                var objectifs = await _dashboardService.GetObjectifsPercepteurAsync(ct);
                
                _logger.LogInformation("Objectifs du percepteur récupérés: {Count} objectifs", objectifs.Count);
                return Ok(objectifs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des objectifs du percepteur");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des objectifs du percepteur", ex);
            }
        }

        /// <summary>
        /// Récupère le résumé des frais
        /// </summary>
        [HttpGet("resume-frais")]
        public async Task<ActionResult<List<ResumeFraisDto>>> GetResumeFrais(CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération du résumé des frais");

                var frais = await _dashboardService.GetResumeFraisAsync(ct);
                
                _logger.LogInformation("Résumé des frais récupéré: {Count} types", frais.Count);
                return Ok(frais);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du résumé des frais");
                return this.TechnicalErrorResponse("Erreur lors de la récupération du résumé des frais", ex);
            }
        }

        /// <summary>
        /// Récupère le dashboard percepteur complet
        /// </summary>
        [HttpGet("summary")]
        public async Task<ActionResult<DashboardPercepteurDto>> GetDashboardPercepteur(CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération du dashboard percepteur complet");

                var dashboard = await _dashboardService.GetDashboardPercepteurAsync(ct);
                
                _logger.LogInformation("Dashboard percepteur récupéré avec succès");
                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du dashboard percepteur");
                return this.TechnicalErrorResponse("Erreur lors de la récupération du dashboard percepteur", ex);
            }
        }

        /// <summary>
        /// Récupère le montant perçu sur une période
        /// </summary>
        [HttpGet("montant-percu")]
        public async Task<ActionResult<decimal>> GetMontantPerçu([FromQuery] DateTime debut, [FromQuery] DateTime fin, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération du montant perçu du {Debut} au {Fin}", debut, fin);

                var montant = await _dashboardService.GetMontantPerçuAsync(debut, fin, ct);
                
                _logger.LogInformation("Montant perçu récupéré: {Montant}", montant);
                return Ok(montant);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du montant perçu");
                return this.TechnicalErrorResponse("Erreur lors de la récupération du montant perçu", ex);
            }
        }

        /// <summary>
        /// Récupère le nombre de transactions sur une période
        /// </summary>
        [HttpGet("nombre-transactions")]
        public async Task<ActionResult<int>> GetNombreTransactions([FromQuery] DateTime debut, [FromQuery] DateTime fin, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération du nombre de transactions du {Debut} au {Fin}", debut, fin);

                var nombre = await _dashboardService.GetNombreTransactionsAsync(debut, fin, ct);
                
                _logger.LogInformation("Nombre de transactions récupéré: {Nombre}", nombre);
                return Ok(nombre);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du nombre de transactions");
                return this.TechnicalErrorResponse("Erreur lors de la récupération du nombre de transactions", ex);
            }
        }

        /// <summary>
        /// Récupère le montant moyen des transactions sur une période
        /// </summary>
        [HttpGet("montant-moyen")]
        public async Task<ActionResult<decimal>> GetMontantMoyen([FromQuery] DateTime debut, [FromQuery] DateTime fin, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération du montant moyen du {Debut} au {Fin}", debut, fin);

                var montant = await _dashboardService.GetMontantMoyenAsync(debut, fin, ct);
                
                _logger.LogInformation("Montant moyen récupéré: {Montant}", montant);
                return Ok(montant);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du montant moyen");
                return this.TechnicalErrorResponse("Erreur lors de la récupération du montant moyen", ex);
            }
        }

        /// <summary>
        /// Récupère le taux de succès sur une période
        /// </summary>
        [HttpGet("taux-succes")]
        public async Task<ActionResult<decimal>> GetTauxSucces([FromQuery] DateTime debut, [FromQuery] DateTime fin, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération du taux de succès du {Debut} au {Fin}", debut, fin);

                var taux = await _dashboardService.GetTauxSuccesAsync(debut, fin, ct);
                
                _logger.LogInformation("Taux de succès récupéré: {Taux}", taux);
                return Ok(taux);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du taux de succès");
                return this.TechnicalErrorResponse("Erreur lors de la récupération du taux de succès", ex);
            }
        }

        /// <summary>
        /// Récupère le montant des commissions sur une période
        /// </summary>
        [HttpGet("montant-commissions")]
        public async Task<ActionResult<decimal>> GetMontantCommissions([FromQuery] DateTime debut, [FromQuery] DateTime fin, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération du montant des commissions du {Debut} au {Fin}", debut, fin);

                var montant = await _dashboardService.GetMontantCommissionsAsync(debut, fin, ct);
                
                _logger.LogInformation("Montant des commissions récupéré: {Montant}", montant);
                return Ok(montant);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du montant des commissions");
                return this.TechnicalErrorResponse("Erreur lors de la récupération du montant des commissions", ex);
            }
        }

        /// <summary>
        /// Récupère le montant des frais sur une période
        /// </summary>
        [HttpGet("montant-frais")]
        public async Task<ActionResult<decimal>> GetMontantFrais([FromQuery] DateTime debut, [FromQuery] DateTime fin, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération du montant des frais du {Debut} au {Fin}", debut, fin);

                var montant = await _dashboardService.GetMontantFraisAsync(debut, fin, ct);
                
                _logger.LogInformation("Montant des frais récupéré: {Montant}", montant);
                return Ok(montant);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du montant des frais");
                return this.TechnicalErrorResponse("Erreur lors de la récupération du montant des frais", ex);
            }
        }

        /// <summary>
        /// Récupère le net à percevoir sur une période
        /// </summary>
        [HttpGet("net-a-percevoir")]
        public async Task<ActionResult<decimal>> GetNetAPercevoir([FromQuery] DateTime debut, [FromQuery] DateTime fin, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération du net à percevoir du {Debut} au {Fin}", debut, fin);

                var montant = await _dashboardService.GetNetAPercevoirAsync(debut, fin, ct);
                
                _logger.LogInformation("Net à percevoir récupéré: {Montant}", montant);
                return Ok(montant);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du net à percevoir");
                return this.TechnicalErrorResponse("Erreur lors de la récupération du net à percevoir", ex);
            }
        }

        /// <summary>
        /// Récupère les transactions par statut
        /// </summary>
        [HttpGet("transactions-statut")]
        public async Task<ActionResult<List<PercepteurTransactionDto>>> GetTransactionsParStatut([FromQuery] string statut, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération des transactions avec statut: {Statut}", statut);

                var transactions = await _dashboardService.GetTransactionsParStatutAsync(statut, ct);
                
                _logger.LogInformation("Transactions par statut récupérées: {Count} transactions", transactions.Count);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des transactions par statut");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des transactions par statut", ex);
            }
        }

        /// <summary>
        /// Récupère les transactions par agent
        /// </summary>
        [HttpGet("transactions-agent")]
        public async Task<ActionResult<List<PercepteurTransactionDto>>> GetTransactionsParAgent([FromQuery] int agentId, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération des transactions pour l'agent {AgentId}", agentId);

                var transactions = await _dashboardService.GetTransactionsParAgentAsync(agentId, ct);
                
                _logger.LogInformation("Transactions par agent récupérées: {Count} transactions", transactions.Count);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des transactions par agent");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des transactions par agent", ex);
            }
        }

        /// <summary>
        /// Récupère la performance d'un agent
        /// </summary>
        [HttpGet("performance-agent")]
        public async Task<ActionResult<AgentStatsDto>> GetPerformanceAgent([FromQuery] int agentId, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération de la performance de l'agent {AgentId}", agentId);

                var performance = await _dashboardService.GetPerformanceAgentAsync(agentId, ct);
                
                _logger.LogInformation("Performance de l'agent récupérée avec succès");
                return Ok(performance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de la performance de l'agent");
                return this.TechnicalErrorResponse("Erreur lors de la récupération de la performance de l'agent", ex);
            }
        }

        /// <summary>
        /// Récupère l'évolution des transactions
        /// </summary>
        [HttpGet("evolution-transactions")]
        public async Task<ActionResult<List<TendanceTransactionDto>>> GetEvolutionTransactions([FromQuery] int mois, CancellationToken ct)
        {
            try
            {
                // Par défaut, mois = 12
                if (mois == 0) mois = 12;
                
                _logger.LogInformation("Récupération de l'évolution des transactions sur {Mois} mois", mois);

                var evolution = await _dashboardService.GetEvolutionTransactionsAsync(mois, ct);
                
                _logger.LogInformation("Évolution des transactions récupérée: {Count} mois", evolution.Count);
                return Ok(evolution);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de l'évolution des transactions");
                return this.TechnicalErrorResponse("Erreur lors de la récupération de l'évolution des transactions", ex);
            }
        }

        /// <summary>
        /// Récupère le résumé journalier
        /// </summary>
        [HttpGet("resume-journalier")]
        public async Task<ActionResult<List<PerformanceJournaliereDto>>> GetResumeJournalier([FromQuery] DateTime debut, [FromQuery] DateTime fin, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération du résumé journalier du {Debut} au {Fin}", debut, fin);

                var resume = await _dashboardService.GetResumeJournalierAsync(debut, fin, ct);
                
                _logger.LogInformation("Résumé journalier récupéré: {Count} jours", resume.Count);
                return Ok(resume);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du résumé journalier");
                return this.TechnicalErrorResponse("Erreur lors de la récupération du résumé journalier", ex);
            }
        }

        /// <summary>
        /// Récupère le solde à percevoir
        /// </summary>
        [HttpGet("solde-a-percevoir")]
        public async Task<ActionResult<decimal>> GetSoldeAPercevoir(CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération du solde à percevoir");

                var solde = await _dashboardService.GetSoldeAPercevoirAsync(ct);
                
                _logger.LogInformation("Solde à percevoir récupéré: {Solde}", solde);
                return Ok(solde);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du solde à percevoir");
                return this.TechnicalErrorResponse("Erreur lors de la récupération du solde à percevoir", ex);
            }
        }

        /// <summary>
        /// Récupère le montant en attente
        /// </summary>
        [HttpGet("montant-en-attente")]
        public async Task<ActionResult<decimal>> GetMontantEnAttente(CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération du montant en attente");

                var montant = await _dashboardService.GetMontantEnAttenteAsync(ct);
                
                _logger.LogInformation("Montant en attente récupéré: {Montant}", montant);
                return Ok(montant);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du montant en attente");
                return this.TechnicalErrorResponse("Erreur lors de la récupération du montant en attente", ex);
            }
        }

        /// <summary>
        /// Récupère le nombre de transactions en attente
        /// </summary>
        [HttpGet("transactions-en-attente")]
        public async Task<ActionResult<int>> GetTransactionsEnAttente(CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération du nombre de transactions en attente");

                var nombre = await _dashboardService.GetTransactionsEnAttenteAsync(ct);
                
                _logger.LogInformation("Transactions en attente récupérées: {Nombre}", nombre);
                return Ok(nombre);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du nombre de transactions en attente");
                return this.TechnicalErrorResponse("Erreur lors de la récupération du nombre de transactions en attente", ex);
            }
        }

        /// <summary>
        /// Rapport détaillé perception Agent (VA) vs Affilié (guichet direct) — synthèse + lignes paginées.
        /// </summary>
        [HttpGet("rapport-perception")]
        [Authorize(Roles = "Admin,Percepteur,Financier")]
        public async Task<ActionResult<PerceptionRapportResponseDto>> GetRapportPerception(
            [FromQuery] string? origine,
            [FromQuery] string? statut,
            [FromQuery] DateTime? dateDebut,
            [FromQuery] DateTime? dateFin,
            [FromQuery] int? agentId,
            [FromQuery] int? affilieId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation(
                    "Rapport perception — origine {Origine}, statut {Statut}, agent {AgentId}, affilié {AffilieId}",
                    origine, statut, agentId, affilieId);

                var pagination = new PaginationRequest { Page = pageNumber, PageSize = pageSize };
                var rapport = await _dashboardService.GetRapportPerceptionAsync(
                    dateDebut, dateFin, origine, statut, agentId, affilieId, pagination, ct);

                return Ok(rapport);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du rapport perception");
                return this.TechnicalErrorResponse("Erreur lors de la récupération du rapport perception", ex);
            }
        }
    }
}
