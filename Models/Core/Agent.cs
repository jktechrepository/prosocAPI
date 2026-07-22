using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ProsocAPI.Models.Core
{
    public class Agent
    {
        [Key]
        public int IdAgent { get; set; }
        
        [Required, StringLength(200)]
        public string NomComplet { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Matricule { get; set; } = string.Empty;

        [Required, StringLength(20)]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [StringLength(200)]
        public string? EmailAgent { get; set; }

        [StringLength(100)]
        public string? Fonction { get; set; }

        [StringLength(100)]
        public string? RoleAgent { get; set; }

        [StringLength(500)]
        public string? PhotoUrl { get; set; }

        public DateTime DateCreation { get; set; } = DateTime.Now;

        public DateTime? DateModification { get; set; }

        public bool Statut { get; set; } = true;

        public int? CategorieAgentId { get; set; }

        [ForeignKey("CategorieAgentId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual CategorieAgent? CategorieAgent { get; set; }
        
        public int? ZoneSocialeId { get; set; }
        
        [ForeignKey("ZoneSocialeId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual ZoneSociale? Zone { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public virtual ICollection<WalletAgent> Wallets { get; set; } = new List<WalletAgent>();

        [JsonIgnore]
        [ValidateNever]
        public virtual WalletVirtuelAgent? WalletVirtuel { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public virtual ICollection<Adhesion> AdhesionsCrees { get; set; } = new List<Adhesion>();

        [JsonIgnore]
        [ValidateNever]
        public virtual ICollection<Collecte> Collectes { get; set; } = new List<Collecte>();

        [JsonIgnore]
        [ValidateNever]
        public virtual ICollection<RetraitAgent> Retraits { get; set; } = new List<RetraitAgent>();
    }
}
