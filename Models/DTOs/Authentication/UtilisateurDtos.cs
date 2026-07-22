using System.ComponentModel.DataAnnotations;

namespace ProsocAPI.Models.DTOs.Authentication
{
    public class UtilisateurReadDto
    {
        public int IdUtilisateur { get; set; }
        public Guid? ReferenceUtilisateur { get; set; }
        public string NomUtilisateur { get; set; } = string.Empty;
        public string? EmailUtilisateur { get; set; }
        public string? PhoneUtilisateur { get; set; }
        public bool Statut { get; set; }
        public int? RoleId { get; set; }
        public int? AgentId { get; set; }
        public int? AffilieId { get; set; }
        public int? HopitalPartenaireId { get; set; }
        public int? AssureurId { get; set; }
        public DateTime DateCreation { get; set; }
    }

    public class UtilisateurCreateDto
    {
        [Required]
        [MaxLength(100)]
        public string NomUtilisateur { get; set; } = string.Empty;

        [MaxLength(200)]
        [EmailAddress]
        public string? EmailUtilisateur { get; set; }

        [MaxLength(30)]
        public string? PhoneUtilisateur { get; set; }

        [Required]
        [MinLength(6)]
        [MaxLength(200)]
        public string MotDePasse { get; set; } = string.Empty;

        public bool Statut { get; set; } = true;

        public int? RoleId { get; set; }
        public int? AgentId { get; set; }
        public int? AffilieId { get; set; }
        public int? HopitalPartenaireId { get; set; }
        public int? AssureurId { get; set; }
    }

    /// <summary>Création d'un compte personnel d'accueil hôpital partenaire.</summary>
    public class AgentHopitalUtilisateurCreateDto
    {
        [Required]
        [MaxLength(100)]
        public string NomUtilisateur { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        [EmailAddress]
        public string EmailUtilisateur { get; set; } = string.Empty;

        [MaxLength(30)]
        public string? PhoneUtilisateur { get; set; }

        [Required]
        [MinLength(6)]
        [MaxLength(200)]
        public string MotDePasse { get; set; } = string.Empty;

        [Required]
        public int HopitalPartenaireId { get; set; }

        public bool Statut { get; set; } = true;
    }

    /// <summary>Création d'un compte portail partenaire assureur.</summary>
    public class AssureurUtilisateurCreateDto
    {
        [Required]
        [MaxLength(100)]
        public string NomUtilisateur { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        [EmailAddress]
        public string EmailUtilisateur { get; set; } = string.Empty;

        [MaxLength(30)]
        public string? PhoneUtilisateur { get; set; }

        [Required]
        [MinLength(6)]
        [MaxLength(200)]
        public string MotDePasse { get; set; } = string.Empty;

        [Required]
        public int AssureurId { get; set; }

        public bool Statut { get; set; } = true;
    }

    public class UtilisateurUpdateDto
    {
        [Required]
        [MaxLength(100)]
        public string NomUtilisateur { get; set; } = string.Empty;

        [MaxLength(200)]
        [EmailAddress]
        public string? EmailUtilisateur { get; set; }

        [MaxLength(30)]
        public string? PhoneUtilisateur { get; set; }

        public bool Statut { get; set; } = true;

        public int? RoleId { get; set; }
        public int? AgentId { get; set; }
        public int? AffilieId { get; set; }
        public int? HopitalPartenaireId { get; set; }
        public int? AssureurId { get; set; }
    }
}
