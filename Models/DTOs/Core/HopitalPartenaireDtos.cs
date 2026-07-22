using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProsocAPI.Models.DTOs.Core
{
    public class HopitalPartenaireReadDto
    {
        public int IdHopital { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string? Adresse { get; set; }
        public string? Telephone { get; set; }
        public string? Email { get; set; }
        public string? ContactPersonne { get; set; }
        public string CodeAcces { get; set; } = string.Empty;
        public string? Niveau { get; set; }
        public bool EstActif { get; set; }
        public string? ServicesOfferts { get; set; }
        public decimal? PlafondJournalier { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime? DateModification { get; set; }
        public bool Statut { get; set; }
    }

    public class HopitalPartenaireCreateDto
    {
        [Required]
        [StringLength(200)]
        public string Nom { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Adresse { get; set; }

        [StringLength(100)]
        public string? Telephone { get; set; }

        [StringLength(200)]
        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(100)]
        public string? ContactPersonne { get; set; }

        [Required]
        [StringLength(50)]
        public string CodeAcces { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Niveau { get; set; }

        public bool EstActif { get; set; } = true;

        [StringLength(1000)]
        public string? ServicesOfferts { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? PlafondJournalier { get; set; }
    }

    public class HopitalPartenaireUpdateDto
    {
        [Required]
        [StringLength(200)]
        public string Nom { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Adresse { get; set; }

        [StringLength(100)]
        public string? Telephone { get; set; }

        [StringLength(200)]
        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(100)]
        public string? ContactPersonne { get; set; }

        [Required]
        [StringLength(50)]
        public string CodeAcces { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Niveau { get; set; }

        public bool EstActif { get; set; }

        [StringLength(1000)]
        public string? ServicesOfferts { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? PlafondJournalier { get; set; }

        public bool Statut { get; set; }
    }

    public class HopitalAccesValidationDto
    {
        [Required]
        [StringLength(50)]
        public string CodeAcces { get; set; } = string.Empty;
    }
}
