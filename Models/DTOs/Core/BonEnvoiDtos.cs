using System.ComponentModel.DataAnnotations;

namespace ProsocAPI.Models.DTOs.Core
{
    public class BonEnvoiReadDto
    {
        public int IdBonEnvoi { get; set; }
        public string NumeroBon { get; set; } = string.Empty;
        public int AffilieId { get; set; }
        public string? AffilieNom { get; set; }
        public int PrestationId { get; set; }
        public string? PrestationNom { get; set; }
        public int? JetonMedicalId { get; set; }
        public string? JetonMedicalCode { get; set; }
        public DateTime DateEmission { get; set; }
        public DateTime? DateUtilisation { get; set; }
        public bool EstUtilise { get; set; }
        public bool Statut { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime? DateModification { get; set; }
        public string? QrCodePayload { get; set; }
        public string? QrCodeImageBase64 { get; set; }
    }

    public class BonEnvoiCreateDto
    {
        [Required]
        [StringLength(50)]
        public string NumeroBon { get; set; } = string.Empty;

        [Required]
        public int AffilieId { get; set; }

        [Required]
        public int PrestationId { get; set; }

        public bool Statut { get; set; } = true;
    }

    public class BonEnvoiUpdateDto
    {
        [Required]
        [StringLength(50)]
        public string NumeroBon { get; set; } = string.Empty;

        [Required]
        public int AffilieId { get; set; }

        [Required]
        public int PrestationId { get; set; }

        public DateTime? DateUtilisation { get; set; }
        public bool EstUtilise { get; set; }
        public bool Statut { get; set; }
    }

    public class BonEnvoiUtilisationDto
    {
        public bool EstUtilise { get; set; }
        public DateTime? DateUtilisation { get; set; }
    }

    /// <summary>Contenu lu après scan du QR (envoyé par l'app mobile).</summary>
    public class BonEnvoiScanRequestDto
    {
        [Required]
        public string QrCodePayload { get; set; } = string.Empty;

        /// <summary>Marque le bon comme utilisé après vérification réussie.</summary>
        public bool MarquerUtilise { get; set; }
    }

    public class BonEnvoiScanResultDto
    {
        public bool Valide { get; set; }
        public string Message { get; set; } = string.Empty;
        public BonEnvoiReadDto? Bon { get; set; }
        public int? DemandeId { get; set; }
        public string? AffilieMatricule { get; set; }
        public string? JetonMedicalCode { get; set; }
    }
}
