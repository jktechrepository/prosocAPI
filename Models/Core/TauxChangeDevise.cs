using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ProsocAPI.Models.Core
{
    /// <summary>
    /// Taux de change directionnel entre deux devises, historisé par date d'effet.
    /// Exemple : 1 USD = 2850 CDF → DeviseSource=USD, DeviseCible=CDF, Taux=2850.
    /// </summary>
    public class TauxChangeDevise
    {
        [Key]
        public int IdTauxChangeDevise { get; set; }

        public int DeviseSourceId { get; set; }

        public int DeviseCibleId { get; set; }

        [Column(TypeName = "decimal(18,6)")]
        public decimal Taux { get; set; }

        public DateTime DateEffet { get; set; } = DateTime.UtcNow;

        public bool Statut { get; set; } = true;

        public DateTime DateCreation { get; set; } = DateTime.Now;

        public DateTime? DateModification { get; set; }

        [ForeignKey("DeviseSourceId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Devise DeviseSource { get; set; } = null!;

        [ForeignKey("DeviseCibleId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Devise DeviseCible { get; set; } = null!;
    }
}
