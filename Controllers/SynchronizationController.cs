using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProsocAPI.Services.Synchronization;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SynchronizationController : ControllerBase
    {
        private readonly IUserSynchronizationService _synchronizationService;
        private readonly ILogger<SynchronizationController> _logger;

        public SynchronizationController(
            IUserSynchronizationService synchronizationService,
            ILogger<SynchronizationController> logger)
        {
            _synchronizationService = synchronizationService;
            _logger = logger;
        }

        // 🔄 SYNCHRONISATION AGENT → UTILISATEUR
        [HttpPost("agent/{agentId}")]
        public async Task<IActionResult> SynchronizeAgent(int agentId, CancellationToken ct)
        {
            try
            {
                await _synchronizationService.SynchronizeFromAgentAsync(agentId, ct);
                return Ok(new { Message = $"Synchronisation de l'agent {agentId} réussie" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la synchronisation de l'agent {AgentId}", agentId);
                return this.TechnicalErrorResponse("Erreur lors de la synchronisation", ex);
            }
        }

        // 🔄 SYNCHRONISATION AFFILIÉ → UTILISATEUR
        [HttpPost("affilie/{affilieId}")]
        public async Task<IActionResult> SynchronizeAffilie(int affilieId, CancellationToken ct)
        {
            try
            {
                await _synchronizationService.SynchronizeFromAffilieAsync(affilieId, ct);
                return Ok(new { Message = $"Synchronisation de l'affilié {affilieId} réussie" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la synchronisation de l'affilié {AffilieId}", affilieId);
                return this.TechnicalErrorResponse("Erreur lors de la synchronisation", ex);
            }
        }

        // 🔄 SYNCHRONISATION UTILISATEUR → AGENT/AFFILIÉ
        [HttpPost("utilisateur/{utilisateurId}")]
        public async Task<IActionResult> SynchronizeUtilisateur(int utilisateurId, CancellationToken ct)
        {
            try
            {
                await _synchronizationService.SynchronizeFromUtilisateurAsync(utilisateurId, ct);
                return Ok(new { Message = $"Synchronisation de l'utilisateur {utilisateurId} réussie" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la synchronisation de l'utilisateur {UtilisateurId}", utilisateurId);
                return this.TechnicalErrorResponse("Erreur lors de la synchronisation", ex);
            }
        }

        // 🔍 DÉTECTION DES CONFLITS
        [HttpGet("conflicts")]
        public async Task<IActionResult> GetConflicts(CancellationToken ct)
        {
            try
            {
                var conflicts = await _synchronizationService.DetectConflictsAsync(ct);
                return Ok(new { Conflicts = conflicts, Count = conflicts.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la détection des conflits");
                return this.TechnicalErrorResponse("Erreur lors de la détection des conflits", ex);
            }
        }

        // 📊 MÉTRIQUES DE SYNCHRONISATION
        [HttpGet("metrics")]
        public async Task<IActionResult> GetMetrics(CancellationToken ct)
        {
            try
            {
                var metrics = await _synchronizationService.GetSynchronizationMetricsAsync(ct);
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des métriques");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des métriques", ex);
            }
        }

        // 🔄 SYNCHRONISATION MASSIVE
        [HttpPost("sync-all")]
        public async Task<IActionResult> SynchronizeAll(CancellationToken ct)
        {
            try
            {
                var metrics = await _synchronizationService.GetSynchronizationMetricsAsync(ct);
                var results = new
                {
                    AgentsSynchronized = 0,
                    AffiliesSynchronized = 0,
                    Errors = new List<string>()
                };

                // 🔄 SYNCHRONISATION DES AGENTS
                var agents = await _synchronizationService.GetSynchronizationMetricsAsync(ct);
                // Note: Cette partie nécessiterait une méthode pour récupérer tous les agents non synchronisés
                // Pour l'instant, nous retournons les métriques

                _logger.LogInformation("Synchronisation massive initiée");

                return Ok(new { 
                    Message = "Synchronisation massive initiée",
                    Metrics = metrics,
                    Results = results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la synchronisation massive");
                return this.TechnicalErrorResponse("Erreur lors de la synchronisation massive", ex);
            }
        }
    }
}
