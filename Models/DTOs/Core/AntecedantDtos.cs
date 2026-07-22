using System.ComponentModel.DataAnnotations;

namespace ProsocAPI.Models.DTOs.Core
{
    public class AntecedantReadDto
    {
        public int IdAntecedant { get; set; }
        public string Description { get; set; } = string.Empty;
        public int AffilieId { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime? DateModification { get; set; }
        public bool Statut { get; set; }
    }

    public class AntecedantCreateDto
    {
        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        // 🆕 MODIFIÉ : AffilieId optionnel pour la création avec affilié
        public int? AffilieId { get; set; }

        public bool Statut { get; set; } = true;
    }

    public class AntecedantUpdateDto
    {
        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        // 🆕 MODIFIÉ : AffilieId optionnel pour la cohérence
        public int? AffilieId { get; set; }

        public bool Statut { get; set; }
    }
}
