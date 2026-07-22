using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using ProsocAPI.Models.Authentication;

namespace ProsocAPI.Models.Core
{
    public class SessionCaisse
    {
        [Key]
        public int IdSessionCaisse { get; set; }

        [Required]
        public int UtilisateurId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SoldeOuverture { get; set; }

        [Required]
        public int DeviseId { get; set; }

        [Required]
        [StringLength(20)]
        public string Statut { get; set; } = SessionCaisseStatut.Ouverte;

        public DateTime DateOuverture { get; set; } = DateTime.Now;

        public DateTime? DateCloture { get; set; }

        [StringLength(500)]
        public string? ObservationCloture { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? SoldeTheoriqueCloture { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? SoldeReelCloture { get; set; }

        public DateTime DateCreation { get; set; } = DateTime.Now;

        public DateTime? DateModification { get; set; }

        public bool StatutActif { get; set; } = true;

        [ForeignKey("UtilisateurId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Utilisateur Utilisateur { get; set; } = null!;

        [ForeignKey("DeviseId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Devise Devise { get; set; } = null!;

        [JsonIgnore]
        [ValidateNever]
        public virtual ICollection<MouvementCaisse> Mouvements { get; set; } = new List<MouvementCaisse>();
    }
}
