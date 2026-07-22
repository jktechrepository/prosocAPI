using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ProsocAPI.Models.Authentication
{
    public class Role
    {
        [Key]
        public int IdRole { get; set; }

        [Required]
        [MaxLength(50)]
        public string Nom { get; set; } = string.Empty;
        
       
        [MaxLength(50)]
        public string? Code { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Description { get; set; }

        public int? Niveau { get; set; } = 5;

        public bool Statut { get; set; } = true;

        [JsonIgnore]
        public DateTime DateCreation { get; set; } = DateTime.Now;

        // Relations
        [JsonIgnore]
        public virtual ICollection<Utilisateur> Utilisateurs { get; set; } = new List<Utilisateur>();

        [JsonIgnore]
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
