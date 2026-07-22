using System.ComponentModel.DataAnnotations;

namespace ProsocAPI.Models.DTOs.Core
{
    // KPIs financiers globaux
    public class FinancierKpisDto
    {
        public decimal ChiffreAffairesTotal { get; set; }
        public decimal MontantTotalCollectes { get; set; }
        /// <summary>Code ISO de la devise principale (ex. USD) pour tous les montants consolidés.</summary>
        public string CodeDeviseConsolidation { get; set; } = "USD";
        public decimal MontantTotalCommissions { get; set; }
        public decimal TauxCroissanceCA { get; set; }
        public decimal TauxCroissanceCollectes { get; set; }
        public decimal TauxCroissanceCommissions { get; set; }
        public int NombreTotalAdhesions { get; set; }
        public int NombreTotalAgents { get; set; }
        public decimal PanierMoyen { get; set; }
        public decimal TauxConversion { get; set; }
    }

    // Performance financière mensuelle
    public class PerformanceMensuelleDto
    {
        public string Mois { get; set; } = string.Empty;
        public decimal ChiffreAffaires { get; set; }
        public decimal MontantCollectes { get; set; }
        public decimal MontantCommissions { get; set; }
        public int NombreAdhesions { get; set; }
        public int NombreCollectes { get; set; }
        public decimal PanierMoyen { get; set; }
        public decimal TauxConversion { get; set; }
        public decimal ObjectifCA { get; set; }
        public decimal AtteinteObjectifCA { get; set; }
    }

    // Répartition des revenus par source
    public class RevenusSourceDto
    {
        public string Source { get; set; } = string.Empty;
        public decimal Montant { get; set; }
        public decimal Pourcentage { get; set; }
        public int NombreTransactions { get; set; }
    }

    // Évolution des commissions par agent
    public class CommissionAgentDto
    {
        public int AgentId { get; set; }
        public string NomAgent { get; set; } = string.Empty;
        public decimal MontantCommission { get; set; }
        public decimal MontantCollectes { get; set; }
        public int NombreCollectes { get; set; }
        public decimal TauxCommission { get; set; }
        public decimal Progression { get; set; }
    }

    // Top agents par performance
    public class TopAgentPerformanceDto
    {
        public int AgentId { get; set; }
        public string NomAgent { get; set; } = string.Empty;
        public decimal ChiffreAffaires { get; set; }
        public decimal MontantCommissions { get; set; }
        public int NombreAdhesions { get; set; }
        public int NombreCollectes { get; set; }
        public decimal PanierMoyen { get; set; }
        public decimal TauxConversion { get; set; }
        public int Rang { get; set; }
    }

    // Statistiques des produits
    public class ProduitStatsDto
    {
        public string NomProduit { get; set; } = string.Empty;
        public int NombreSouscriptions { get; set; }
        public decimal MontantTotal { get; set; }
        public decimal Pourcentage { get; set; }
        public decimal Croissance { get; set; }
        public decimal MontantMoyen { get; set; }
    }

    // Tendances financières
    public class TendanceFinanciereDto
    {
        public string Periode { get; set; } = string.Empty;
        public decimal ChiffreAffaires { get; set; }
        public decimal MontantCollectes { get; set; }
        public decimal MontantCommissions { get; set; }
        public int NombreAdhesions { get; set; }
        public decimal TauxCroissanceCA { get; set; }
        public decimal TauxCroissanceCollectes { get; set; }
        public decimal TauxCroissanceCommissions { get; set; }
    }

    // Résumé des transactions par période
    public class TransactionPeriodeDto
    {
        public string Periode { get; set; } = string.Empty;
        public int NombreTransactions { get; set; }
        public decimal MontantTotal { get; set; }
        public decimal MontantMoyen { get; set; }
        public decimal MontantMin { get; set; }
        public decimal MontantMax { get; set; }
        public int TransactionsReussies { get; set; }
        public int TransactionsEchouees { get; set; }
        public decimal TauxSucces { get; set; }
    }

    // Objectifs financiers
    public class ObjectifFinancierDto
    {
        public string TypeObjectif { get; set; } = string.Empty;
        public decimal Objectif { get; set; }
        public decimal Realise { get; set; }
        public decimal Atteinte { get; set; }
        public decimal Restant { get; set; }
        public string Periode { get; set; } = string.Empty;
        public decimal ProgressionPrecedente { get; set; }
    }

    // Répartition géographique des revenus
    public class RevenuGeographiqueDto
    {
        public string Region { get; set; } = string.Empty;
        public decimal Montant { get; set; }
        public decimal Pourcentage { get; set; }
        public int NombreClients { get; set; }
        public int NombreAgents { get; set; }
        public decimal Croissance { get; set; }
    }

    // Indicateurs de rentabilité
    public class RentabiliteDto
    {
        public decimal MargeBrute { get; set; }
        public decimal TauxMargeBrute { get; set; }
        public decimal CoutAcquisition { get; set; }
        public decimal ValeurClient { get; set; }
        public decimal RetourInvestissement { get; set; }
        public decimal TauxRetention { get; set; }
        public decimal ChurnRate { get; set; }
        public decimal LTV { get; set; }
    }

    // Graphiques financiers
    public class FinancierGraphsDto
    {
        public List<PerformanceMensuelleDto> PerformancesMensuelles { get; set; } = new();
        public List<RevenusSourceDto> RevenusParSource { get; set; } = new();
        public List<TendanceFinanciereDto> Tendances { get; set; } = new();
        public List<TransactionPeriodeDto> TransactionsParPeriode { get; set; } = new();
        public List<RevenuGeographiqueDto> RevenusParRegion { get; set; } = new();
    }

    // Dashboard financier complet
    public class DashboardFinancierDto
    {
        public FinancierKpisDto Kpis { get; set; } = new();
        public string CodeDeviseConsolidation { get; set; } = "USD";
        public FinancierGraphsDto Graphs { get; set; } = new();
        public List<TopAgentPerformanceDto> TopAgents { get; set; } = new();
        public List<CommissionAgentDto> CommissionsAgents { get; set; } = new();
        public List<ProduitStatsDto> ProduitsStats { get; set; } = new();
        public List<ObjectifFinancierDto> Objectifs { get; set; } = new();
        public RentabiliteDto Rentabilite { get; set; } = new();
        public DateTime DerniereMiseAJour { get; set; }
    }
}
