using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ProsocAPI.Models.Authentication
{
    public class RolePermission
    {
        [Key]
        public int IdRolePermission { get; set; }

        [Required]
        public int RoleId { get; set; }

        [Required]
        public int PermissionId { get; set; }

        public DateTime DateAttribution { get; set; } = DateTime.Now;

        public int? IdUtilisateurAttribution { get; set; }

        // Relations
        [ForeignKey(nameof(RoleId))]
        [JsonIgnore]
        public virtual Role Role { get; set; } = null!;

        [ForeignKey(nameof(PermissionId))]
        [JsonIgnore]
        public virtual Permission Permission { get; set; } = null!;
    }
}
