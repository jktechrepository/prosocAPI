using System.ComponentModel.DataAnnotations;

namespace ProsocAPI.Models.DTOs.Authentication
{
    public class PermissionReadDto
    {
        public int IdPermission { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Categorie { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public bool Statut { get; set; }
        public DateTime DateCreation { get; set; }
    }

    public class PermissionCreateDto
    {
        [Required]
        [MaxLength(100)]
        public string Nom { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(50)]
        public string Categorie { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Action { get; set; } = string.Empty;

        public bool Statut { get; set; } = true;
    }

    public class PermissionUpdateDto
    {
        [Required]
        [MaxLength(100)]
        public string Nom { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(50)]
        public string Categorie { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Action { get; set; } = string.Empty;

        public bool Statut { get; set; } = true;
    }
}
