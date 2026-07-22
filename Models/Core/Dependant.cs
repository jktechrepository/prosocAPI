using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ProsocAPI.Models.Core
{
    public class Dependant
    {
        [Key]
        public int IdDependant { get; set; }
        
        [Required, StringLength(200)]
        public string Nom { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Adresse { get; set; }
        
        [StringLength(50)]
        public string LienParente { get; set; } = string.Empty;
        
        public DateTime? DateNaissance { get; set; }
        
        [StringLength(20)]
        public string? Telephone { get; set; }

        [Column(TypeName = "longblob")]
        [JsonIgnore]
        public byte[]? CertificatScolariteData { get; set; }

        [StringLength(100)]
        public string? CertificatScolariteContentType { get; set; }
        
        public int AffilieId { get; set; }

        public DateTime DateCreation { get; set; } = DateTime.Now;

        public DateTime? DateModification { get; set; }

        public bool Statut { get; set; } = true;

        [ForeignKey("AffilieId")]
        [InverseProperty("Dependants")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Affilie Affilie { get; set; } = null!;

        [InverseProperty("Dependant")]
        [JsonIgnore]
        [ValidateNever]
        public virtual ICollection<Antecedant> Antecedants { get; set; } = new List<Antecedant>();
    }
}
