using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProsocAPI.Models.Core
{
    public abstract class ProduitBase
    {
        [Key]
        public int IdProduit { get; set; }

        [Required, StringLength(200)]
        public string Nom { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Montant { get; set; }

        /// <summary>Mensuel ou Annuel</summary>
        [Required, StringLength(20)]
        public string Periodicite { get; set; } = "Mensuel";

        public int AgeMin { get; set; }

        public int AgeMax { get; set; }

        /// <summary>Produit inclus dans la cotisation (montant 0, pas de commission AT à la souscription).</summary>
        public bool EstGratuit { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal TauxCommissionAT { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal TauxCommissionAA { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal TauxCommissionAAMash { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal TauxCommissionAAStructure { get; set; }

        public bool Statut { get; set; } = true;

        public DateTime DateCreation { get; set; } = DateTime.Now;
    }
}
