using System.ComponentModel.DataAnnotations;

namespace ProsocAPI.Models.DTOs.Core
{
    public class AffilieReadDto
    {
        public int IdAffilie { get; set; }
        public string CodeAdhesion { get; set; } = string.Empty;
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string? NomComplet { get; set; }
        public DateTime DateNaissance { get; set; }
        public string? Telephone { get; set; }
        public string? Postnom { get; set; }

        [StringLength(150)]
        [EmailAddress]
        public string? EmailAffilie { get; set; }
        public string? ProvinceResidence { get; set; }
        public string? CommuneResidence { get; set; }
        public string? QuartierResidence { get; set; }
        public string? AvenueResidence { get; set; }
        public string? NumeroResidence { get; set; }
        public string? CommuneActivite { get; set; }
        public string? QuartierActivite { get; set; }
        public string? AvenueActivite { get; set; }
        public string? NumeroActivite { get; set; }
        public bool HasPhoto { get; set; }
        public bool HasCarteIdentite { get; set; }
        /// <summary>Contenu photo en base64 (depuis PhotoData).</summary>
        public string? PhotoBase64 { get; set; }
        /// <summary>Alias de <see cref="PhotoBase64"/> pour les clients qui attendent photoUrl.</summary>
        public string? PhotoUrl { get; set; }
        /// <summary>Contenu carte d'identité en base64 (depuis CarteIdentiteData).</summary>
        public string? CarteIdentiteBase64 { get; set; }
        public string? PhotoContentType { get; set; }
        public string? CarteIdentiteContentType { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime? DateModification { get; set; }
        public bool Statut { get; set; }

        public List<DependantReadDto> Dependants { get; set; } = new();
        public List<AntecedentReadDto> Antecedants { get; set; } = new();
        public PersonneContactReadDto? PersonneContact { get; set; }
    }

    public class AffilieCreateDto
    {
        [Required]
        [StringLength(20)]
        public string CodeAdhesion { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Nom { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Prenom { get; set; } = string.Empty;

        [StringLength(200)]
        public string? NomComplet { get; set; }

        public DateTime DateNaissance { get; set; }

        [StringLength(20)]
        public string? Telephone { get; set; }

        [StringLength(100)]
        public string? Postnom { get; set; }

        [StringLength(150)]
        [EmailAddress]
        public string? EmailAffilie { get; set; }

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

        public string? PhotoBase64 { get; set; }

        [StringLength(100)]
        public string? PhotoContentType { get; set; }

        public string? CarteIdentiteBase64 { get; set; }

        [StringLength(100)]
        public string? CarteIdentiteContentType { get; set; }

        public bool Statut { get; set; } = true;
    }

    public class AffilieUpdateDto
    {
        [Required]
        [StringLength(20)]
        public string CodeAdhesion { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Nom { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Prenom { get; set; } = string.Empty;

        [StringLength(200)]
        public string? NomComplet { get; set; }

        public DateTime DateNaissance { get; set; }

        [StringLength(20)]
        public string? Telephone { get; set; }

        [StringLength(100)]
        public string? Postnom { get; set; }

        [StringLength(150)]
        [EmailAddress]
        public string? EmailAffilie { get; set; }

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

        public string? PhotoBase64 { get; set; }

        [StringLength(100)]
        public string? PhotoContentType { get; set; }

        public string? CarteIdentiteBase64 { get; set; }

        [StringLength(100)]
        public string? CarteIdentiteContentType { get; set; }

        public bool Statut { get; set; }
    }
}
