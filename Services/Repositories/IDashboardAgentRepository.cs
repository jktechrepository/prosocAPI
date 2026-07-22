using ProsocAPI.Models.DTOs.Core;

namespace ProsocAPI.Services.Repositories
{
    public interface IDashboardAgentRepository
    {
        // KPIs personnels de l'agent
        Task<AgentKpisDto> GetAgentKpisAsync(int agentId, CancellationToken ct = default);
        
        // Graphiques personnels de l'agent
        Task<AgentGraphsDto> GetAgentGraphsAsync(int agentId, CancellationToken ct = default);
        
        // Performance personnelle de l'agent
        Task<AgentPerformanceDto> GetAgentPerformanceAsync(int agentId, CancellationToken ct = default);
        
        // Liste des affiliés récents de l'agent
        Task<List<AgentAffilieRecentDto>> GetAffiliesRecentsAsync(int agentId, int limit = 10, CancellationToken ct = default);
        
        // Collectes en attente de l'agent
        Task<List<AgentCollecteEnAttenteDto>> GetCollectesEnAttenteAsync(int agentId, CancellationToken ct = default);
        
        // Commissions détaillées de l'agent
        Task<List<AgentCommissionDto>> GetCommissionsAsync(int agentId, int mois, int annee, CancellationToken ct = default);
        
        // Objectifs de l'agent
        Task<AgentObjectifDto> GetObjectifsAsync(int agentId, int mois, int annee, CancellationToken ct = default);
        
        // Statistiques des prestations de l'agent
        Task<List<PrestationStatsDto>> GetPrestationsStatsAsync(int agentId, CancellationToken ct = default);
        
        // Activité quotidienne de l'agent
        Task<List<DailyActivityDto>> GetActiviteQuotidienneAsync(int agentId, int jours = 30, CancellationToken ct = default);
        
        // Résumé des performances mensuelles
        Task<List<MonthlyCollectionDto>> GetPerformancesMensuellesAsync(int agentId, int mois, CancellationToken ct = default);
        
        // Top des affiliés de l'agent
        Task<List<AgentAffilieRecentDto>> GetTopAffiliesAsync(int agentId, int limit = 5, CancellationToken ct = default);

        /// <summary>Primes générées (collectes souscription) pour l'agent.</summary>
        Task<AgentPrimesResumeDto> GetPrimesGenereesAsync(int agentId, int? mois, int? annee, int limitDetails, CancellationToken ct = default);

        /// <summary>Commissions wallet (mouvements crédit) pour l'agent.</summary>
        Task<AgentCommissionsResumeDto> GetCommissionsResumeAsync(int agentId, int limitMouvements, CancellationToken ct = default);

        /// <summary>Suivi des adhérents rattachés à l'agent.</summary>
        Task<List<AgentSuiviAdherentDto>> GetSuiviAdherentsAsync(
            int agentId,
            int limit,
            string? statutGlobal = null,
            CancellationToken ct = default);

        /// <summary>Dashboard consolidé AT — primes, commissions, suivi adhérents.</summary>
        Task<AgentTerrainDashboardDto> GetDashboardTerrainAsync(int agentId, CancellationToken ct = default);
    }
}
