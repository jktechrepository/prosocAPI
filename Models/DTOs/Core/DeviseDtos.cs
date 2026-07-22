using System.ComponentModel.DataAnnotations;

namespace ProsocAPI.Models.DTOs.Core
{
    public class DeviseReadDto
    {
        public int IdDevise { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Nom { get; set; } = string.Empty;
        public string? Symbole { get; set; }
        public bool EstDevisePrincipale { get; set; }
        public bool Statut { get; set; }
    }

    public class DeviseCreateDto
    {
        [Required]
        [StringLength(10)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Nom { get; set; } = string.Empty;

        [StringLength(10)]
        public string? Symbole { get; set; }

        public bool EstDevisePrincipale { get; set; }

        public bool Statut { get; set; } = true;
    }

    public class DeviseUpdateDto
    {
        [Required]
        [StringLength(10)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Nom { get; set; } = string.Empty;

        [StringLength(10)]
        public string? Symbole { get; set; }

        public bool EstDevisePrincipale { get; set; }

        public bool Statut { get; set; } = true;
    }
}
