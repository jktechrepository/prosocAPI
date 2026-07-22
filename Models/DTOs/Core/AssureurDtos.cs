using System.ComponentModel.DataAnnotations;

namespace ProsocAPI.Models.DTOs.Core
{
    public class AssureurReadDto
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool Statut { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime? DateModification { get; set; }
        public int NombreProduits { get; set; }
    }

    public class AssureurCreateDto
    {
        [Required]
        [StringLength(200)]
        public string Nom { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public bool Statut { get; set; } = true;
    }

    public class AssureurUpdateDto
    {
        [Required]
        [StringLength(200)]
        public string Nom { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public bool Statut { get; set; }
    }
}
