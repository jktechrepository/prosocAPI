using System.ComponentModel.DataAnnotations;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.Pagination;

namespace ProsocAPI.Models.DTOs.Core
{
    public class PerceptionVirtuelleConfirmerDto
    {
        [Required]
        public int AgentId { get; set; }

        [Required]
        [MinLength(1)]
        public List<int> CollecteIds { get; set; } = new();

        [StringLength(500)]
        public string? Observation { get; set; }
    }

    public class PerceptionVirtuelleAnnulerDto
    {
        [Required]
        [StringLength(500, MinimumLength = 1)]
        public string Motif { get; set; } = string.Empty;
    }

    public class PerceptionVirtuelleConfirmerResultDto
    {
        public bool Succes { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? CodeErreur { get; set; }
        public int? PerceptionVirtuelleId { get; set; }
        public decimal? MontantTotal { get; set; }
        public int? NombreCollectes { get; set; }
        public decimal? SoldeRestantAgent { get; set; }
    }

    public class CollecteVirtuelleEnAttenteDto
    {
        public int IdCollecte { get; set; }
        public int? AgentId { get; set; }
        public int AgentIdEffectif { get; set; }
        public string? AgentNom { get; set; }
        public string? AgentMatricule { get; set; }
        public int AffilieId { get; set; }
        public string? AffilieNom { get; set; }
        public decimal Montant { get; set; }
        public decimal MontantDevisePrincipale { get; set; }
        public string? DeviseCode { get; set; }
        public DateTime DateCollecte { get; set; }
        public string? TypeCollecte { get; set; }
        public string? ReferencePaiement { get; set; }
        public string StatutPerception { get; set; } = string.Empty;
    }

    public class PerceptionVirtuelleSyntheseAgentDto
    {
        public int AgentId { get; set; }
        public string? AgentNom { get; set; }
        public string? AgentMatricule { get; set; }
        public int NombreCollectesEnAttente { get; set; }
        public decimal MontantEnAttente { get; set; }
        public string? DeviseCode { get; set; }
    }

    public class PerceptionVirtuelleLigneReadDto
    {
        public int IdLigne { get; set; }
        public int CollecteId { get; set; }
        public int AgentId { get; set; }
        public decimal Montant { get; set; }
        public int? WalletVirtuelMouvementId { get; set; }
        public string? AffilieNom { get; set; }
        public DateTime? DateCollecte { get; set; }
    }

    public class PerceptionVirtuelleReadDto
    {
        public int IdPerceptionVirtuelle { get; set; }
        public int AgentId { get; set; }
        public string? AgentNom { get; set; }
        public string? AgentMatricule { get; set; }
        public int PercepteurUtilisateurId { get; set; }
        public string? PercepteurNom { get; set; }
        public decimal MontantTotal { get; set; }
        public int DeviseId { get; set; }
        public string? DeviseCode { get; set; }
        public int NombreCollectes { get; set; }
        public DateTime DatePerception { get; set; }
        public string? Observation { get; set; }
        public string StatutMetier { get; set; } = PerceptionVirtuelleStatuts.Confirmee;
        public string? MotifAnnulation { get; set; }
        public DateTime? DateAnnulation { get; set; }
        public int? AnnuleParUtilisateurId { get; set; }
        public string? AnnuleParNom { get; set; }
        public List<PerceptionVirtuelleLigneReadDto> Lignes { get; set; } = new();
    }

    public class CollectesVirtuellesEnAttenteFiltreDto
    {
        public int? AgentId { get; set; }
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
        public PaginationRequest Pagination { get; set; } = new();
    }

    public class PerceptionVirtuelleHistoriqueFiltreDto
    {
        public int? PercepteurUtilisateurId { get; set; }
        public int? AgentId { get; set; }
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
    }

    public class PerceptionReconciliationAnomaliesDto
    {
        public int CollectesPercuSansJournal { get; set; }
        public int DebitsSansCollecte { get; set; }
        public int CollectesVaSansDebit { get; set; }
    }

    public class PerceptionReconciliationDto
    {
        public string? DeviseCode { get; set; }
        public int? AgentId { get; set; }
        public decimal MontantDebitWallet { get; set; }
        public decimal MontantCollectesVaValides { get; set; }
        public decimal MontantNonPerçu { get; set; }
        public decimal MontantPerçuTerrain { get; set; }
        public int NombreNonPerçu { get; set; }
        public int NombrePerçu { get; set; }
        public PerceptionReconciliationAnomaliesDto Anomalies { get; set; } = new();
    }
}
