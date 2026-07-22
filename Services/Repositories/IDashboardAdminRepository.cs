using ProsocAPI.Models.DTOs.DashboardAdmin;

namespace ProsocAPI.Services.Repositories
{
    /// <summary>
    /// Interface pour le service du dashboard administrateur
    /// </summary>
    public interface IDashboardAdminRepository
    {
        /// <summary>
        /// Récupère les KPIs principaux du dashboard
        /// </summary>
        Task<DashboardAdminKpisDto> GetKpisAsync(CancellationToken ct = default);

        /// <summary>
        /// Récupère les données pour les graphiques du dashboard
        /// </summary>
        Task<DashboardAdminGraphsDto> GetGraphsAsync(int mois = 12, CancellationToken ct = default);

        /// <summary>
        /// Récupère la performance des meilleurs agents
        /// </summary>
        Task<List<PerformanceAgentsDto>> GetTopAgentsAsync(int limit = 10, CancellationToken ct = default);

        /// <summary>
        /// Valide une collecte en attente
        /// </summary>
        Task<bool> ValidateCollecteAsync(int collecteId, CancellationToken ct = default);

        /// <summary>
        /// Active ou désactive un agent
        /// </summary>
        Task<bool> ToggleAgentStatusAsync(int agentId, CancellationToken ct = default);

        /// <summary>
        /// Récupère les collectes en attente de validation
        /// </summary>
        Task<List<CollecteEnAttenteDto>> GetCollectesEnAttenteAsync(CancellationToken ct = default);
    }
}
