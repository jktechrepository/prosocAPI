using System.ComponentModel.DataAnnotations;

namespace ProsocAPI.Models.DTOs.Core
{
    public class TypeAdhesionReadDto
    {
        public int Id { get; set; }
        public string Libelle { get; set; } = string.Empty;
        public int MaxDependants { get; set; }
        public string? Description { get; set; }
        public decimal Montant { get; set; }
        public int DeviseId { get; set; }
        public bool Statut { get; set; }
        public DateTime DateCreation { get; set; }
        public int CategorieAdhesionId { get; set; }
    }

    public class TypeAdhesionCreateDto
    {
        [Required]
        [StringLength(50)]
        public string Libelle { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int CategorieAdhesionId { get; set; }

        [Range(0, int.MaxValue)]
        public int MaxDependants { get; set; }

        [StringLength(255)]
        public string? Description { get; set; }

        [Range(0, 999999999.99)]
        public decimal Montant { get; set; }

        [Range(1, int.MaxValue)]
        public int DeviseId { get; set; }

        public bool Statut { get; set; } = true;
    }

    public class TypeAdhesionUpdateDto
    {
        [Required]
        [StringLength(50)]
        public string Libelle { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int CategorieAdhesionId { get; set; }

        [Range(0, int.MaxValue)]
        public int MaxDependants { get; set; }

        [StringLength(255)]
        public string? Description { get; set; }

        [Range(0, 999999999.99)]
        public decimal Montant { get; set; }

        [Range(1, int.MaxValue)]
        public int DeviseId { get; set; }

        public bool Statut { get; set; } = true;
    }
}
