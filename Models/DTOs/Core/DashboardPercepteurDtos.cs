using System.ComponentModel.DataAnnotations;
using ProsocAPI.Models.Pagination;

namespace ProsocAPI.Models.DTOs.Core
{
    // KPIs du percepteur
    public class PercepteurKpisDto
    {
        public decimal MontantTotalPerçu { get; set; }
        public decimal MontantDuJour { get; set; }
        public decimal MontantSemaine { get; set; }
        public decimal MontantMois { get; set; }
        public decimal MontantAnnee { get; set; }
        public int NombreTotalTransactions { get; set; }
        public int TransactionsDuJour { get; set; }
        public int TransactionsSemaine { get; set; }
        public int TransactionsMois { get; set; }
        public decimal MontantMoyenTransaction { get; set; }
        public decimal TauxCroissance { get; set; }
        public decimal ObjectifJournalier { get; set; }
        public decimal AtteinteObjectifJournalier { get; set; }
        public int NombreAgentsActifs { get; set; }
        public decimal TauxSucces { get; set; }
        /// <summary>Code ISO de la devise principale (ex. USD) pour les montants consolidés.</summary>
        public string? DevisePrincipaleCode { get; set; }
        public decimal MontantVirtuelEnAttente { get; set; }
        public int NombreCollectesVirtuellesEnAttente { get; set; }
        /// <summary>Synthèse perception Agent (VA) vs Affilié (guichet direct).</summary>
        public PerceptionRapportSyntheseDto? RapportPerception { get; set; }
    }

    // Transaction du percepteur
    public class PercepteurTransactionDto
    {
        public int IdTransaction { get; set; }
        public DateTime DateTransaction { get; set; }
        public decimal Montant { get; set; }
        public string TypeTransaction { get; set; } = string.Empty;
        public string Statut { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string NomAgent { get; set; } = string.Empty;
        public string NomAffilie { get; set; } = string.Empty;
        public string ModePaiement { get; set; } = string.Empty;
        public decimal Commission { get; set; }
        public decimal Frais { get; set; }
        public decimal NetAPercevoir { get; set; }
        public string? Notes { get; set; }
    }

    // Performance journalière
    public class PerformanceJournaliereDto
    {
        public string Date { get; set; } = string.Empty;
        public decimal MontantTotal { get; set; }
        public int NombreTransactions { get; set; }
        public decimal MontantMoyen { get; set; }
        public decimal ObjectifJournalier { get; set; }
        public decimal AtteinteObjectif { get; set; }
        public int TransactionsReussies { get; set; }
        public int TransactionsEchouees { get; set; }
        public decimal TauxSucces { get; set; }
        public decimal MontantCommissions { get; set; }
        public decimal MontantFrais { get; set; }
    }

    // Résumé mensuel
    public class ResumeMensuelDto
    {
        public string Mois { get; set; } = string.Empty;
        public decimal MontantTotal { get; set; }
        public int NombreTransactions { get; set; }
        public decimal MontantMoyen { get; set; }
        public decimal ObjectifMensuel { get; set; }
        public decimal AtteinteObjectif { get; set; }
        public decimal Croissance { get; set; }
        public decimal MontantCommissions { get; set; }
        public decimal MontantFrais { get; set; }
        public decimal NetAPercevoir { get; set; }
        public int NombreAgentsActifs { get; set; }
        public decimal TauxSucces { get; set; }
        public int TransactionsReussies { get; set; }  // ✅ Ajouté
    }

    // Top agents par performance
    public class TopAgentPercepteurDto
    {
        public int AgentId { get; set; }
        public string NomAgent { get; set; } = string.Empty;
        public decimal MontantTotal { get; set; }
        public int NombreTransactions { get; set; }
        public decimal MontantMoyen { get; set; }
        public decimal TauxSucces { get; set; }
        public decimal MontantCommissions { get; set; }
        public decimal NetAPercevoir { get; set; }
        public decimal Progression { get; set; }
        public int Rang { get; set; }
        public DateTime? DerniereTransaction { get; set; }  // ✅ Nullable
    }

    // Répartition des transactions par type
    public class TransactionTypeDto
    {
        public string Type { get; set; } = string.Empty;
        public decimal Montant { get; set; }
        public int NombreTransactions { get; set; }
        public decimal Pourcentage { get; set; }
        public decimal MontantMoyen { get; set; }
        public decimal TauxSucces { get; set; }
    }

    // Répartition des revenus par mode de paiement
    public class PaiementModeDto
    {
        public string ModePaiement { get; set; } = string.Empty;
        public decimal Montant { get; set; }
        public int NombreTransactions { get; set; }
        public decimal Pourcentage { get; set; }
        public decimal Frais { get; set; }
        public decimal NetAPercevoir { get; set; }
    }

    // Statistiques des agents
    public class AgentStatsDto
    {
        public int AgentId { get; set; }
        public string NomAgent { get; set; } = string.Empty;
        public decimal MontantTotal { get; set; }
        public int NombreTransactions { get; set; }
        public decimal MontantMoyen { get; set; }
        public decimal TauxSucces { get; set; }
        public decimal MontantCommissions { get; set; }
        public decimal MontantFrais { get; set; }
        public decimal NetAPercevoir { get; set; }
        public decimal Progression { get; set; }
        public DateTime? DerniereTransaction { get; set; }  // ✅ Nullable
        public int NombreJoursActifs { get; set; }
        public decimal PerformanceJournaliere { get; set; }
    }

    // Tendances des transactions
    public class TendanceTransactionDto
    {
        public string Periode { get; set; } = string.Empty;
        public decimal MontantTotal { get; set; }
        public int NombreTransactions { get; set; }
        public decimal MontantMoyen { get; set; }
        public decimal TauxCroissance { get; set; }
        public decimal TauxSucces { get; set; }
        public decimal MontantCommissions { get; set; }
        public decimal MontantFrais { get; set; }
        public decimal NetAPercevoir { get; set; }
    }

    // Objectifs du percepteur
    public class ObjectifPercepteurDto
    {
        public string TypeObjectif { get; set; } = string.Empty;
        public decimal Objectif { get; set; }
        public decimal Realise { get; set; }
        public decimal Atteinte { get; set; }
        public decimal Restant { get; set; }
        public string Periode { get; set; } = string.Empty;
        public decimal ProgressionPrecedente { get; set; }
        public DateTime DateLimite { get; set; }
        public bool EstAtteint { get; set; }
    }

    // Résumé des frais
    public class ResumeFraisDto
    {
        public string TypeFrais { get; set; } = string.Empty;
        public decimal MontantTotal { get; set; }
        public decimal Pourcentage { get; set; }
        public int NombreTransactions { get; set; }
        public decimal MontantMoyen { get; set; }
        public decimal Croissance { get; set; }
    }

    // Graphiques du percepteur
    public class PercepteurGraphsDto
    {
        public List<PerformanceJournaliereDto> PerformancesJournalieres { get; set; } = new();
        public List<ResumeMensuelDto> ResumeMensuels { get; set; } = new();
        public List<TransactionTypeDto> TransactionsParType { get; set; } = new();
        public List<PaiementModeDto> PaiementsParMode { get; set; } = new();
        public List<TendanceTransactionDto> Tendances { get; set; } = new();
        public List<ResumeFraisDto> ResumeFrais { get; set; } = new();
    }

    // Dashboard percepteur complet
    public class DashboardPercepteurDto
    {
        public PercepteurKpisDto Kpis { get; set; } = new();
        public PercepteurGraphsDto Graphs { get; set; } = new();
        public List<TopAgentPercepteurDto> TopAgents { get; set; } = new();
        public List<AgentStatsDto> AgentsStats { get; set; } = new();
        public List<ObjectifPercepteurDto> Objectifs { get; set; } = new();
        public List<PercepteurTransactionDto> TransactionsRecentes { get; set; } = new();
        public DateTime DerniereMiseAJour { get; set; }
        public decimal SoldeAPercevoir { get; set; }
        public decimal MontantEnAttente { get; set; }
        public int TransactionsEnAttente { get; set; }
        public PerceptionRapportSyntheseDto? RapportPerception { get; set; }
    }

    public class PerceptionRapportCanalDto
    {
        public decimal MontantEnAttente { get; set; }
        public int NombreEnAttente { get; set; }
        public decimal MontantPerçu { get; set; }
        public int NombrePerçu { get; set; }
    }

    public class PerceptionRapportSyntheseDto
    {
        public PerceptionRapportCanalDto Agent { get; set; } = new();
        public PerceptionRapportCanalDto Affilie { get; set; } = new();
        public decimal TotalPerçu { get; set; }
        public string? DeviseCode { get; set; }
    }

    public class PerceptionRapportLigneDto
    {
        public string OriginePerception { get; set; } = string.Empty;
        public string StatutPerception { get; set; } = string.Empty;
        public int IdCollecte { get; set; }
        public decimal Montant { get; set; }
        public decimal MontantDevisePrincipale { get; set; }
        public string? DeviseCode { get; set; }
        public int AffilieId { get; set; }
        public string? AffilieNom { get; set; }
        public int? AgentId { get; set; }
        public string? AgentNom { get; set; }
        public string? AgentMatricule { get; set; }
        public string? ModePaiement { get; set; }
        public DateTime DateCollecte { get; set; }
        public DateTime? DatePerception { get; set; }
        public int? PerceptionVirtuelleId { get; set; }
        public int? WalletVirtuelMouvementId { get; set; }
        public string? PercepteurNom { get; set; }
        public string? ReferencePaiement { get; set; }
        public string? Observation { get; set; }
    }

    public class PerceptionRapportResponseDto
    {
        public PerceptionRapportSyntheseDto Synthese { get; set; } = new();
        public PaginatedResponse<PerceptionRapportLigneDto> Lignes { get; set; } = new();
    }
}
