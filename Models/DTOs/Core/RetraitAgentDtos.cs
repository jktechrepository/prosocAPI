using System.ComponentModel.DataAnnotations;

namespace ProsocAPI.Models.DTOs.Core
{
    // DTOs pour DemandeRetraitAgent
    public class DemandeRetraitAgentReadDto
    {
        public int IdDemande { get; set; }
        public int AgentId { get; set; }
        public string? AgentNom { get; set; }
        public string? AgentMatricule { get; set; }
        public decimal MontantDemande { get; set; }
        public string TypeRetrait { get; set; } = string.Empty;
        public string StatutDemande { get; set; } = string.Empty;
        public string? MotifRetrait { get; set; }
        public string? MotifRejet { get; set; }
        public int? DeviseId { get; set; }
        public string? DeviseCode { get; set; }
        public string? DeviseSymbole { get; set; }
        public DateTime DateDemande { get; set; }
        public DateTime? DateValidation { get; set; }
        public DateTime? DateTraitement { get; set; }
        public int? AgentValidationId { get; set; }
        public string? AgentValidationNom { get; set; }
        public int? JetonRetraitId { get; set; }
        public string? JetonRetraitCode { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime? DateModification { get; set; }
        public bool Statut { get; set; }
    }

    public class DemandeRetraitAgentCreateDto
    {
        [Required]
        public int AgentId { get; set; }

        /// <summary>Obligatoire en fenêtre PARTIEL ; ignoré en fenêtre TOTAL (solde complet).</summary>
        public decimal? MontantDemande { get; set; }

        /// <summary>Optionnel : ignoré si incompatible ; l'API force le type selon la fenêtre courante.</summary>
        [StringLength(20)]
        public string? TypeRetrait { get; set; }

        [StringLength(500)]
        public string? MotifRetrait { get; set; }
    }

    public class DemandeRetraitAgentValidationDto
    {
        [Required]
        public int IdDemande { get; set; }

        [Required]
        [StringLength(20)]
        public string StatutDemande { get; set; } = string.Empty; // "VALIDEE", "REJETEE"

        [StringLength(500)]
        public string? MotifValidation { get; set; }

        [Required]
        public int AgentValidationId { get; set; }
    }

    // DTOs pour JetonRetrait
    public class JetonRetraitReadDto
    {
        public int IdJeton { get; set; }
        public int AgentId { get; set; }
        public string? AgentNom { get; set; }
        public string? AgentMatricule { get; set; }
        public int DemandeRetraitId { get; set; }
        public string CodeJeton { get; set; } = string.Empty;
        public decimal MontantRetrait { get; set; }
        public DateTime DateEmission { get; set; }
        public DateTime? DateUtilisation { get; set; }
        public DateTime DateExpiration { get; set; }
        public bool EstValide { get; set; }
        public bool EstUtilise { get; set; }
        public string? ObservationUtilisation { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime? DateModification { get; set; }
        public bool Statut { get; set; }
    }

    // DTOs pour validation et workflow
    public class PeriodeRetraitVerificationDto
    {
        public DateTime Date { get; set; }
        public bool EstPeriodeAutorisee { get; set; }
        public string Message { get; set; } = string.Empty;
        public int JourDuMois { get; set; }
        public string PeriodeInfo { get; set; } = string.Empty;
    }

    public class PeriodeRetraitCouranteDto
    {
        public DateTime Date { get; set; }
        public bool EstPeriodeAutorisee { get; set; }
        public string Message { get; set; } = string.Empty;
        public int JourDuMois { get; set; }
        public string PeriodeInfo { get; set; } = string.Empty;
        public int Fenetre1Debut { get; set; }
        public int Fenetre1Fin { get; set; }
        public int Fenetre2Debut { get; set; }
        public int Fenetre2Fin { get; set; }
        public string? FenetreActive { get; set; }
        public string? TypeRetraitAutorise { get; set; }
        public decimal MontantMinimumPartiel { get; set; }
        public bool MontantDemandeRequis { get; set; }
    }

    public class SoldeVerificationDto
    {
        public int AgentId { get; set; }
        public string? AgentNom { get; set; }
        public decimal SoldeDisponible { get; set; }
        public decimal MontantDemande { get; set; }
        public bool SoldeSuffisant { get; set; }
        public decimal Difference { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? DeviseId { get; set; }
        public string? DeviseCode { get; set; }
        public string? DeviseSymbole { get; set; }
    }

    public class RetraitWorkflowResultDto
    {
        public bool Succes { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? DemandeId { get; set; }
        public int? JetonId { get; set; }
        public string? JetonCode { get; set; }
        public decimal? MontantRetrait { get; set; }
        public string? TypeRetrait { get; set; }
        public int? DeviseId { get; set; }
        public string? DeviseCode { get; set; }
        public string? DeviseSymbole { get; set; }
        public DateTime? DateEmission { get; set; }
        public DateTime? DateExpiration { get; set; }
    }

    public class DemandeRetraitAgentStatsDto
    {
        public int TotalDemandes { get; set; }
        public int DemandesEnAttente { get; set; }
        public int DemandesValidees { get; set; }
        public int DemandesRejetees { get; set; }
        public int DemandesTraitees { get; set; }
        public decimal TotalMontantDemande { get; set; }
        public decimal TotalMontantTraite { get; set; }
        public decimal TauxValidation { get; set; }
        public DateTime DateStats { get; set; }
    }

    // DTOs pour utilisation du jeton
    public class JetonRetraitUtilisationDto
    {
        [Required]
        public int IdJeton { get; set; }

        [Required]
        [StringLength(20)]
        public string CodeJeton { get; set; } = string.Empty;

        [Required]
        public int AgentId { get; set; }

        [StringLength(500)]
        public string? ObservationUtilisation { get; set; }

        /// <summary>Optionnel — sinon session OUVERTE du caissier connecté.</summary>
        public int? SessionCaisseId { get; set; }
    }

    public class RetraitPaiementResultDto
    {
        public bool Succes { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? CodeErreur { get; set; }
        public int? DemandeId { get; set; }
        public int? JetonId { get; set; }
        public string? JetonCode { get; set; }
        public decimal? MontantPaye { get; set; }
        public decimal? SoldeWalletApres { get; set; }
        public decimal? SoldeCaisseSessionApres { get; set; }
        public int? WalletMouvementId { get; set; }
        public int? MouvementCaisseId { get; set; }
        public int? SessionCaisseId { get; set; }
    }

    // Anciens DTOs conservés pour compatibilité
    public class RetraitAgentReadDto
    {
        public int IdRetraitAgent { get; set; }
        public int AgentId { get; set; }
        public string? AgentNom { get; set; }
        public decimal Montant { get; set; }
        public int? DeviseId { get; set; }
        public string? DeviseNom { get; set; }
        public string? DeviseCode { get; set; }
        public string CodeRetraitPin { get; set; } = string.Empty;
        public DateTime DateDemande { get; set; }
        public bool EstValide { get; set; }
    }

    public class RetraitAgentCreateDto
    {
        [Required]
        public int AgentId { get; set; }

        [Range(0.01, 999999999.99)]
        public decimal Montant { get; set; }

        public int? DeviseId { get; set; }

        [Required]
        [StringLength(20)]
        public string CodeRetraitPin { get; set; } = string.Empty;
    }

    public class RetraitAgentUpdateDto
    {
        [Required]
        public int AgentId { get; set; }

        [Range(0.01, 999999999.99)]
        public decimal Montant { get; set; }

        public int? DeviseId { get; set; }

        [Required]
        [StringLength(20)]
        public string CodeRetraitPin { get; set; } = string.Empty;

        public bool EstValide { get; set; }
    }

    public class RetraitAgentValidationDto
    {
        public bool EstValide { get; set; }
    }
}
