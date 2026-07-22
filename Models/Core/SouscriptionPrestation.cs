using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ProsocAPI.Models.Core
{
    public class SouscriptionPrestation
    {
        [Key]
        public int IdSouscriptionPrestation { get; set; }
        
        public int AffilieId { get; set; }
        
        public int PrestationId { get; set; }
        
        public DateTime DateSouscription { get; set; } = DateTime.Now;
        
        public bool Statut { get; set; } = true;

        public DateTime DateCreation { get; set; } = DateTime.Now;

        public DateTime? DateModification { get; set; }

        [ForeignKey("AffilieId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Affilie Affilie { get; set; } = null!;

        [ForeignKey("PrestationId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Prestation Prestation { get; set; } = null!;

        [JsonIgnore]
        [ValidateNever]
        public virtual ICollection<Collecte> Collectes { get; set; } = new List<Collecte>();
    }
}
