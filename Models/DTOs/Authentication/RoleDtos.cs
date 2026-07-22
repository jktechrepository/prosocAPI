using System.ComponentModel.DataAnnotations;

namespace ProsocAPI.Models.DTOs.Authentication
{
    public class RoleReadDto
    {
        public int IdRole { get; set; }
        public string Nom { get; set; } = string.Empty;
        
        public string? Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? Niveau { get; set; }
        public bool Statut { get; set; }
        public DateTime DateCreation { get; set; }
    }

    public class RoleCreateDto
    {
        [Required]
        [MaxLength(50)]
        public string Nom { get; set; } = string.Empty;
        
        public string? Code { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Description { get; set; }

        public int? Niveau { get; set; }

        public bool Statut { get; set; } = true;
    }

    public class RoleUpdateDto
    {
        [Required]
        [MaxLength(50)]
        public string Nom { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string? Code { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Description { get; set; }

        public int? Niveau { get; set; }

        public bool Statut { get; set; } = true;
    }
}
