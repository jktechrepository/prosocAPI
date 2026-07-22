using ProsocAPI.Models.DTOs.Core;

namespace ProsocAPI.Services
{
    public interface ICommissionDashboardService
    {
        /// <summary>
        /// Récupérer le dashboard complet des commissions pour un agent
        /// </summary>
        Task<CommissionDashboardDto> GetDashboardAsync(int agentId);

        /// <summary>
        /// Récupérer la liste des commissions avec filtres et pagination
        /// </summary>
        Task<(List<CommissionItemDto> Commissions, int Total)> GetCommissionsAsync(int agentId, CommissionFilterDto filter);

        /// <summary>
        /// Récupérer les statistiques de commissions par période
        /// </summary>
        Task<List<CommissionStatsDto>> GetStatsAsync(int agentId, DateTime debut, DateTime fin);

        /// <summary>
        /// Récupérer le résumé mensuel des commissions
        /// </summary>
        Task<List<MonthlyCommissionSummaryDto>> GetMonthlySummariesAsync(int agentId, int annee);

        /// <summary>
        /// Exporter les commissions au format spécifié
        /// </summary>
        Task<CommissionExportDto> ExportCommissionsAsync(int agentId, CommissionFilterDto filter, string format = "csv");

        /// <summary>
        /// Récupérer les tendances de commissions (graphique)
        /// </summary>
        Task<List<DailyCommissionDto>> GetTrendsAsync(int agentId, DateTime debut, DateTime fin);
    }
}
