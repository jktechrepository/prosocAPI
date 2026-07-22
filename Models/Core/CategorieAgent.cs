using System.ComponentModel.DataAnnotations;

namespace ProsocAPI.Models.Core
{
    public class CategorieAgent
    {
        [Key]
        public int IdCategorieAgent { get; set; }

        /// <summary>Code court technique (ex. AT, FI) — matricule, MAASH, filtres.</summary>
        [Required]
        [MaxLength(10)]
        public string Code { get; set; } = string.Empty;

        /// <summary>Libellé affiché (ex. Agent de Terrain (AT)).</summary>
        [Required]
        [MaxLength(200)]
        public string LibelleCategorie { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool Statut { get; set; } = true;

        public DateTime DateCreation { get; set; } = DateTime.Now;

        public DateTime? DateModification { get; set; }

        public virtual ICollection<Agent> Agents { get; set; } = new List<Agent>();
    }
}
