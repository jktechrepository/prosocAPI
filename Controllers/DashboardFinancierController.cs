using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Services.Repositories;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Financier")]
    public class DashboardFinancierController : ControllerBase
    {
        private readonly IDashboardFinancierRepository _dashboardService;
        private readonly ILogger<DashboardFinancierController> _logger;

        public DashboardFinancierController(IDashboardFinancierRepository dashboardService, ILogger<DashboardFinancierController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        /// <summary>
        /// Récupère les KPIs financiers globaux
        /// </summary>
        [HttpGet("kpis")]
        public async Task<ActionResult<FinancierKpisDto>> GetKpisFinanciers(CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération des KPIs financiers");

                var kpis = await _dashboardService.GetKpisFinanciersAsync(ct);
                
                _logger.LogInformation("KPIs financiers récupérés avec succès");
                return Ok(kpis);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des KPIs financiers");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des KPIs financiers", ex);
            }
        }

        /// <summary>
        /// Récupère les graphiques financiers
        /// </summary>
       /*
        [HttpGet("graphs")]
        public async Task<ActionResult<FinancierGraphsDto>> GetGraphsFinanciers(CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération des graphiques financiers");

                var graphs = await _dashboardService.GetGraphsFinanciersAsync(ct);
                
                _logger.LogInformation("Graphiques financiers récupérés avec succès");
                return Ok(graphs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des graphiques financiers");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des graphiques financiers", ex);
            }
        }
        */
        /// <summary>
        /// Récupère les performances mensuelles
        /// </summary>
        [HttpGet("performances-mensuelles")]
        public async Task<ActionResult<List<PerformanceMensuelleDto>>> GetPerformancesMensuelles([FromQuery] int mois, CancellationToken ct)
        {
            try
            {
                // Par défaut, mois = 12
                if (mois == 0) mois = 12;
                
                _logger.LogInformation("Récupération des performances sur {Mois} mois", mois);

                var performances = await _dashboardService.GetPerformancesMensuellesAsync(mois, ct);
                
                _logger.LogInformation("Performances mensuelles récupérées: {Count} mois", performances.Count);
                return Ok(performances);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des performances mensuelles");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des performances mensuelles", ex);
            }
        }

        /// <summary>
        /// Récupère la répartition des revenus par source
        /// </summary>
        [HttpGet("revenus-source")]
        public async Task<ActionResult<List<RevenusSourceDto>>> GetRevenusParSource(CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération des revenus par source");

                var revenus = await _dashboardService.GetRevenusParSourceAsync(ct);
                
                _logger.LogInformation("Revenus par source récupérés: {Count} sources", revenus.Count);
                return Ok(revenus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des revenus par source");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des revenus par source", ex);
            }
        }

        /// <summary>
        /// Récupère le top des agents par performance
        /// </summary>
        [HttpGet("top-agents")]
        public async Task<ActionResult<List<TopAgentPerformanceDto>>> GetTopAgentsPerformance([FromQuery] int limit, CancellationToken ct)
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
        /// Récupère les commissions par agent
        /// </summary>
        [HttpGet("commissions-agents")]
        public async Task<ActionResult<List<CommissionAgentDto>>> GetCommissionsAgents(CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération des commissions par agent");

                var commissions = await _dashboardService.GetCommissionsAgentsAsync(ct);
                
                _logger.LogInformation("Commissions par agent récupérées: {Count} agents", commissions.Count);
                return Ok(commissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des commissions par agent");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des commissions par agent", ex);
            }
        }

        /// <summary>
        /// Récupère les statistiques des produits
        /// </summary>
        [HttpGet("produits-stats")]
        public async Task<ActionResult<List<ProduitStatsDto>>> GetProduitsStats(CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération des statistiques des produits");

                var stats = await _dashboardService.GetProduitsStatsAsync(ct);
                
                _logger.LogInformation("Statistiques des produits récupérées: {Count} produits", stats.Count);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des statistiques des produits");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des statistiques des produits", ex);
            }
        }

        /// <summary>
        /// Récupère les tendances financières
        /// </summary>
        [HttpGet("tendances")]
        public async Task<ActionResult<List<TendanceFinanciereDto>>> GetTendancesFinancieres([FromQuery] int jours, CancellationToken ct)
        {
            try
            {
                // Par défaut, jours = 30
                if (jours == 0) jours = 30;
                
                _logger.LogInformation("Récupération des tendances sur {Jours} jours", jours);

                var tendances = await _dashboardService.GetTendancesFinancieresAsync(jours, ct);
                
                _logger.LogInformation("Tendances financières récupérées: {Count} jours", tendances.Count);
                return Ok(tendances);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des tendances financières");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des tendances financières", ex);
            }
        }

        /// <summary>
        /// Récupère les transactions par période
        /// </summary>
        [HttpGet("transactions-periode")]
        public async Task<ActionResult<List<TransactionPeriodeDto>>> GetTransactionsParPeriode([FromQuery] int jours, CancellationToken ct)
        {
            try
            {
                // Par défaut, jours = 30
                if (jours == 0) jours = 30;
                
                _logger.LogInformation("Récupération des transactions sur {Jours} jours", jours);

                var transactions = await _dashboardService.GetTransactionsParPeriodeAsync(jours, ct);
                
                _logger.LogInformation("Transactions par période récupérées: {Count} périodes", transactions.Count);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des transactions par période");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des transactions par période", ex);
            }
        }

        /// <summary>
        /// Récupère les objectifs financiers
        /// </summary>
        [HttpGet("objectifs")]
        public async Task<ActionResult<List<ObjectifFinancierDto>>> GetObjectifsFinanciers(CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération des objectifs financiers");

                var objectifs = await _dashboardService.GetObjectifsFinanciersAsync(ct);
                
                _logger.LogInformation("Objectifs financiers récupérés: {Count} objectifs", objectifs.Count);
                return Ok(objectifs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des objectifs financiers");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des objectifs financiers", ex);
            }
        }

        /// <summary>
        /// Récupère les revenus par région
        /// </summary>
        [HttpGet("revenus-region")]
        public async Task<ActionResult<List<RevenuGeographiqueDto>>> GetRevenusParRegion(CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération des revenus par région");

                var revenus = await _dashboardService.GetRevenusParRegionAsync(ct);
                
                _logger.LogInformation("Revenus par région récupérés: {Count} régions", revenus.Count);
                return Ok(revenus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des revenus par région");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des revenus par région", ex);
            }
        }

        /// <summary>
        /// Récupère les indicateurs de rentabilité
        /// </summary>
        [HttpGet("rentabilite")]
        public async Task<ActionResult<RentabiliteDto>> GetRentabilite(CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération des indicateurs de rentabilité");

                var rentabilite = await _dashboardService.GetRentabiliteAsync(ct);
                
                _logger.LogInformation("Indicateurs de rentabilité récupérés avec succès");
                return Ok(rentabilite);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des indicateurs de rentabilité");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des indicateurs de rentabilité", ex);
            }
        }

        /// <summary>
        /// Récupère le dashboard financier complet
        /// </summary>
        [HttpGet("summary")]
        public async Task<ActionResult<DashboardFinancierDto>> GetDashboardFinancier(CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération du dashboard financier complet");

                var dashboard = await _dashboardService.GetDashboardFinancierAsync(ct);
                
                _logger.LogInformation("Dashboard financier récupéré avec succès");
                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du dashboard financier");
                return this.TechnicalErrorResponse("Erreur lors de la récupération du dashboard financier", ex);
            }
        }

        /// <summary>
        /// Récupère le chiffre d'affaires sur une période
        /// </summary>
        [HttpGet("chiffre-affaires")]
        public async Task<ActionResult<decimal>> GetChiffreAffaires([FromQuery] DateTime debut, [FromQuery] DateTime fin, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération du chiffre d'affaires du {Debut} au {Fin}", debut, fin);

                var ca = await _dashboardService.GetChiffreAffairesAsync(debut, fin, ct);
                
                _logger.LogInformation("Chiffre d'affaires récupéré: {CA}", ca);
                return Ok(ca);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du chiffre d'affaires");
                return this.TechnicalErrorResponse("Erreur lors de la récupération du chiffre d'affaires", ex);
            }
        }

        /// <summary>
        /// Récupère le montant des collectes sur une période
        /// </summary>
        [HttpGet("montant-collectes")]
        public async Task<ActionResult<decimal>> GetMontantCollectes([FromQuery] DateTime debut, [FromQuery] DateTime fin, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération du montant des collectes du {Debut} au {Fin}", debut, fin);

                var montant = await _dashboardService.GetMontantCollectesAsync(debut, fin, ct);
                
                _logger.LogInformation("Montant des collectes récupéré: {Montant}", montant);
                return Ok(montant);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du montant des collectes");
                return this.TechnicalErrorResponse("Erreur lors de la récupération du montant des collectes", ex);
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
        /// Récupère le nombre d'adhésions sur une période
        /// </summary>
        [HttpGet("nombre-adhesions")]
        public async Task<ActionResult<int>> GetNombreAdhesions([FromQuery] DateTime debut, [FromQuery] DateTime fin, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération du nombre d'adhésions du {Debut} au {Fin}", debut, fin);

                var nombre = await _dashboardService.GetNombreAdhesionsAsync(debut, fin, ct);
                
                _logger.LogInformation("Nombre d'adhésions récupéré: {Nombre}", nombre);
                return Ok(nombre);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du nombre d'adhésions");
                return this.TechnicalErrorResponse("Erreur lors de la récupération du nombre d'adhésions", ex);
            }
        }

        /// <summary>
        /// Récupère l'évolution des revenus
        /// </summary>
        [HttpGet("evolution-revenus")]
        public async Task<ActionResult<List<TendanceFinanciereDto>>> GetEvolutionRevenus([FromQuery] int mois, CancellationToken ct)
        {
            try
            {
                // Par défaut, mois = 12
                if (mois == 0) mois = 12;
                
                _logger.LogInformation("Récupération de l'évolution des revenus sur {Mois} mois", mois);

                var evolution = await _dashboardService.GetEvolutionRevenusAsync(mois, ct);
                
                _logger.LogInformation("Évolution des revenus récupérée: {Count} mois", evolution.Count);
                return Ok(evolution);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de l'évolution des revenus");
                return this.TechnicalErrorResponse("Erreur lors de la récupération de l'évolution des revenus", ex);
            }
        }

        /// <summary>
        /// Récupère la performance par région
        /// </summary>
        [HttpGet("performance-region")]
        public async Task<ActionResult<List<RevenuGeographiqueDto>>> GetPerformanceParRegion(CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération de la performance par région");

                var performance = await _dashboardService.GetPerformanceParRegionAsync(ct);
                
                _logger.LogInformation("Performance par région récupérée: {Count} régions", performance.Count);
                return Ok(performance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de la performance par région");
                return this.TechnicalErrorResponse("Erreur lors de la récupération de la performance par région", ex);
            }
        }

        /// <summary>
        /// Récupère la rentabilité par produit
        /// </summary>
        [HttpGet("rentabilite-produit")]
        public async Task<ActionResult<List<ProduitStatsDto>>> GetRentabiliteParProduit(CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération de la rentabilité par produit");

                var rentabilite = await _dashboardService.GetRentabiliteParProduitAsync(ct);
                
                _logger.LogInformation("Rentabilité par produit récupérée: {Count} produits", rentabilite.Count);
                return Ok(rentabilite);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de la rentabilité par produit");
                return this.TechnicalErrorResponse("Erreur lors de la récupération de la rentabilité par produit", ex);
            }
        }
    }
}
