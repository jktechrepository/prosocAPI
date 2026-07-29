using System.ComponentModel.DataAnnotations;

namespace ProsocAPI.Models.DTOs.Core
{
    public class DemandeRechargeWalletVirtuelCreateDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int AgentId { get; set; }

        [StringLength(500)]
        public string? Motif { get; set; }
    }

    public class DemandeRechargeWalletVirtuelRejeterDto
    {
        [Required]
        [StringLength(500, MinimumLength = 1)]
        public string Motif { get; set; } = string.Empty;
    }

    public class DemandeRechargeWalletVirtuelReadDto
    {
        public int IdDemande { get; set; }
        public int AgentId { get; set; }
        public string? AgentNom { get; set; }
        public string? AgentMatricule { get; set; }
        public decimal MontantCalcule { get; set; }
        public decimal SoldeAuMomentDemande { get; set; }
        public decimal PlafondAuMomentDemande { get; set; }
        public string StatutDemande { get; set; } = string.Empty;
        public string? Motif { get; set; }
        public string? MotifRejet { get; set; }
        public DateTime DateDemande { get; set; }
        public DateTime? DateConfirmation { get; set; }
        public DateTime? DateRejet { get; set; }
        public int DemandeParUtilisateurId { get; set; }
        public string? DemandeParNom { get; set; }
        public int? ConfirmeParUtilisateurId { get; set; }
        public string? ConfirmeParNom { get; set; }
        public int? RejeteParUtilisateurId { get; set; }
        public string? RejeteParNom { get; set; }
        public int? WalletVirtuelMouvementId { get; set; }
        public decimal? MontantCredite { get; set; }
        public decimal? SoldeAvantCredit { get; set; }
        public decimal? SoldeApresCredit { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime? DateModification { get; set; }
        public bool Statut { get; set; }
    }

    public class DemandeRechargeWalletVirtuelOperationResultDto
    {
        public bool Success { get; set; }
        public string? CodeErreur { get; set; }
        public string? Message { get; set; }
        public bool Conflict { get; set; }
        public bool Forbidden { get; set; }
        public DemandeRechargeWalletVirtuelReadDto? Demande { get; set; }
    }
}
