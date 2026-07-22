using System.ComponentModel.DataAnnotations;

namespace ProsocAPI.Models.DTOs.Authentication
{
    public class LoginRequestDto
    {
        [Required]
        [MaxLength(100)]
        public string NomUtilisateur { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string MotDePasse { get; set; } = string.Empty;

        // Device information for FCM notifications
        [MaxLength(500)]
        public string? FcmToken { get; set; }

        [MaxLength(100)]
        public string? DeviceType { get; set; } // "Android", "iOS", "Web"

        [MaxLength(100)]
        public string? DeviceModel { get; set; }

        [MaxLength(50)]
        public string? OsVersion { get; set; }
    }

    public class LoginResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }
        public int UtilisateurId { get; set; }
        public string NomUtilisateur { get; set; } = string.Empty;
        public string? Role { get; set; }
    }
}
