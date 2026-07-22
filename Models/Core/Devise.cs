using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ProsocAPI.Models.Core
{
    public class Devise
    {
        [Key]
        public int IdDevise { get; set; }
        
        [Required, StringLength(10)]
        public string Code { get; set; } = string.Empty; // CDF, USD
        
        [Required, StringLength(100)]
        public string Nom { get; set; } = string.Empty;

        [StringLength(10)]
        public string? Symbole { get; set; }

        /// <summary>Devise principale de consolidation (USD) — une seule active à la fois.</summary>
        public bool EstDevisePrincipale { get; set; }

        public bool Statut { get; set; } = true;

        public DateTime DateCreation { get; set; } = DateTime.Now;

        public DateTime? DateModification { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public virtual ICollection<Collecte> Collectes { get; set; } = new List<Collecte>();

        [JsonIgnore]
        [ValidateNever]
        public virtual ICollection<RetraitAgent> Retraits { get; set; } = new List<RetraitAgent>();

        [JsonIgnore]
        [ValidateNever]
        public virtual ICollection<ProduitMutuel> ProduitsMutuels { get; set; } = new List<ProduitMutuel>();

        [JsonIgnore]
        [ValidateNever]
        public virtual ICollection<ProduitAssureur> ProduitsAssureurs { get; set; } = new List<ProduitAssureur>();

        [JsonIgnore]
        [ValidateNever]
        public virtual ICollection<Frais> Frais { get; set; } = new List<Frais>();

        [JsonIgnore]
        [ValidateNever]
        public virtual ICollection<TarifCotisation> TarifsCotisation { get; set; } = new List<TarifCotisation>();
    }
}
