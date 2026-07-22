using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ProsocAPI.Models.Authentication
{
    public class Utilisateur
    {
        [Key]
        public int IdUtilisateur { get; set; }
        
        public Guid? ReferenceUtilisateur { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string NomUtilisateur { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? EmailUtilisateur { get; set; }

        [MaxLength(30)]
        public string? PhoneUtilisateur { get; set; }

        [Required]
        public string MotDePasseHash { get; set; } = string.Empty;
        
        public string? DefaultUsername { get; set; }
        
        public bool DoitChangerMotDePasse { get; set; } = false;
        
        public bool Statut { get; set; } = true;
        
        public int? RoleId { get; set; }

        // Relations avec Agent, Affilie et hôpital partenaire
        public int? AgentId { get; set; }
        public int? AffilieId { get; set; }
        public int? HopitalPartenaireId { get; set; }

        public int? AssureurId { get; set; }

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey("AgentId")]
        public Core.Agent? Agent { get; set; }

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey("AffilieId")]
        public Core.Affilie? Affilie { get; set; }

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey("HopitalPartenaireId")]
        public Core.HopitalPartenaire? HopitalPartenaire { get; set; }

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey("AssureurId")]
        public Core.Assureur? Assureur { get; set; }

        // Attributs Techniques
        [JsonIgnore]
        public DateTime DateCreation { get; set; } = DateTime.Now;
        
        [JsonIgnore]
        public bool IsConnecte { get; set; } = false;

        [JsonIgnore]
        [ValidateNever]
        public Role? Role { get; set; }

        [JsonIgnore]
        [ValidateNever]
        [NotMapped]
        public ICollection<Core.Notification>? NotificationsEnvoyees { get; set; }

        [JsonIgnore]
        [ValidateNever]
        [NotMapped]
        public ICollection<Core.Notification>? NotificationsRecues { get; set; }

        [ValidateNever]
        public ICollection<PasswordResetToken>? PasswordResetTokens { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public virtual ICollection<UserPermission>? UserPermissions { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public virtual ICollection<UserDevice>? UserDevices { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public ICollection<RefreshToken>? RefreshTokens { get; set; }

        // ═══════════════════════════════════════════════════════════════════
        // ✅ MULTI-RÔLES : Relation N-N avec Role via UserRole
        // ═══════════════════════════════════════════════════════════════════

        [JsonIgnore]
        [ValidateNever]
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        [NotMapped]
        public IEnumerable<Role> Roles => UserRoles
            .Where(ur => ur.Statut == true)
            .Select(ur => ur.Role);

        [NotMapped]
        public Role? PrimaryRole => UserRoles
            .Where(ur => ur.Statut == true && ur.IsPrimary)
            .Select(ur => ur.Role)
            .FirstOrDefault() 
            ?? UserRoles
                .Where(ur => ur.Statut == true)
                .OrderBy(ur => ur.Role.Niveau ?? 999)
                .Select(ur => ur.Role)
                .FirstOrDefault();
    }
}
