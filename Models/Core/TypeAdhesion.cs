using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ProsocAPI.Models.Core
{
    public class TypeAdhesion
    {
        [Key]
        public int IdTypeAdhesion { get; set; }  // ✅ Standardisé
        
        [Required, StringLength(50)]
        public string Libelle { get; set; } = string.Empty; // Solo, F3, F6
        
        public int MaxDependants { get; set; }
        
        [StringLength(255)]
        public string? Description { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Montant { get; set; }

        public int DeviseId { get; set; }
        
        public bool Statut { get; set; } = true;
        
        public DateTime DateCreation { get; set; } = DateTime.Now;

        public DateTime? DateModification { get; set; }

        public int CategorieAdhesionId { get; set; }

        [ForeignKey("CategorieAdhesionId")]
        [JsonIgnore]
        [ValidateNever]
        public CategorieAdhesion CategorieAdhesion { get; set; } = null!;

        [ForeignKey("DeviseId")]
        [JsonIgnore]
        [ValidateNever]
        public Devise Devise { get; set; } = null!;

        // Navigation properties
        [JsonIgnore]
        [ValidateNever]
        public virtual ICollection<Adhesion> Adhesions { get; set; } = new List<Adhesion>();

        [JsonIgnore]
        [ValidateNever]
        public virtual ICollection<TarifCotisation> CotisationsAffilie { get; set; } = new List<TarifCotisation>();

        [NotMapped]
        public ICollection<TarifCotisation> TarifsCotisation => CotisationsAffilie;
    }
}
