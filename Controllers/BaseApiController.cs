using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.Pagination;
using ProsocAPI.Services;
using Microsoft.Extensions.Logging;

namespace ProsocAPI.Controllers
{
    /// <summary>
    /// Contrôleur de base avec fonctionnalités de pagination
    /// </summary>
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    public abstract class BaseApiController : ControllerBase
    {
        protected readonly IPaginationService _paginationService;
        protected readonly PaginationOptions _paginationOptions;
        protected readonly ILogger<BaseApiController> _logger;

        protected BaseApiController(
            IPaginationService paginationService,
            IOptions<PaginationOptions> paginationOptions,
            ILogger<BaseApiController> logger)
        {
            _paginationService = paginationService;
            _paginationOptions = paginationOptions.Value;
            _logger = logger;
        }

        /// <summary>
        /// Valide et normalise les paramètres de pagination
        /// </summary>
        [ApiExplorerSettings(IgnoreApi = true)]
        public PaginationRequest ValidatePaginationRequest(PaginationRequest request)
        {
            if (request == null)
                request = new PaginationRequest();

            // Normaliser les valeurs
            request.Page = Math.Max(1, request.Page);
            request.PageSize = Math.Min(_paginationOptions.MaxPageSize, Math.Max(1, request.PageSize));

            // Valider la direction de tri
            if (!string.IsNullOrEmpty(request.SortDirection) && 
                !request.SortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase) &&
                !request.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase))
            {
                request.SortDirection = "asc";
            }

            // Limiter la longueur du terme de recherche
            if (request.Search?.Length > 100)
            {
                request.Search = request.Search.Substring(0, 100);
            }

            return request;
        }

        /// <summary>
        /// Crée une réponse paginée standard
        /// </summary>
        protected async Task<ActionResult<PaginatedResponse<T>>> CreatePaginatedResponseAsync<T>(
            IQueryable<T> query,
            PaginationRequest? request = null)
        {
            try
            {
                request = ValidatePaginationRequest(request ?? new PaginationRequest());
                
                var response = await _paginationService.CreatePaginatedResponseAsync(query, request);
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    ErrorCodes.TECHNICAL_INTERNAL_ERROR,
                    "Une erreur technique est survenue lors de la création de la réponse paginée",
                    ex);
            }
        }

        /// <summary>
        /// Crée une réponse paginée étendue
        /// </summary>
        protected async Task<ActionResult<ExtendedPaginatedResponse<T>>> CreateExtendedPaginatedResponseAsync<T>(
            IQueryable<T> query,
            AdvancedPaginationRequest? request = null)
        {
            try
            {
                request = ValidatePaginationRequest(request ?? new AdvancedPaginationRequest()) as AdvancedPaginationRequest;
                
                var response = await _paginationService.CreateExtendedPaginatedResponseAsync(query, request!);
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    ErrorCodes.TECHNICAL_INTERNAL_ERROR,
                    "Une erreur technique est survenue lors de la création de la réponse paginée étendue",
                    ex);
            }
        }

        /// <summary>
        /// Crée une réponse de succès standardisée
        /// </summary>
        protected ActionResult<T> SuccessResponse<T>(T data, string message = "Opération réussie")
        {
            return Ok(new
            {
                success = true,
                message = message,
                data = data,
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Réponse 500 structurée via ErrorService (migration progressive des catch génériques).
        /// N'altère pas les endpoints tant qu'ils n'appellent pas explicitement cette méthode.
        /// </summary>
        protected ObjectResult TechnicalErrorResponse(string message, Exception ex)
        {
            return ((ControllerBase)this).TechnicalErrorResponse(message, ex);
        }

        protected ObjectResult TechnicalErrorResponse(string code, string message, Exception ex)
        {
            return ((ControllerBase)this).TechnicalErrorResponse(code, message, ex);
        }

        /// <summary>
        /// Crée une réponse d'erreur standardisée
        /// </summary>
        protected ActionResult ErrorResponse(string message, int statusCode = 400, object? details = null)
        {
            return StatusCode(statusCode, new
            {
                success = false,
                message = message,
                details = details,
                timestamp = DateTime.UtcNow,
                traceId = HttpContext.TraceIdentifier
            });
        }

        /// <summary>
        /// Valide que l'ID est valide
        /// </summary>
        protected ActionResult ValidateId(int id)
        {
            if (id <= 0)
            {
                return ErrorResponse("ID invalide", 400);
            }
            return Ok();
        }

        /// <summary>
        /// Journalise une action d'API
        /// </summary>
        protected void LogApiAction(string action, object? parameters = null)
        {
            _logger.LogInformation("API Action: {Action} | User: {User} | Parameters: {Parameters}",
                action,
                User?.Identity?.Name ?? "Anonymous",
                parameters);
        }

        /// <summary>
        /// Vérifie si l'utilisateur a la permission requise
        /// </summary>
        protected bool HasPermission(string permission)
        {
            if (User.IsInRole("Admin") || User.IsInRole("SuperAdmin"))
                return true;

            return User.HasClaim("permission", permission);
        }

        /// <summary>
        /// Vérifie le claim JWT sans bypass Admin/SuperAdmin (ex. UPDATE_COLLECTE révoquée pour tous).
        /// </summary>
        protected bool HasExplicitPermission(string permission) =>
            User.HasClaim("permission", permission);

        /// <summary>
        /// Réponse 403 lorsqu'une permission est manquante (ne pas utiliser Forbid(string) : le paramètre est un schéma d'auth).
        /// </summary>
        protected ActionResult ForbiddenPermission(string permission)
        {
            return ErrorResponse($"Permission requise : {permission}", 403);
        }

        /// <summary>
        /// Retourne l'ID de l'utilisateur actuel
        /// </summary>
        protected int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        /// <summary>
        /// Retourne le nom d'utilisateur actuel
        /// </summary>
        protected string GetCurrentUserName()
        {
            return User?.Identity?.Name ?? "Anonymous";
        }
    }

    /// <summary>
    /// Attribut pour valider les paramètres de pagination
    /// </summary>
    public class ValidatePaginationAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var paginationRequest = context.ActionArguments
                .Values
                .OfType<PaginationRequest>()
                .FirstOrDefault();

            if (paginationRequest != null)
            {
                var controller = context.Controller as BaseApiController;
                if (controller != null)
                {
                    var validatedRequest = controller.ValidatePaginationRequest(paginationRequest);
                    
                    // Remplacer l'argument original par la version validée
                    var paramName = context.ActionArguments
                        .FirstOrDefault(kvp => kvp.Value == paginationRequest).Key;
                    
                    if (!string.IsNullOrEmpty(paramName))
                    {
                        context.ActionArguments[paramName] = validatedRequest;
                    }
                }
            }

            base.OnActionExecuting(context);
        }
    }
}
