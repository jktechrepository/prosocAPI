using System.ComponentModel.DataAnnotations;

namespace ProsocAPI.Models.DTOs.Core
{
    public class DemandeBonEnvoiReadDto
    {
        public int IdDemande { get; set; }
        public int AffilieId { get; set; }
        public string? AffilieNom { get; set; }
        public int PrestationId { get; set; }
        public string? PrestationNom { get; set; }
        public string? MotifDemande { get; set; }
        public int? AgentId { get; set; }
        public string? AgentNom { get; set; }
        public string? ObservationAgent { get; set; }
        public DateTime DateDemande { get; set; }
        public DateTime? DateValidation { get; set; }
        public string StatutDemande { get; set; } = string.Empty;
        public int? BonEnvoiId { get; set; }
        public string? BonEnvoiNumero { get; set; }
        public int? JetonMedicalId { get; set; }
        public string? JetonMedicalCode { get; set; }
        public string? QrCodePayload { get; set; }
        public string? QrCodeImageBase64 { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime? DateModification { get; set; }
        public bool Statut { get; set; }
    }

    public class DemandeBonEnvoiCreateDto
    {
        [Required]
        public int AffilieId { get; set; }

        [Required]
        public int PrestationId { get; set; }

        [StringLength(500)]
        public string? MotifDemande { get; set; }

        /// <summary>Optionnel à la création par l'affilié ; renseigné à la confirmation agent.</summary>
        public int? AgentId { get; set; }

        [StringLength(500)]
        public string? ObservationAgent { get; set; }
    }

    /// <summary>Confirmation d'une demande par un agent (validation ou rejet + génération bon/QR).</summary>
    public class DemandeBonEnvoiConfirmerDto
    {
        [Required]
        public int AgentId { get; set; }

        /// <summary>true = valider et générer bon + jeton + QR ; false = rejeter.</summary>
        [Required]
        public bool Accepter { get; set; }

        /// <summary>Obligatoire si Accepter = true.</summary>
        public int? HopitalPartenaireId { get; set; }

        [StringLength(500)]
        public string? ObservationAgent { get; set; }

        [StringLength(500)]
        public string? MotifRejet { get; set; }
    }

    public class DemandeBonEnvoiConfirmationResultDto
    {
        public bool Succes { get; set; }
        public string Message { get; set; } = string.Empty;
        public int IdDemande { get; set; }
        public string StatutDemande { get; set; } = string.Empty;
        public int? BonEnvoiId { get; set; }
        public string? BonEnvoiNumero { get; set; }
        public string? QrCodePayload { get; set; }
        public string? QrCodeImageBase64 { get; set; }
        public int? JetonMedicalId { get; set; }
        public string? JetonMedicalCode { get; set; }
    }

    public class DemandeBonEnvoiValidationDto
    {
        [Required]
        public int IdDemande { get; set; }

        [Required]
        [StringLength(20)]
        public string StatutDemande { get; set; } = string.Empty; // "VALIDEE", "REJETEE"

        [StringLength(500)]
        public string? MotifValidation { get; set; }

        [Required]
        public int AgentId { get; set; } // Agent qui valide
    }

    public class DemandeBonEnvoiGenerationDto
    {
        [Required]
        public int IdDemande { get; set; }

        [Required]
        public int AgentId { get; set; }

        [Required]
        public int HopitalPartenaireId { get; set; }

        [StringLength(500)]
        public string? ObservationGeneration { get; set; }
    }

    public class DemandeBonEnvoiStatsDto
    {
        public int TotalDemandes { get; set; }
        public int DemandesEnAttente { get; set; }
        public int DemandesValidees { get; set; }
        public int DemandesRejetees { get; set; }
        public int BonsGeneres { get; set; }
        public int JetonsGeneres { get; set; }
        public decimal TauxValidation { get; set; }
        public DateTime DateStats { get; set; }
    }
}
