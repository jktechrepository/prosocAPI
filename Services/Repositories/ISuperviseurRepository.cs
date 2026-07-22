using ProsocAPI.Models.DTOs.Core;

namespace ProsocAPI.Services.Repositories
{
    public interface ISuperviseurRepository
    {
        // Statistiques générales du superviseur
        Task<SuperviseurStatsDto> GetStatsSuperviseurAsync(int superviseurId, CancellationToken ct = default);
        
        // Performance des agents supervisés
        Task<List<AgentPerformanceDto>> GetPerformancesAgentsAsync(int superviseurId, CancellationToken ct = default);
        Task<AgentPerformanceDto> GetPerformanceAgentAsync(int superviseurId, int agentId, CancellationToken ct = default);
        Task<List<AgentPerformanceDto>> GetTopAgentsAsync(int superviseurId, int limit = 10, CancellationToken ct = default);
        
        // Gestion de la hiérarchie
        Task<HierarchieSuperviseurDto> GetHierarchieCompleteAsync(int superviseurId, CancellationToken ct = default);
        Task<List<AgentHierarchieDto>> GetAgentsSupervisesDirectsAsync(int superviseurId, CancellationToken ct = default);
        Task<List<AgentHierarchieDto>> GetTousAgentsHierarchieAsync(int superviseurId, CancellationToken ct = default);
        Task<bool> EstDansHierarchieAsync(int superviseurId, int agentId, CancellationToken ct = default);
        
        // Gestion des affectations
        Task<List<AffectationSuperviseurDto>> GetAffectationsRecentesAsync(int superviseurId, int limit = 20, CancellationToken ct = default);
        Task<List<AffectationSuperviseurDto>> GetHistoriqueAffectationsAsync(int agentId, CancellationToken ct = default);
        
        // Objectifs d'équipe
        Task<List<ObjectifEquipeDto>> GetObjectifsEquipeAsync(int superviseurId, CancellationToken ct = default);
        Task<ObjectifEquipeDto> CreerObjectifEquipeAsync(ObjectifEquipeDto objectif, CancellationToken ct = default);
        Task<bool> ModifierObjectifEquipeAsync(int objectifId, ObjectifEquipeDto objectif, CancellationToken ct = default);
        Task<bool> SupprimerObjectifEquipeAsync(int objectifId, CancellationToken ct = default);
        
        // Rapports de performance
        Task<RapportPerformanceEquipeDto> GetRapportPerformanceAsync(int superviseurId, DateTime debut, DateTime fin, CancellationToken ct = default);
        Task<List<RapportPerformanceEquipeDto>> GetRapportsPeriodiquesAsync(int superviseurId, int mois = 6, CancellationToken ct = default);
        Task<byte[]> ExporterRapportPerformanceAsync(int superviseurId, DateTime debut, DateTime fin, CancellationToken ct = default);
        
        // Comparaison entre équipes
        Task<ComparaisonEquipesDto> GetComparaisonEquipesAsync(List<int> superviseurIds, DateTime debut, DateTime fin, CancellationToken ct = default);
        Task<List<SuperviseurStatsDto>> GetClassementSuperviseursAsync(DateTime debut, DateTime fin, CancellationToken ct = default);
        
        // Tendances et analyses
        Task<List<TendanceEquipeDto>> GetTendancesEquipeAsync(int superviseurId, int mois = 12, CancellationToken ct = default);
        Task<List<TendanceEquipeDto>> GetTendancesGeneralesAsync(int mois = 12, CancellationToken ct = default);
        Task<ActiviteSuperviseurDto> GetActiviteJournaliereAsync(int superviseurId, CancellationToken ct = default);
        Task<List<ActiviteSuperviseurDto>> GetActivitePeriodiqueAsync(int superviseurId, DateTime debut, DateTime fin, CancellationToken ct = default);
        
        // Permissions du superviseur
        Task<PermissionSuperviseurDto> GetPermissionsSuperviseurAsync(int superviseurId, CancellationToken ct = default);
        Task<bool> ModifierPermissionsSuperviseurAsync(int superviseurId, PermissionSuperviseurDto permissions, CancellationToken ct = default);
        
        // Dashboard superviseur
        Task<DashboardSuperviseurDto> GetDashboardSuperviseurAsync(int superviseurId, CancellationToken ct = default);
        
        // Méthodes utilitaires
        Task<int> GetNombreTotalAgentsHierarchieAsync(int superviseurId, CancellationToken ct = default);
        Task<decimal> GetMontantTotalHierarchieAsync(int superviseurId, CancellationToken ct = default);
        Task<decimal> GetPerformanceMoyenneHierarchieAsync(int superviseurId, CancellationToken ct = default);
        Task<List<int>> GetIdsAgentsDansHierarchieAsync(int superviseurId, CancellationToken ct = default);
        Task<bool> VerifierPermissionAgentAsync(int superviseurId, int agentId, CancellationToken ct = default);
        
        // Statistiques avancées
        Task<decimal> GetTauxCroissanceEquipeAsync(int superviseurId, DateTime debut, DateTime fin, CancellationToken ct = default);
        Task<decimal> GetTauxSuccesEquipeAsync(int superviseurId, DateTime debut, DateTime fin, CancellationToken ct = default);
        Task<decimal> GetMontantMoyenParAgentAsync(int superviseurId, DateTime debut, DateTime fin, CancellationToken ct = default);
        Task<int> GetNombreTransactionsEquipeAsync(int superviseurId, DateTime debut, DateTime fin, CancellationToken ct = default);
        
        // Gestion des alertes
        Task<List<string>> GetAlertesEquipeAsync(int superviseurId, CancellationToken ct = default);
        Task<bool> CreerAlerteEquipeAsync(int superviseurId, string message, CancellationToken ct = default);
        Task<bool> MarquerAlerteLueAsync(int alerteId, CancellationToken ct = default);
        
        // Export et reporting
        Task<byte[]> ExporterDonneesEquipeAsync(int superviseurId, DateTime debut, DateTime fin, string format = "Excel", CancellationToken ct = default);
        Task<byte[]> ExporterHierarchieAsync(int superviseurId, CancellationToken ct = default);
        Task<byte[]> ExporterPerformancesAgentsAsync(int superviseurId, DateTime debut, DateTime fin, CancellationToken ct = default);
    }
}
