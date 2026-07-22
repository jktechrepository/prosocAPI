using System.ComponentModel.DataAnnotations;

namespace Prosoc.Models.DTOs.CategorieAgent
{
    public class CategorieAgentDto
    {
        public int IdCategorieAgent { get; set; }
        public string Code { get; set; } = string.Empty;
        public string LibelleCategorie { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool Statut { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime? DateModification { get; set; }
        public int NombreAgents { get; set; }
    }

    public class CreateCategorieAgentDto
    {
        [Required(ErrorMessage = "Le code de la catégorie est requis")]
        [StringLength(10, MinimumLength = 2, ErrorMessage = "Le code doit contenir entre 2 et 10 caractères")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "La description est requise")]
        [StringLength(500, ErrorMessage = "La description ne peut pas dépasser 500 caractères")]
        public string Description { get; set; } = string.Empty;

        /// <summary>Optionnel — sinon généré : « {Description} ({Code}) ».</summary>
        [StringLength(200, ErrorMessage = "Le libellé ne peut pas dépasser 200 caractères")]
        public string? LibelleCategorie { get; set; }

        public bool Statut { get; set; } = true;
    }

    public class UpdateCategorieAgentDto
    {
        public int IdCategorieAgent { get; set; }

        [Required(ErrorMessage = "Le code de la catégorie est requis")]
        [StringLength(10, MinimumLength = 2, ErrorMessage = "Le code doit contenir entre 2 et 10 caractères")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "La description est requise")]
        [StringLength(500, ErrorMessage = "La description ne peut pas dépasser 500 caractères")]
        public string Description { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Le libellé ne peut pas dépasser 200 caractères")]
        public string? LibelleCategorie { get; set; }

        public bool Statut { get; set; }
    }

    public class CategorieAgentSummaryDto
    {
        public int IdCategorieAgent { get; set; }
        public string Code { get; set; } = string.Empty;
        public string LibelleCategorie { get; set; } = string.Empty;
        public bool Statut { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime? DateModification { get; set; }
    }
}
