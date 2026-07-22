using System.ComponentModel.DataAnnotations;

namespace ProsocAPI.Models.DTOs.Core
{
    public class CategorieAdhesionReadDto
    {
        public int IdCategorieAdhesion { get; set; }
        public string Libelle { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool Statut { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime? DateModification { get; set; }
        public int NombreAdhesions { get; set; }
    }

    public class CategorieAdhesionCreateDto
    {
        [Required]
        [StringLength(200)]
        public string Libelle { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public bool Statut { get; set; } = true;
    }

    public class CategorieAdhesionUpdateDto
    {
        [Required]
        [StringLength(200)]
        public string Libelle { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public bool Statut { get; set; }
    }
}
