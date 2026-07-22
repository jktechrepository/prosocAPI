using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ProsocAPI.Models.Authentication
{
    public class UserRole
    {
        [Key]
        public int IdUserRole { get; set; }

        [Required]
        public int UtilisateurId { get; set; }

        [Required]
        public int RoleId { get; set; }

        public bool IsPrimary { get; set; } = false;

        public DateTime DateAttribution { get; set; } = DateTime.Now;

        public int? IdUtilisateurAttribution { get; set; }

        public bool Statut { get; set; } = true;

        // Relations
        [ForeignKey(nameof(UtilisateurId))]
        [JsonIgnore]
        public virtual Utilisateur Utilisateur { get; set; } = null!;

        [ForeignKey(nameof(RoleId))]
        [JsonIgnore]
        public virtual Role Role { get; set; } = null!;
    }
}
