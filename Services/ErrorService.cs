using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ProsocAPI.Models.Configuration;
using ProsocAPI.Models.DTOs.Core;

namespace ProsocAPI.Services
{
    /// <summary>
    /// Service pour la gestion structurée des erreurs
    /// </summary>
    public class ErrorService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ErrorService> _logger;
        private readonly ErrorHandlingOptions _errorHandlingOptions;
        private readonly IHostEnvironment _hostEnvironment;

        public ErrorService(
            IHttpContextAccessor httpContextAccessor,
            ILogger<ErrorService> logger,
            IOptions<ErrorHandlingOptions> errorHandlingOptions,
            IHostEnvironment hostEnvironment)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _errorHandlingOptions = errorHandlingOptions.Value;
            _hostEnvironment = hostEnvironment;
        }

        /// <summary>
        /// Crée une réponse d'erreur structurée pour les validations
        /// </summary>
        public ErrorResponse CreateValidationError(string code, string message, List<ValidationError>? validationErrors = null)
        {
            var errorResponse = CreateBaseErrorResponse(code, message, ErrorType.Validation, ErrorSeverity.Medium);
            
            if (validationErrors != null)
            {
                errorResponse.Error.ValidationErrors = validationErrors;
            }

            AddSuggestions(errorResponse, code);
            
            _logger.LogWarning("Validation error: {Code} - {Message}", code, message);
            
            return errorResponse;
        }

        /// <summary>
        /// Crée une réponse d'erreur structurée pour les erreurs métier
        /// </summary>
        public ErrorResponse CreateBusinessError(string code, string message, List<ErrorDetail>? details = null)
        {
            var errorResponse = CreateBaseErrorResponse(code, message, ErrorType.Business, ErrorSeverity.High);
            
            if (details != null)
            {
                errorResponse.Error.Details = details;
            }

            AddSuggestions(errorResponse, code);
            
            _logger.LogError("Business error: {Code} - {Message}", code, message);
            
            return errorResponse;
        }

        /// <summary>
        /// Crée une réponse d'erreur structurée pour les conflits
        /// </summary>
        public ErrorResponse CreateConflictError(string code, string message, object? conflictData = null)
        {
            var errorResponse = CreateBaseErrorResponse(code, message, ErrorType.Conflict, ErrorSeverity.Medium);
            
            if (conflictData != null)
            {
                errorResponse.Error.Details.Add(new ErrorDetail
                {
                    Field = "ConflictData",
                    Value = conflictData,
                    Issue = "Resource already exists"
                });
            }

            AddSuggestions(errorResponse, code);
            
            _logger.LogWarning("Conflict error: {Code} - {Message}", code, message);
            
            return errorResponse;
        }

        /// <summary>
        /// Crée une réponse d'erreur structurée pour les ressources non trouvées
        /// </summary>
        public ErrorResponse CreateNotFoundError(string code, string message, string? resourceType = null, string? resourceId = null)
        {
            var errorResponse = CreateBaseErrorResponse(code, message, ErrorType.NotFound, ErrorSeverity.Medium);
            
            if (!string.IsNullOrEmpty(resourceType) && !string.IsNullOrEmpty(resourceId))
            {
                errorResponse.Error.Details.Add(new ErrorDetail
                {
                    Field = "Resource",
                    Value = $"{resourceType}:{resourceId}",
                    Issue = "Resource not found"
                });
            }

            AddSuggestions(errorResponse, code);
            
            _logger.LogWarning("Not found error: {Code} - {Message}", code, message);
            
            return errorResponse;
        }

        /// <summary>
        /// Crée une réponse d'erreur structurée pour les erreurs techniques
        /// </summary>
        public ErrorResponse CreateTechnicalError(string code, string message, Exception? exception = null)
        {
            var errorResponse = CreateBaseErrorResponse(code, message, ErrorType.Technical, ErrorSeverity.High);

            if (exception != null && ShouldExposeExceptionDetails())
            {
                errorResponse.Error.Details.Add(new ErrorDetail
                {
                    Field = "Exception",
                    Value = exception.GetType().Name,
                    Issue = exception.Message
                });
            }

            AddSuggestions(errorResponse, code);

            _logger.LogError(exception, "Technical error: {Code} - {Message} | CorrelationId={CorrelationId}",
                code, message, errorResponse.CorrelationId);

            return errorResponse;
        }

        /// <summary>
        /// Indique si exception.Message peut être renvoyé au client (jamais la stack trace).
        /// </summary>
        public bool ShouldExposeExceptionDetails()
        {
            if (_errorHandlingOptions.ExposeExceptionDetails)
                return true;

            return _hostEnvironment.IsDevelopment()
                || string.Equals(_hostEnvironment.EnvironmentName, "Staging", StringComparison.OrdinalIgnoreCase)
                || string.Equals(_hostEnvironment.EnvironmentName, "IntegrationTests", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Crée la base de la réponse d'erreur
        /// </summary>
        private ErrorResponse CreateBaseErrorResponse(string code, string message, ErrorType type, ErrorSeverity severity)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            
            return new ErrorResponse
            {
                Error = new ErrorInfo
                {
                    Code = code,
                    Message = message,
                    Type = type,
                    Severity = severity
                },
                CorrelationId = GenerateCorrelationId(),
                Timestamp = DateTime.UtcNow,
                Path = httpContext?.Request.Path ?? string.Empty,
                RequestId = GetRequestId(httpContext)
            };
        }

        /// <summary>
        /// Ajoute des suggestions basées sur le code d'erreur
        /// </summary>
        private void AddSuggestions(ErrorResponse errorResponse, string code)
        {
            errorResponse.Error.Suggestions = code switch
            {
                ErrorCodes.VALIDATION_AGE_MINIMUM => new List<string>
                {
                    "Vérifiez que la date de naissance est correcte",
                    "L'âge minimum requis est de 18 ans",
                    "Assurez-vous que le format de date est YYYY-MM-DD"
                },
                
                ErrorCodes.VALIDATION_COLLECTE_MONTANT => new List<string>
                {
                    "Le montant doit être positif",
                    "Vérifiez que le montant n'est pas nul",
                    "Assurez-vous que le format du montant est correct"
                },
                
                ErrorCodes.VALIDATION_COLLECTE_MODE_PAIEMENT => new List<string>
                {
                    "Modes valides: ESPECE, MOBILE_MONEY, CARTE_BANCAIRE, VIREMENT_BANCAIRE, CHEQUE, VIRTUAL_ACCOUNT",
                    "Vérifiez l'orthographe du mode de paiement",
                    "Contactez le support pour ajouter un nouveau mode de paiement"
                },
                
                ErrorCodes.VALIDATION_CROISEE_TYPE_REFERENCE => new List<string>
                {
                    "Collecte de type SOUSCRIPTION : souscription.prestationId requis",
                    "Collecte de type FRAIS doit avoir un FraisId",
                    "Une collecte ne peut pas avoir les deux types de référence"
                },
                
                ErrorCodes.BUSINESS_PRESTATION_INEXISTANTE => new List<string>
                {
                    "Vérifiez que la prestation existe dans le système",
                    "Consultez la liste des prestations disponibles",
                    "Contactez l'administrateur pour ajouter une nouvelle prestation"
                },
                
                ErrorCodes.BUSINESS_ADHESION_EXISTANTE => new List<string>
                {
                    "Un affilié avec les mêmes informations existe déjà",
                    "Vérifiez si l'affilié n'est pas déjà enregistré",
                    "Contactez le support pour vérifier le statut de l'affilié"
                },
                
                _ => new List<string>
                {
                    "Vérifiez les données envoyées",
                    "Consultez la documentation de l'API",
                    "Contactez le support technique"
                }
            };
        }

        /// <summary>
        /// Génère un ID de corrélation unique
        /// </summary>
        private string GenerateCorrelationId()
        {
            return Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        }

        /// <summary>
        /// Récupère l'ID de la requête
        /// </summary>
        private string? GetRequestId(HttpContext? httpContext)
        {
            return httpContext?.TraceIdentifier;
        }
    }
}
