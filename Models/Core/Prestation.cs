using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ProsocAPI.Models.Core
{
    public class Prestation
    {
        [Key]
        public int IdPrestation { get; set; }  // ✅ Standardisé
        
        [Required, StringLength(200)]
        public string NomPrestation { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Description { get; set; }

        /// <summary>Périodicité appliquée à la prestation (ex. Mensuel, Annuel).</summary>
        [Required, StringLength(20)]
        public string Periodicite { get; set; } = "Mensuel";
        
        // 🆕 NOUVEAUX CHAMPS
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Le montant doit être supérieur à 0")]
        public decimal Montant { get; set; }
        
        [Required]
        [ForeignKey("DeviseId")]
        public int DeviseId { get; set; }
        
        // 🆕 NAVIGATION PROPERTY
        [ForeignKey("DeviseId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Devise? Devise { get; set; }
        
        // CHAMPS EXISTANTS
        public int? ProduitMutuelId { get; set; }
        
        public int? ProduitAssureurId { get; set; }

        public DateTime DateCreation { get; set; } = DateTime.Now;

        public DateTime? DateModification { get; set; }

        public bool Statut { get; set; } = true;

        // NAVIGATION PROPERTIES EXISTANTES
        [ForeignKey("ProduitMutuelId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual ProduitMutuel? ProduitMutuel { get; set; }

        [ForeignKey("ProduitAssureurId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual ProduitAssureur? ProduitAssureur { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public virtual ICollection<SouscriptionPrestation> Souscriptions { get; set; } = new List<SouscriptionPrestation>();

        [JsonIgnore]
        [ValidateNever]
        public virtual ICollection<BonEnvoi> BonsEnvoi { get; set; } = new List<BonEnvoi>();

        [JsonIgnore]
        [ValidateNever]
        public virtual ICollection<Collecte> Collectes { get; set; } = new List<Collecte>();
    }
}
