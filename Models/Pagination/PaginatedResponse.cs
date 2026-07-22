using System.Text.Json.Serialization;

namespace ProsocAPI.Models.Pagination
{
    /// <summary>
    /// Réponse paginée universelle
    /// </summary>
    /// <typeparam name="T">Type des données</typeparam>
    public class PaginatedResponse<T>
    {
        /// <summary>
        /// Données de la page actuelle
        /// </summary>
        public List<T> Data { get; set; } = new();

        /// <summary>
        /// Numéro de la page actuelle
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// Nombre d'éléments par page
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Nombre total d'éléments
        /// </summary>
        public long TotalItems { get; set; }

        /// <summary>
        /// Nombre total de pages
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Indique s'il y a une page suivante
        /// </summary>
        public bool HasNextPage { get; set; }

        /// <summary>
        /// Indique s'il y a une page précédente
        /// </summary>
        public bool HasPreviousPage { get; set; }

        /// <summary>
        /// Premier élément de la page (index global)
        /// </summary>
        public long StartItem { get; set; }

        /// <summary>
        /// Dernier élément de la page (index global)
        /// </summary>
        public long EndItem { get; set; }

        /// <summary>
        /// Métadonnées supplémentaires
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, object> Metadata { get; set; } = new();

        /// <summary>
        /// Constructeur
        /// </summary>
        public PaginatedResponse()
        {
        }

        /// <summary>
        /// Constructeur avec paramètres
        /// </summary>
        public PaginatedResponse(List<T> data, int currentPage, int pageSize, long totalItems)
        {
            Data = data;
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalItems = totalItems;
            TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            HasNextPage = currentPage < TotalPages;
            HasPreviousPage = currentPage > 1;
            StartItem = (currentPage - 1) * pageSize + 1;
            EndItem = Math.Min(currentPage * pageSize, totalItems);
        }

        /// <summary>
        /// Crée une réponse vide
        /// </summary>
        public static PaginatedResponse<T> Empty(int currentPage = 1, int pageSize = 20)
        {
            return new PaginatedResponse<T>(new List<T>(), currentPage, pageSize, 0);
        }
    }

    /// <summary>
    /// Réponse paginée avec métadonnées étendues
    /// </summary>
    /// <typeparam name="T">Type des données</typeparam>
    public class ExtendedPaginatedResponse<T> : PaginatedResponse<T>
    {
        /// <summary>
        /// Temps d'exécution de la requête (en millisecondes)
        /// </summary>
        public long ExecutionTimeMs { get; set; }

        /// <summary>
        /// Filtres appliqués
        /// </summary>
        public List<string> AppliedFilters { get; set; } = new();

        /// <summary>
        /// Tri appliqué
        /// </summary>
        public string AppliedSorting { get; set; } = string.Empty;

        /// <summary>
        /// Cache hit/miss
        /// </summary>
        public bool FromCache { get; set; }

        /// <summary>
        /// Version de l'API
        /// </summary>
        public string ApiVersion { get; set; } = "v1";

        /// <summary>
        /// Timestamp de la réponse
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Options de pagination pour la configuration
    /// </summary>
    public class PaginationOptions
    {
        /// <summary>
        /// Taille de page par défaut
        /// </summary>
        public int DefaultPageSize { get; set; } = 20;

        /// <summary>
        /// Taille de page maximale
        /// </summary>
        public int MaxPageSize { get; set; } = 100;

        /// <summary>
        /// Nombre maximum de résultats de recherche
        /// </summary>
        public int MaxSearchResults { get; set; } = 1000;

        /// <summary>
        /// Activer le cache pour les résultats paginés
        /// </summary>
        public bool EnableCache { get; set; } = true;

        /// <summary>
        /// Durée du cache en secondes
        /// </summary>
        public int CacheDurationSeconds { get; set; } = 300;

        /// <summary>
        /// Champs de recherche par défaut
        /// </summary>
        public List<string> DefaultSearchFields { get; set; } = new();
    }
}
