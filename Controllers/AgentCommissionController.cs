using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prosoc.Data;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Services;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AgentCommissionController : ControllerBase
    {
        private readonly ICommissionDashboardService _dashboardService;
        private readonly ProsocDbContext _db;
        private readonly ILogger<AgentCommissionController> _logger;

        public AgentCommissionController(
            ICommissionDashboardService dashboardService,
            ProsocDbContext db,
            ILogger<AgentCommissionController> logger)
        {
            _dashboardService = dashboardService;
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// Dashboard complet des commissions d'un agent. <paramref name="idAgent"/> est passé en paramètre de requête.
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<ActionResult<CommissionDashboardDto>> GetDashboard(
            [FromQuery][Range(1, int.MaxValue)] int idAgent,
            CancellationToken ct)
        {
            try
            {
                var agentId = await ResolveAgentIdAsync(idAgent, ct);
                var dashboard = await _dashboardService.GetDashboardAsync(agentId);
                return Ok(dashboard);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la récupération du dashboard agent ",
                    ex);
            }
        }

        /// <summary>
        /// Liste des commissions avec filtres et pagination (<c>filter.idAgent</c> obligatoire).
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<object>> GetCommissions(
            [FromQuery] CommissionFilterDto filter,
            CancellationToken ct)
        {
            try
            {
                var agentId = await ResolveAgentIdAsync(filter.IdAgent, ct);
                var (commissions, total) = await _dashboardService.GetCommissionsAsync(agentId, filter);

                return Ok(new
                {
                    commissions,
                    total,
                    page = filter.Page,
                    pageSize = filter.PageSize,
                    totalPages = (int)Math.Ceiling((double)total / filter.PageSize)
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la récupération des commissions",
                    ex);
            }
        }

        /// <summary>
        /// Statistiques de commissions par période pour un agent.
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<List<CommissionStatsDto>>> GetStats(
            [FromQuery][Range(1, int.MaxValue)] int idAgent,
            [FromQuery] DateTime debut,
            [FromQuery] DateTime fin,
            CancellationToken ct)
        {
            try
            {
                var agentId = await ResolveAgentIdAsync(idAgent, ct);
                var stats = await _dashboardService.GetStatsAsync(agentId, debut, fin);
                return Ok(stats);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la récupération des statistiques",
                    ex);
            }
        }

        /// <summary>
        /// Résumés mensuels des commissions d'un agent.
        /// </summary>
        [HttpGet("monthly/{annee:int}")]
        public async Task<ActionResult<List<MonthlyCommissionSummaryDto>>> GetMonthlySummaries(
            int annee,
            [FromQuery][Range(1, int.MaxValue)] int idAgent,
            CancellationToken ct)
        {
            try
            {
                var agentId = await ResolveAgentIdAsync(idAgent, ct);
                var summaries = await _dashboardService.GetMonthlySummariesAsync(agentId, annee);
                return Ok(summaries);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la récupération des résumés mensuels",
                    ex);
            }
        }

        /// <summary>
        /// Tendances de commissions (graphique) pour un agent.
        /// </summary>
        [HttpGet("trends")]
        public async Task<ActionResult<List<DailyCommissionDto>>> GetTrends(
            [FromQuery][Range(1, int.MaxValue)] int idAgent,
            [FromQuery] DateTime debut,
            [FromQuery] DateTime fin,
            CancellationToken ct)
        {
            try
            {
                var agentId = await ResolveAgentIdAsync(idAgent, ct);
                var trends = await _dashboardService.GetTrendsAsync(agentId, debut, fin);
                return Ok(trends);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la récupération des tendances",
                    ex);
            }
        }

        /// <summary>
        /// Export CSV des commissions (<c>filter.idAgent</c> obligatoire).
        /// </summary>
        [HttpGet("export/csv")]
        public async Task<ActionResult> ExportCsv([FromQuery] CommissionFilterDto filter, CancellationToken ct)
        {
            try
            {
                var agentId = await ResolveAgentIdAsync(filter.IdAgent, ct);
                var export = await _dashboardService.ExportCommissionsAsync(agentId, filter, "csv");

                var csv = GenerateCsvContent(export);
                var bytes = Encoding.UTF8.GetBytes(csv);

                return File(
                    bytes,
                    "text/csv",
                    $"commissions_{export.AgentNom.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.csv");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de l'export CSV",
                    ex);
            }
        }

        /// <summary>
        /// Export JSON des commissions (<c>filter.idAgent</c> obligatoire).
        /// </summary>
        [HttpGet("export/json")]
        public async Task<ActionResult<CommissionExportDto>> ExportJson(
            [FromQuery] CommissionFilterDto filter,
            CancellationToken ct)
        {
            try
            {
                var agentId = await ResolveAgentIdAsync(filter.IdAgent, ct);
                var export = await _dashboardService.ExportCommissionsAsync(agentId, filter, "json");
                return Ok(export);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de l'export JSON",
                    ex);
            }
        }

        private async Task<int> ResolveAgentIdAsync(int idAgent, CancellationToken ct)
        {
            if (idAgent <= 0)
                throw new ArgumentException("Le paramètre idAgent est obligatoire et doit être > 0.");

            var exists = await _db.Agents.AnyAsync(a => a.IdAgent == idAgent, ct);
            if (!exists)
                throw new KeyNotFoundException($"Agent avec ID {idAgent} introuvable.");

            return idAgent;
        }

        private string GenerateCsvContent(CommissionExportDto export)
        {
            var csv = new StringBuilder();

            csv.AppendLine("Commission Report");
            csv.AppendLine($"Agent: {export.AgentNom} ({export.AgentMatricule})");
            csv.AppendLine($"Période: {export.Periode}");
            csv.AppendLine($"Date de génération: {export.DateGeneration}");
            csv.AppendLine();

            csv.AppendLine("Résumé");
            csv.AppendLine($"Total des commissions: {export.Resume.TotalCommissions:F2} {export.Resume.Devise}");
            csv.AppendLine($"Nombre de commissions: {export.Resume.NombreCommissions}");
            csv.AppendLine($"Commission moyenne: {export.Resume.CommissionMoyenne:F2} {export.Resume.Devise}");
            csv.AppendLine($"Solde actuel: {export.Resume.SoldeActuel:F2} {export.Resume.Devise}");
            csv.AppendLine();

            csv.AppendLine("Détail des commissions");
            csv.AppendLine("Date;Montant;Source;Description;Affilié;Montant collecte;Solde après opération");

            foreach (var commission in export.Commissions)
            {
                csv.AppendLine($"{commission.DateOperation:dd/MM/yyyy HH:mm};{commission.Montant:F2};{commission.Source};\"{commission.Description}\";\"{commission.AffilieNom}\";{commission.CollecteMontant:F2};{commission.SoldeApresOperation:F2}");
            }

            return csv.ToString();
        }
    }
}
