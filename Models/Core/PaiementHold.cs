using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProsocAPI.Models.Core
{
    public class PaiementHold
    {
        [Key]
        public int IdPaiementHold { get; set; }

        public Guid IdCollecteEnAttente { get; set; }

        public int? AffilieId { get; set; }

        public TypeCollecte TypeCollecte { get; set; }

        public int Mois { get; set; }

        public int Annee { get; set; }

        public int? FraisId { get; set; }

        public int? SouscriptionPrestationId { get; set; }

        public int? CotisationAffilieId { get; set; }

        [MaxLength(30)]
        public string? TelephoneAffilie { get; set; }

        public DateTime ExpireAt { get; set; }

        public DateTime DateCreation { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(IdCollecteEnAttente))]
        public virtual CollecteEnAttente? CollecteEnAttente { get; set; }
    }
}
