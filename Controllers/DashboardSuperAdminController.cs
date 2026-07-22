using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProsocAPI.Models.DTOs.DashboardAdmin;
using ProsocAPI.Models.DTOs.DashboardSuperAdmin;
using ProsocAPI.Services.Repositories;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SuperAdmin")]
    public class DashboardSuperAdminController : ControllerBase
    {
        private readonly IDashboardSuperAdminRepository _dashboardService;
        private readonly ILogger<DashboardSuperAdminController> _logger;

        public DashboardSuperAdminController(
            IDashboardSuperAdminRepository dashboardService,
            ILogger<DashboardSuperAdminController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        /// <summary>Dashboard consolidé SuperAdmin (KPIs admin + gouvernance système).</summary>
        [HttpGet("summary")]
        public async Task<ActionResult<DashboardSuperAdminDto>> GetSummary(CancellationToken ct)
        {
            try
            {
                return Ok(await _dashboardService.GetDashboardSummaryAsync(ct));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur dashboard SuperAdmin (summary)");
                return this.TechnicalErrorResponse("Erreur lors de la récupération du dashboard SuperAdmin", ex);
            }
        }

        /// <summary>KPIs métier (même périmètre que DashboardAdmin).</summary>
        [HttpGet("kpis-admin")]
        public async Task<ActionResult<DashboardAdminKpisDto>> GetKpisAdmin(CancellationToken ct)
        {
            try
            {
                return Ok(await _dashboardService.GetKpisAdminAsync(ct));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur KPIs admin SuperAdmin");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des KPIs admin", ex);
            }
        }

        /// <summary>KPIs plateforme (utilisateurs, rôles, FlexPay, paiements en attente).</summary>
        [HttpGet("kpis-systeme")]
        public async Task<ActionResult<SuperAdminSystemKpisDto>> GetKpisSysteme(CancellationToken ct)
        {
            try
            {
                return Ok(await _dashboardService.GetKpisSystemeAsync(ct));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur KPIs système SuperAdmin");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des KPIs système", ex);
            }
        }

        /// <summary>Répartition des utilisateurs actifs par rôle.</summary>
        [HttpGet("utilisateurs-par-role")]
        public async Task<ActionResult<List<UtilisateursParRoleDto>>> GetUtilisateursParRole(CancellationToken ct)
        {
            try
            {
                return Ok(await _dashboardService.GetUtilisateursParRoleAsync(ct));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur répartition utilisateurs par rôle");
                return this.TechnicalErrorResponse("Erreur lors de la répartition par rôle", ex);
            }
        }
    }
}
