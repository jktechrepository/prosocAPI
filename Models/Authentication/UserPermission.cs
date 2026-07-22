using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ProsocAPI.Models.Authentication
{
    public class UserPermission
    {
        [Key]
        public int IdUserPermission { get; set; }

        [Required]
        public int UtilisateurId { get; set; }

        [Required]
        public int PermissionId { get; set; }

        [Required]
        public bool IsGranted { get; set; }

        public DateTime DateAttribution { get; set; } = DateTime.UtcNow;

        public DateTime? DateExpiration { get; set; }

        [MaxLength(500)]
        public string? Commentaire { get; set; }

        public int? AttribueParIdUtilisateur { get; set; }

        // Navigation properties
        [JsonIgnore]
        [ForeignKey(nameof(UtilisateurId))]
        public virtual Utilisateur? Utilisateur { get; set; }

        [JsonIgnore]
        [ForeignKey(nameof(PermissionId))]
        public virtual Permission? Permission { get; set; }

        [JsonIgnore]
        [ForeignKey(nameof(AttribueParIdUtilisateur))]
        [NotMapped]
        public virtual Utilisateur? AttribuePar { get; set; }

        public bool IsValid()
        {
            if (DateExpiration == null)
                return true;

            return DateTime.UtcNow <= DateExpiration.Value;
        }
    }
}
