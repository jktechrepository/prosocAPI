using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProsocAPI.Models.Pagination;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;

namespace ProsocAPI.Services
{
    /// <summary>
    /// Service de pagination universelle
    /// </summary>
    public interface IPaginationService
    {
        Task<PaginatedResponse<T>> CreatePaginatedResponseAsync<T>(
            IQueryable<T> query,
            PaginationRequest request,
            CancellationToken cancellationToken = default);

        Task<ExtendedPaginatedResponse<T>> CreateExtendedPaginatedResponseAsync<T>(
            IQueryable<T> query,
            AdvancedPaginationRequest request,
            CancellationToken cancellationToken = default);

        IQueryable<T> ApplyFilters<T>(IQueryable<T> query, List<FilterRequest> filters);
        IQueryable<T> ApplySorting<T>(IQueryable<T> query, string sortBy, string sortDirection = "asc");
        IQueryable<T> ApplySearch<T>(IQueryable<T> query, string searchTerm, List<string>? searchFields = null);
    }

    public class PaginationService : IPaginationService
    {
        private readonly ILogger<PaginationService> _logger;
        private readonly PaginationOptions _options;

        public PaginationService(ILogger<PaginationService> logger, IOptions<PaginationOptions> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        public async Task<PaginatedResponse<T>> CreatePaginatedResponseAsync<T>(
            IQueryable<T> query,
            PaginationRequest request,
            CancellationToken cancellationToken = default)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                // Validation et normalisation des paramètres
                request.Page = Math.Max(1, request.Page);
                request.PageSize = Math.Min(_options.MaxPageSize, Math.Max(1, request.PageSize));

                // Appliquer les filtres si présents
                if (!string.IsNullOrEmpty(request.Filters))
                {
                    try
                    {
                        var filters = JsonSerializer.Deserialize<List<FilterRequest>>(request.Filters);
                        if (filters != null)
                        {
                            query = ApplyFilters(query, filters);
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Erreur lors de la désérialisation des filtres: {Filters}", request.Filters);
                    }
                }

                // Appliquer la recherche
                if (!string.IsNullOrEmpty(request.Search))
                {
                    query = ApplySearch(query, request.Search);
                }

                // Compter le total avant pagination
                var totalItems = await query.CountAsync(cancellationToken);

                // Appliquer le tri (EF Core exige un OrderBy avant Skip/Take)
                if (!string.IsNullOrEmpty(request.SortBy))
                {
                    query = ApplySorting(query, request.SortBy, request.SortDirection);
                }
                else
                {
                    query = ApplyDefaultSorting(query);
                }

                // Appliquer la pagination
                var data = await query
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync(cancellationToken);

                stopwatch.Stop();

                _logger.LogDebug("Pagination exécutée en {ElapsedMs}ms - Page: {Page}, PageSize: {PageSize}, Total: {Total}",
                    stopwatch.ElapsedMilliseconds, request.Page, request.PageSize, totalItems);

                return new PaginatedResponse<T>(data, request.Page, request.PageSize, totalItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de la réponse paginée");
                throw;
            }
        }

        public async Task<ExtendedPaginatedResponse<T>> CreateExtendedPaginatedResponseAsync<T>(
            IQueryable<T> query,
            AdvancedPaginationRequest request,
            CancellationToken cancellationToken = default)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                // Créer la réponse de base
                var baseResponse = await CreatePaginatedResponseAsync<T>(query, request, cancellationToken);

                // Créer la réponse étendue
                var extendedResponse = new ExtendedPaginatedResponse<T>
                {
                    Data = baseResponse.Data,
                    CurrentPage = baseResponse.CurrentPage,
                    PageSize = baseResponse.PageSize,
                    TotalItems = baseResponse.TotalItems,
                    TotalPages = baseResponse.TotalPages,
                    HasNextPage = baseResponse.HasNextPage,
                    HasPreviousPage = baseResponse.HasPreviousPage,
                    StartItem = baseResponse.StartItem,
                    EndItem = baseResponse.EndItem,
                    ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                    AppliedFilters = request.FilterList.Select(f => $"{f.Field} {f.Operator} {f.Value}").ToList(),
                    AppliedSorting = string.IsNullOrEmpty(request.SortBy) ? "Default" : $"{request.SortBy} {request.SortDirection}",
                    FromCache = false, // TODO: Implémenter la logique de cache
                    ApiVersion = "v1",
                    Timestamp = DateTime.UtcNow
                };

                // Appliquer les filtres de champs si spécifiés
                if (request.IncludeFields.Any() || request.ExcludeFields.Any())
                {
                    extendedResponse.Data = ApplyFieldFiltering(extendedResponse.Data, request.IncludeFields, request.ExcludeFields);
                }

                return extendedResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de la réponse paginée étendue");
                throw;
            }
        }

        public IQueryable<T> ApplyFilters<T>(IQueryable<T> query, List<FilterRequest> filters)
        {
            if (filters == null || !filters.Any())
                return query;

            foreach (var filter in filters)
            {
                try
                {
                    query = ApplyFilter(query, filter);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Erreur lors de l'application du filtre: {Field} {Operator} {Value}",
                        filter.Field, filter.Operator, filter.Value);
                }
            }

            return query;
        }

        private IQueryable<T> ApplyFilter<T>(IQueryable<T> query, FilterRequest filter)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, filter.Field);
            var constant = Expression.Constant(ConvertValue(filter.Value, property.Type));

            Expression comparison = filter.Operator.ToLower() switch
            {
                "eq" => Expression.Equal(property, constant),
                "ne" => Expression.NotEqual(property, constant),
                "gt" => Expression.GreaterThan(property, constant),
                "gte" => Expression.GreaterThanOrEqual(property, constant),
                "lt" => Expression.LessThan(property, constant),
                "lte" => Expression.LessThanOrEqual(property, constant),
                "contains" => Expression.Call(property, "Contains", null, constant),
                "startswith" => Expression.Call(property, "StartsWith", null, constant),
                "endswith" => Expression.Call(property, "EndsWith", null, constant),
                _ => throw new ArgumentException($"Opérateur non supporté: {filter.Operator}")
            };

            var lambda = Expression.Lambda<Func<T, bool>>(comparison, parameter);
            return query.Where(lambda);
        }

        /// <summary>
        /// Tri stable par défaut lorsque le client n'envoie pas de sortBy.
        /// Évite l'exception EF « Skip/Take without OrderBy ».
        /// </summary>
        private IQueryable<T> ApplyDefaultSorting<T>(IQueryable<T> query)
        {
            var entityType = typeof(T);
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;

            var dateCreation = entityType.GetProperty("DateCreation", flags);
            if (dateCreation != null && IsSortableScalarType(dateCreation.PropertyType))
                return ApplySorting(query, dateCreation.Name, "desc");

            var idProperty = entityType.GetProperty("Id", flags)
                ?? entityType.GetProperties(flags)
                    .FirstOrDefault(p => p.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase)
                                         && IsSortableScalarType(p.PropertyType));

            if (idProperty != null)
                return ApplySorting(query, idProperty.Name, "desc");

            _logger.LogWarning(
                "Pagination sans sortBy : aucun champ DateCreation/Id trouvé pour {EntityType}",
                entityType.Name);

            return query;
        }

        private static bool IsSortableScalarType(Type type)
        {
            var underlying = Nullable.GetUnderlyingType(type) ?? type;
            return underlying == typeof(int)
                || underlying == typeof(long)
                || underlying == typeof(short)
                || underlying == typeof(Guid)
                || underlying == typeof(DateTime)
                || underlying == typeof(decimal);
        }

        public IQueryable<T> ApplySorting<T>(IQueryable<T> query, string sortBy, string sortDirection = "asc")
        {
            if (string.IsNullOrEmpty(sortBy))
                return query;

            try
            {
                var propertyInfo = ResolveSortProperty(typeof(T), sortBy);
                if (propertyInfo == null || !IsSortableScalarType(propertyInfo.PropertyType))
                {
                    _logger.LogWarning(
                        "Tri ignoré : propriété introuvable ou non triable {SortBy} sur {EntityType}",
                        sortBy,
                        typeof(T).Name);
                    return query;
                }

                var parameter = Expression.Parameter(typeof(T), "x");
                var property = Expression.Property(parameter, propertyInfo);
                var lambda = Expression.Lambda(property, parameter);

                var methodName = sortDirection.ToLower() == "desc" ? "OrderByDescending" : "OrderBy";
                var method = typeof(Queryable).GetMethods()
                    .Where(m => m.Name == methodName && m.GetParameters().Length == 2)
                    .Single()
                    .MakeGenericMethod(typeof(T), property.Type);

                return (IQueryable<T>)method.Invoke(null, new object[] { query, lambda })!;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erreur lors de l'application du tri: {SortBy} {SortDirection}", sortBy, sortDirection);
                return query;
            }
        }

        /// <summary>
        /// Résout une propriété de tri (IgnoreCase). Alias <c>id</c> → <c>Id</c>, clé <c>[Key]</c>, ou <c>Id{Entity}</c> (ex. IdAgent).
        /// </summary>
        private static PropertyInfo? ResolveSortProperty(Type entityType, string sortBy)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;

            var direct = entityType.GetProperty(sortBy, flags);
            if (direct != null)
                return direct;

            if (!string.Equals(sortBy, "id", StringComparison.OrdinalIgnoreCase))
                return null;

            var byNameId = entityType.GetProperty("Id", flags);
            if (byNameId != null && IsSortableScalarType(byNameId.PropertyType))
                return byNameId;

            var byKeyAttr = entityType.GetProperties(flags)
                .FirstOrDefault(p => p.GetCustomAttribute<KeyAttribute>() != null
                                     && IsSortableScalarType(p.PropertyType));
            if (byKeyAttr != null)
                return byKeyAttr;

            var byEntityPrefix = entityType.GetProperty("Id" + entityType.Name, flags);
            if (byEntityPrefix != null && IsSortableScalarType(byEntityPrefix.PropertyType))
                return byEntityPrefix;

            return entityType.GetProperties(flags)
                .FirstOrDefault(p => p.Name.StartsWith("Id", StringComparison.OrdinalIgnoreCase)
                                     && IsSortableScalarType(p.PropertyType));
        }

        public IQueryable<T> ApplySearch<T>(IQueryable<T> query, string searchTerm, List<string>? searchFields = null)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return query;

            var parameter = Expression.Parameter(typeof(T), "x");
            Expression? searchExpression = null;

            var fieldsToSearch = searchFields ?? _options.DefaultSearchFields;
            
            if (!fieldsToSearch.Any())
            {
                // Si aucun champ n'est spécifié, essayer de chercher dans toutes les propriétés string
                var stringProperties = typeof(T).GetProperties()
                    .Where(p => p.PropertyType == typeof(string))
                    .Select(p => p.Name);

                fieldsToSearch = stringProperties.ToList();
            }

            foreach (var field in fieldsToSearch)
            {
                try
                {
                    var property = Expression.Property(parameter, field);
                    var searchTermConstant = Expression.Constant(searchTerm, typeof(string));
                    var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                    var containsExpression = Expression.Call(property, containsMethod!, searchTermConstant);

                    searchExpression = searchExpression == null 
                        ? containsExpression 
                        : Expression.OrElse(searchExpression, containsExpression);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Impossible d'ajouter le champ {Field} à la recherche", field);
                }
            }

            if (searchExpression != null)
            {
                var lambda = Expression.Lambda<Func<T, bool>>(searchExpression, parameter);
                query = query.Where(lambda);
            }

            return query;
        }

        private List<T> ApplyFieldFiltering<T>(List<T> data, List<string> includeFields, List<string> excludeFields)
        {
            if (!includeFields.Any() && !excludeFields.Any())
                return data;

            var result = new List<T>();

            foreach (var item in data)
            {
                var expando = new ExpandoObject() as IDictionary<string, object>;
                var itemType = item.GetType();

                foreach (var property in itemType.GetProperties())
                {
                    var shouldInclude = !includeFields.Any() || includeFields.Contains(property.Name);
                    var shouldExclude = excludeFields.Contains(property.Name);

                    if (shouldInclude && !shouldExclude)
                    {
                        var value = property.GetValue(item);
                        if (value != null)
                        {
                            expando[property.Name] = value;
                        }
                    }
                }

                // Convertir l'ExpandoObject en T (simplifié - dans la pratique, utiliser un mapper)
                var json = JsonSerializer.Serialize(expando);
                var filteredItem = JsonSerializer.Deserialize<T>(json);
                if (filteredItem != null)
                {
                    result.Add(filteredItem);
                }
            }

            return result;
        }

        private object ConvertValue(string value, Type targetType)
        {
            if (targetType == typeof(string))
                return value;

            if (targetType == typeof(int) || targetType == typeof(int?))
                return int.Parse(value);

            if (targetType == typeof(long) || targetType == typeof(long?))
                return long.Parse(value);

            if (targetType == typeof(decimal) || targetType == typeof(decimal?))
                return decimal.Parse(value);

            if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
                return DateTime.Parse(value);

            if (targetType == typeof(bool) || targetType == typeof(bool?))
                return bool.Parse(value);

            if (targetType.IsEnum)
                return Enum.Parse(targetType, value);

            return Convert.ChangeType(value, targetType);
        }
    }
}
