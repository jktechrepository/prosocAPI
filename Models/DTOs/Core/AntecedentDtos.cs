using System.ComponentModel.DataAnnotations;

namespace ProsocAPI.Models.DTOs.Core
{
    public class AntecedentReadDto
    {
        public int IdAntecedant { get; set; }
        public string Description { get; set; } = string.Empty;
        public int AffilieId { get; set; }
        public string? AffilieNom { get; set; }
        public int? DependantId { get; set; }
        public string? DependantNom { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime? DateModification { get; set; }
        public bool Statut { get; set; }
    }

    public class AntecedentCreateDto
    {
        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int AffilieId { get; set; }

        public int? DependantId { get; set; }

        public bool Statut { get; set; } = true;
    }

    public class AntecedentUpdateDto
    {
        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int AffilieId { get; set; }

        public int? DependantId { get; set; }

        public bool Statut { get; set; }
    }
}
