using System.ComponentModel.DataAnnotations;
using ProsocAPI.Models.Core;

namespace ProsocAPI.Models.DTOs.Core
{
    public class TargetAgentReadDto
    {
        public int IdTargetAgent { get; set; }
        public int RoleId { get; set; }
        public string? RoleNom { get; set; }
        public string LibelleTarget { get; set; } = string.Empty;
        public PeriodiciteTarget Periodicite { get; set; }
        public int Nombre { get; set; }
        public bool Statut { get; set; }
    }

    public class TargetAgentCreateDto
    {
        [Required]
        [StringLength(50)]
        public string RoleNom { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string LibelleTarget { get; set; } = string.Empty;

        [Required]
        public PeriodiciteTarget Periodicite { get; set; }

        [Range(1, int.MaxValue)]
        public int Nombre { get; set; }

        public bool Statut { get; set; } = true;
    }

    public class TargetAgentUpdateDto
    {
        [Required]
        [StringLength(50)]
        public string RoleNom { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string LibelleTarget { get; set; } = string.Empty;

        [Required]
        public PeriodiciteTarget Periodicite { get; set; }

        [Range(1, int.MaxValue)]
        public int Nombre { get; set; }

        public bool Statut { get; set; } = true;
    }
}
