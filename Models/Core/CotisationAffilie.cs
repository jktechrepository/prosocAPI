using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ProsocAPI.Models.Core
{
    /// <summary>
    /// Grille tarifaire de cotisation affilié (mensuelle ou annuelle), par type d'adhésion.
    /// Le montant est unitaire par personne ; le total famille se calcule côté métier.
    /// </summary>
    public class TarifCotisation
    {
        [Key]
        public int IdCotisationAffilie { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Montant { get; set; }

        /// <summary>Mensuel ou Annuel</summary>
        [Required, StringLength(20)]
        public string Periodicite { get; set; } = string.Empty;

        public int TypeAdhesionId { get; set; }

        public int DeviseId { get; set; }

        [StringLength(255)]
        public string? LibelleTarifCotisation { get; set; }

        [StringLength(255)]
        public string? LibelleTarifCotisationNormalized { get; set; }

        public bool Statut { get; set; } = true;

        public DateTime DateCreation { get; set; } = DateTime.Now;

        public DateTime? DateModification { get; set; }

        [ForeignKey("TypeAdhesionId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual TypeAdhesion TypeAdhesion { get; set; } = null!;

        [ForeignKey("DeviseId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Devise Devise { get; set; } = null!;

        [JsonIgnore]
        [ValidateNever]
        public virtual ICollection<Collecte> Collectes { get; set; } = new List<Collecte>();
    }

    [NotMapped]
    [Obsolete("Use TarifCotisation instead.")]
    public class CotisationAffilie : TarifCotisation
    {
    }
}
