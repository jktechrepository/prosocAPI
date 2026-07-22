using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ProsocAPI.Models.Core
{
    public class HopitalPartenaire
    {
        [Key]
        public int IdHopital { get; set; }
        
        [Required, StringLength(200)]
        public string Nom { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Adresse { get; set; }
        
        [StringLength(100)]
        public string? Telephone { get; set; }
        
        [StringLength(200)]
        public string? Email { get; set; }
        
        [StringLength(100)]
        public string? ContactPersonne { get; set; }
        
        [Required, StringLength(50)]
        public string CodeAcces { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string? Niveau { get; set; } // Primaire, Secondaire, Tertiaire
        
        [Required]
        public bool EstActif { get; set; } = true;
        
        [StringLength(1000)]
        public string? ServicesOfferts { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? PlafondJournalier { get; set; }
        
        public DateTime DateCreation { get; set; } = DateTime.Now;
        
        public DateTime? DateModification { get; set; }
        
        public bool Statut { get; set; } = true;
        
        [JsonIgnore]
        [ValidateNever]
        public virtual ICollection<JetonMedical> JetonsEmis { get; set; } = new List<JetonMedical>();
        
        [JsonIgnore]
        [ValidateNever]
        public virtual ICollection<Prestation> PrestationsAutorisees { get; set; } = new List<Prestation>();
    }
}
