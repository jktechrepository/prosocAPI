using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using ProsocAPI.Models.Authentication;

namespace ProsocAPI.Models.Core
{
    /// <summary>
    /// Représente les frais applicables aux transactions
    /// </summary>
    public class Frais
    {
        [Key]
        public int IdFrais { get; set; }
        
        /// <summary>Identifiant métier stable (ex. PENALITE_RETARD_COTISATION).</summary>
        [StringLength(50)]
        public string? Code { get; set; }

        [Required]
        [StringLength(100)]
        public string Libelle { get; set; } = string.Empty;
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public double Montant { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal TauxCommission { get; set; }

        /// <summary>Ponctuel, Mensuel ou Annuel</summary>
        [Required, StringLength(20)]
        public string Periodicite { get; set; } = "Ponctuel";
        
        [Required]
        public int DeviseId { get; set; }
        
        // Navigation properties
        [ForeignKey("DeviseId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Devise? Devise { get; set; }
        
        // Métadonnées
        public DateTime DateCreation { get; set; } = DateTime.Now;
        public DateTime? DateModification { get; set; }
        public bool Statut { get; set; } = true;
        
        // Métadonnées d'audit
        public int? CreeParId { get; set; }
        public int? ModifieParId { get; set; }
        public DateTime? DateSuppression { get; set; }
        public bool EstSupprime { get; set; } = false;
        
        // Navigation pour l'audit
        [ForeignKey("CreeParId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Utilisateur? CreePar { get; set; }
        
        [ForeignKey("ModifieParId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Utilisateur? ModifiePar { get; set; }
        
        // NOUVEAU : Collection de collectes associées
        [JsonIgnore]
        [ValidateNever]
        public virtual ICollection<Collecte> Collectes { get; set; } = new List<Collecte>();
    }
}
