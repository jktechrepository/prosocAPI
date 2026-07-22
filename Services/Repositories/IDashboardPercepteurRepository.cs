using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.Pagination;

namespace ProsocAPI.Services.Repositories
{
    public interface IDashboardPercepteurRepository
    {
        // KPIs du percepteur
        Task<PercepteurKpisDto> GetKpisPercepteurAsync(CancellationToken ct = default);
        
        // Graphiques du percepteur
        Task<PercepteurGraphsDto> GetGraphsPercepteurAsync(CancellationToken ct = default);
        
        // Transactions du percepteur
        Task<List<PercepteurTransactionDto>> GetTransactionsAsync(int limit = 50, CancellationToken ct = default);
        
        // Performance journalière
        Task<List<PerformanceJournaliereDto>> GetPerformancesJournalieresAsync(int jours = 30, CancellationToken ct = default);
        
        // Résumé mensuel
        Task<List<ResumeMensuelDto>> GetResumeMensuelsAsync(int mois = 12, CancellationToken ct = default);
        
        // Top agents par performance
        Task<List<TopAgentPercepteurDto>> GetTopAgentsPerformanceAsync(int limit = 10, CancellationToken ct = default);
        
        // Répartition des transactions par type
        Task<List<TransactionTypeDto>> GetTransactionsParTypeAsync(CancellationToken ct = default);
        
        // Répartition des paiements par mode
        Task<List<PaiementModeDto>> GetPaiementsParModeAsync(CancellationToken ct = default);
        
        // Statistiques des agents
        Task<List<AgentStatsDto>> GetAgentsStatsAsync(CancellationToken ct = default);
        
        // Tendances des transactions
        Task<List<TendanceTransactionDto>> GetTendancesTransactionsAsync(int jours = 30, CancellationToken ct = default);
        
        // Objectifs du percepteur
        Task<List<ObjectifPercepteurDto>> GetObjectifsPercepteurAsync(CancellationToken ct = default);
        
        // Résumé des frais
        Task<List<ResumeFraisDto>> GetResumeFraisAsync(CancellationToken ct = default);
        
        // Dashboard percepteur complet
        Task<DashboardPercepteurDto> GetDashboardPercepteurAsync(CancellationToken ct = default);
        
        // Montant total perçu par période
        Task<decimal> GetMontantPerçuAsync(DateTime debut, DateTime fin, CancellationToken ct = default);
        
        // Nombre de transactions par période
        Task<int> GetNombreTransactionsAsync(DateTime debut, DateTime fin, CancellationToken ct = default);
        
        // Montant moyen des transactions par période
        Task<decimal> GetMontantMoyenAsync(DateTime debut, DateTime fin, CancellationToken ct = default);
        
        // Taux de succès par période
        Task<decimal> GetTauxSuccesAsync(DateTime debut, DateTime fin, CancellationToken ct = default);
        
        // Montant des commissions par période
        Task<decimal> GetMontantCommissionsAsync(DateTime debut, DateTime fin, CancellationToken ct = default);
        
        // Montant des frais par période
        Task<decimal> GetMontantFraisAsync(DateTime debut, DateTime fin, CancellationToken ct = default);
        
        // Net à percevoir par période
        Task<decimal> GetNetAPercevoirAsync(DateTime debut, DateTime fin, CancellationToken ct = default);
        
        // Transactions par statut
        Task<List<PercepteurTransactionDto>> GetTransactionsParStatutAsync(string statut, CancellationToken ct = default);
        
        // Transactions par agent
        Task<List<PercepteurTransactionDto>> GetTransactionsParAgentAsync(int agentId, CancellationToken ct = default);
        
        // Performance par agent
        Task<AgentStatsDto> GetPerformanceAgentAsync(int agentId, CancellationToken ct = default);
        
        // Évolution des transactions
        Task<List<TendanceTransactionDto>> GetEvolutionTransactionsAsync(int mois = 12, CancellationToken ct = default);
        
        // Résumé journalier
        Task<List<PerformanceJournaliereDto>> GetResumeJournalierAsync(DateTime debut, DateTime fin, CancellationToken ct = default);
        
        // Solde à percevoir
        Task<decimal> GetSoldeAPercevoirAsync(CancellationToken ct = default);
        
        // Montant en attente
        Task<decimal> GetMontantEnAttenteAsync(CancellationToken ct = default);
        
        // Transactions en attente
        Task<int> GetTransactionsEnAttenteAsync(CancellationToken ct = default);

        Task<PerceptionRapportSyntheseDto> GetRapportPerceptionSyntheseAsync(
            DateTime? dateDebut = null,
            DateTime? dateFin = null,
            string? origine = null,
            string? statut = null,
            int? agentId = null,
            int? affilieId = null,
            CancellationToken ct = default);

        Task<PerceptionRapportResponseDto> GetRapportPerceptionAsync(
            DateTime? dateDebut = null,
            DateTime? dateFin = null,
            string? origine = null,
            string? statut = null,
            int? agentId = null,
            int? affilieId = null,
            PaginationRequest? pagination = null,
            CancellationToken ct = default);
    }
}
