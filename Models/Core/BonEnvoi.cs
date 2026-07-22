using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ProsocAPI.Models.Core
{
    public class BonEnvoi
    {
        [Key]
        public int IdBonEnvoi { get; set; }
        
        [Required, StringLength(50)]
        public string NumeroBon { get; set; } = string.Empty;
        
        public int AffilieId { get; set; }
        
        public int PrestationId { get; set; }

        /// <summary>Lien direct vers le jeton (nullable pour les bons historiques avant backfill R1/R2).</summary>
        public int? JetonMedicalId { get; set; }
        
        public DateTime DateEmission { get; set; } = DateTime.Now;
        
        public DateTime? DateUtilisation { get; set; }
        
        public bool EstUtilise { get; set; } = false;
        
        public bool Statut { get; set; } = true;

        /// <summary>Contenu encodé dans le QR (JSON signé).</summary>
        [StringLength(2000)]
        public string? QrCodePayload { get; set; }

        /// <summary>Image PNG du QR code en base64.</summary>
        public string? QrCodeImageBase64 { get; set; }

        public DateTime DateCreation { get; set; } = DateTime.Now;

        public DateTime? DateModification { get; set; }

        [ForeignKey("AffilieId")]
        [InverseProperty("BonsEnvoi")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Affilie Affilie { get; set; } = null!;

        [ForeignKey("PrestationId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Prestation Prestation { get; set; } = null!;

        [ForeignKey("JetonMedicalId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual JetonMedical? JetonMedical { get; set; }
    }
}
