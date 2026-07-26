using ProsocAPI.Models.DTOs.Core;

namespace ProsocAPI.Services.Repositories
{
    public interface IDashboardFinancierRepository
    {
        // KPIs financiers globaux
        Task<FinancierKpisDto> GetKpisFinanciersAsync(CancellationToken ct = default);
        
        // Graphiques financiers
        Task<FinancierGraphsDto> GetGraphsFinanciersAsync(CancellationToken ct = default);
        
        // Performance mensuelle
        Task<List<PerformanceMensuelleDto>> GetPerformancesMensuellesAsync(int mois = 12, CancellationToken ct = default);
        
        // Répartition des revenus par source
        Task<List<RevenusSourceDto>> GetRevenusParSourceAsync(CancellationToken ct = default);
        
        // Top agents par performance
        Task<List<TopAgentPerformanceDto>> GetTopAgentsPerformanceAsync(int limit = 10, CancellationToken ct = default);
        
        // Commissions par agent
        Task<List<CommissionAgentDto>> GetCommissionsAgentsAsync(CancellationToken ct = default);
        
        // Statistiques des produits
        Task<List<ProduitStatsDto>> GetProduitsStatsAsync(CancellationToken ct = default);
        
        // Tendances financières
        Task<List<TendanceFinanciereDto>> GetTendancesFinancieresAsync(int jours = 30, CancellationToken ct = default);
        
        // Transactions par période
        Task<List<TransactionPeriodeDto>> GetTransactionsParPeriodeAsync(int jours = 30, CancellationToken ct = default);
        
        // Objectifs financiers
        Task<List<ObjectifFinancierDto>> GetObjectifsFinanciersAsync(CancellationToken ct = default);

        /// <summary>Reporting TargetAgent adhésions : synthèse par rôle + détail par agent.</summary>
        Task<ObjectifsAgentsFinancierDto> GetObjectifsAgentsAsync(
            int? mois = null,
            int? annee = null,
            CancellationToken ct = default);

        // Répartition géographique des revenus
        Task<List<RevenuGeographiqueDto>> GetRevenusParRegionAsync(CancellationToken ct = default);
        
        // Indicateurs de rentabilité
        Task<RentabiliteDto> GetRentabiliteAsync(CancellationToken ct = default);
        
        // Dashboard financier complet
        Task<DashboardFinancierDto> GetDashboardFinancierAsync(CancellationToken ct = default);
        
        // Chiffre d'affaires par période
        Task<decimal> GetChiffreAffairesAsync(DateTime debut, DateTime fin, CancellationToken ct = default);
        
        // Montant total des collectes par période
        Task<decimal> GetMontantCollectesAsync(DateTime debut, DateTime fin, CancellationToken ct = default);
        
        // Montant total des commissions par période
        Task<decimal> GetMontantCommissionsAsync(DateTime debut, DateTime fin, CancellationToken ct = default);
        
        // Nombre total d'adhésions par période
        Task<int> GetNombreAdhesionsAsync(DateTime debut, DateTime fin, CancellationToken ct = default);
        
        // Évolution des revenus
        Task<List<TendanceFinanciereDto>> GetEvolutionRevenusAsync(int mois = 12, CancellationToken ct = default);
        
        // Performance par région
        Task<List<RevenuGeographiqueDto>> GetPerformanceParRegionAsync(CancellationToken ct = default);
        
        // Rentabilité par produit
        Task<List<ProduitStatsDto>> GetRentabiliteParProduitAsync(CancellationToken ct = default);
    }
}
