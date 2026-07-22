using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ProsocAPI.Models.Core
{
    public class Affilie
    {
        [Key]
        public int IdAffilie { get; set; }
        
        [Required, StringLength(20)]
        public string CodeAdhesion { get; set; } = string.Empty;
        
        [Required, StringLength(100)]
        public string Nom { get; set; } = string.Empty;
        
        [Required, StringLength(100)]
        public string Prenom { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string NomComplet { get; set; } = string.Empty;
        
        public DateTime DateNaissance { get; set; }
        
        [StringLength(20)]
        public string? Telephone { get; set; }

        [StringLength(150)]
        [EmailAddress]
        public string? EmailAffilie { get; set; }

        [StringLength(100)]
        public string? Postnom { get; set; }

        [StringLength(100)]
        public string? ProvinceResidence { get; set; }

        [StringLength(100)]
        public string? CommuneResidence { get; set; }

        [StringLength(100)]
        public string? QuartierResidence { get; set; }

        [StringLength(100)]
        public string? AvenueResidence { get; set; }

        [StringLength(50)]
        public string? NumeroResidence { get; set; }

        [StringLength(100)]
        public string? CommuneActivite { get; set; }

        [StringLength(100)]
        public string? QuartierActivite { get; set; }

        [StringLength(100)]
        public string? AvenueActivite { get; set; }

        [StringLength(50)]
        public string? NumeroActivite { get; set; }

        [Column(TypeName = "longblob")]
        [JsonIgnore]
        public byte[]? PhotoData { get; set; }

        [StringLength(100)]
        public string? PhotoContentType { get; set; }

        [Column(TypeName = "longblob")]
        [JsonIgnore]
        public byte[]? CarteIdentiteData { get; set; }

        [StringLength(100)]
        public string? CarteIdentiteContentType { get; set; }

        public DateTime DateCreation { get; set; } = DateTime.Now;

        public DateTime? DateModification { get; set; }

        public bool Statut { get; set; } = true;

        [JsonIgnore]
        [ValidateNever]
        public Adhesion? Adhesion { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public virtual ICollection<Adhesion> Adhesions { get; set; } = new List<Adhesion>();

        [JsonIgnore]
        [ValidateNever]
        public virtual ICollection<JetonMedical> JetonsMedicaux { get; set; } = new List<JetonMedical>();
        
        [JsonIgnore]
        [ValidateNever]
        public virtual ICollection<Dependant> Dependants { get; set; } = new List<Dependant>();

        [JsonIgnore]
        [ValidateNever]
        public virtual PersonneContact? PersonneContact { get; set; }
        
        [JsonIgnore]
        [ValidateNever]
        public virtual ICollection<SouscriptionPrestation> Souscriptions { get; set; } = new List<SouscriptionPrestation>();
        
        [JsonIgnore]
        [ValidateNever]
        public virtual ICollection<Antecedant> Antecedants { get; set; } = new List<Antecedant>();

        [JsonIgnore]
        [ValidateNever]
        public virtual ICollection<BonEnvoi> BonsEnvoi { get; set; } = new List<BonEnvoi>();

        [JsonIgnore]
        [ValidateNever]
        public virtual ICollection<Collecte> Collectes { get; set; } = new List<Collecte>();
    }
}
