using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProsocAPI.Models.Pagination
{
    /// <summary>
    /// Modèle de requête pour la pagination universelle
    /// </summary>
    public class PaginationRequest
    {
        /// <summary>
        /// Numéro de la page (commence à 1)
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Le numéro de page doit être supérieur à 0")]
        [JsonPropertyName("pageNumber")]
        public int Page { get; set; } = 1;

        /// <summary>
        /// Nombre d'éléments par page
        /// </summary>
        [Range(1, 100, ErrorMessage = "La taille de la page doit être entre 1 et 100")]
        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// Champ de tri
        /// </summary>
        public string? SortBy { get; set; }

        /// <summary>
        /// Direction du tri (asc/desc)
        /// </summary>
        [RegularExpression("^(asc|desc)$", ErrorMessage = "Le sens de tri doit être 'asc' ou 'desc'")]
        public string SortDirection { get; set; } = "asc";

        /// <summary>
        /// Terme de recherche
        /// </summary>
        [StringLength(100, ErrorMessage = "Le terme de recherche ne peut pas dépasser 100 caractères")]
        public string? Search { get; set; }

        /// <summary>
        /// Filtres additionnels (format JSON)
        /// </summary>
        public string? Filters { get; set; }
    }

    /// <summary>
    /// Filtre générique pour la pagination
    /// </summary>
    public class FilterRequest
    {
        public string Field { get; set; } = string.Empty;
        public string Operator { get; set; } = string.Empty; // eq, ne, gt, gte, lt, lte, contains, startswith, endswith
        public string Value { get; set; } = string.Empty;
    }

    /// <summary>
    /// Requête de pagination étendue avec filtres typés
    /// </summary>
    public class AdvancedPaginationRequest : PaginationRequest
    {
        /// <summary>
        /// Liste des filtres à appliquer
        /// </summary>
        public List<FilterRequest> FilterList { get; set; } = new();

        /// <summary>
        /// Champs à inclure dans la réponse
        /// </summary>
        public List<string> IncludeFields { get; set; } = new();

        /// <summary>
        /// Champs à exclure de la réponse
        /// </summary>
        public List<string> ExcludeFields { get; set; } = new();
    }
}
