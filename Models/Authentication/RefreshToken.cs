using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ProsocAPI.Models.Authentication
{
    public class RefreshToken
    {
        [Key]
        public int IdRefreshToken { get; set; }

        [Required]
        [ForeignKey(nameof(Utilisateur))]
        public int UtilisateurId { get; set; }

        [Required]
        [MaxLength(500)]
        public string TokenHash { get; set; } = string.Empty;

        [Required]
        public DateTime DateCreation { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime DateExpiration { get; set; }

        public DateTime? DateRevocation { get; set; }

        [MaxLength(200)]
        public string? DeviceInfo { get; set; }

        [MaxLength(50)]
        public string? IpAddress { get; set; }

        // Propriétés calculées
        public bool EstRevoke => DateRevocation.HasValue;
        public bool EstExpire => DateTime.UtcNow > DateExpiration;
        public bool EstActif => !EstRevoke && !EstExpire;

        // Relations
        [JsonIgnore]
        public virtual Utilisateur Utilisateur { get; set; } = null!;
    }

    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;

        public string? DeviceInfo { get; set; }
    }
}
