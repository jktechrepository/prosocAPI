using System.Text.Json.Serialization;

namespace ProsocAPI.Models.DTOs.Core
{
    /// <summary>
    /// DTO pour les réponses d'erreur structurées
    /// </summary>
    public class ErrorResponse
    {
        [JsonPropertyName("error")]
        public ErrorInfo Error { get; set; } = new();

        [JsonPropertyName("correlationId")]
        public string CorrelationId { get; set; } = string.Empty;

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("path")]
        public string Path { get; set; } = string.Empty;

        [JsonPropertyName("requestId")]
        public string? RequestId { get; set; }
    }

    /// <summary>
    /// Informations détaillées sur l'erreur
    /// </summary>
    public class ErrorInfo
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public ErrorType Type { get; set; }

        [JsonPropertyName("severity")]
        public ErrorSeverity Severity { get; set; }

        [JsonPropertyName("details")]
        public List<ErrorDetail> Details { get; set; } = new();

        [JsonPropertyName("suggestions")]
        public List<string> Suggestions { get; set; } = new();

        [JsonPropertyName("validationErrors")]
        public List<ValidationError> ValidationErrors { get; set; } = new();
    }

    /// <summary>
    /// Type d'erreur
    /// </summary>
    public enum ErrorType
    {
        Validation,
        Business,
        Technical,
        NotFound,
        Conflict,
        Unauthorized,
        Forbidden
    }

    /// <summary>
    /// Sévérité de l'erreur
    /// </summary>
    public enum ErrorSeverity
    {
        Low,
        Medium,
        High,
        Critical
    }

    /// <summary>
    /// Détail spécifique de l'erreur
    /// </summary>
    public class ErrorDetail
    {
        [JsonPropertyName("field")]
        public string Field { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public object? Value { get; set; }

        [JsonPropertyName("issue")]
        public string Issue { get; set; } = string.Empty;

        [JsonPropertyName("expected")]
        public string? Expected { get; set; }
    }

    /// <summary>
    /// Erreur de validation spécifique
    /// </summary>
    public class ValidationError
    {
        [JsonPropertyName("field")]
        public string Field { get; set; } = string.Empty;

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("attemptedValue")]
        public object? AttemptedValue { get; set; }

        [JsonPropertyName("errorCode")]
        public string ErrorCode { get; set; } = string.Empty;
    }

    /// <summary>
    /// Codes d'erreur standardisés
    /// </summary>
    public static class ErrorCodes
    {
        // Validation
        public const string VALIDATION_AGE_MINIMUM = "VALIDATION_AGE_MINIMUM";
        public const string VALIDATION_AGE_MAXIMUM_ADHERENT = "VALIDATION_AGE_MAXIMUM_ADHERENT";
        public const string VALIDATION_PERSONNE_EN_CHARGE = "VALIDATION_PERSONNE_EN_CHARGE";
        public const string VALIDATION_NIVEAU1_AT = "VALIDATION_NIVEAU1_AT";
        public const string VALIDATION_NIVEAU2_ENCODEUR = "VALIDATION_NIVEAU2_ENCODEUR";
        public const string VALIDATION_COLLECTE_MONTANT = "VALIDATION_COLLECTE_MONTANT";
        public const string VALIDATION_COLLECTE_MODE_PAIEMENT = "VALIDATION_COLLECTE_MODE_PAIEMENT";
        public const string VALIDATION_COLLECTE_DATE = "VALIDATION_COLLECTE_DATE";
        public const string VALIDATION_COLLECTE_REFERENCE = "VALIDATION_COLLECTE_REFERENCE";
        public const string VALIDATION_DEPENDANT_NOM = "VALIDATION_DEPENDANT_NOM";
        public const string VALIDATION_DEPENDANT_LIEN = "VALIDATION_DEPENDANT_LIEN";
        public const string VALIDATION_CROISEE_TYPE_REFERENCE = "VALIDATION_CROISEE_TYPE_REFERENCE";

        // Business
        public const string BUSINESS_PRESTATION_INEXISTANTE = "BUSINESS_PRESTATION_INEXISTANTE";
        public const string BUSINESS_FRAIS_INEXISTANT = "BUSINESS_FRAIS_INEXISTANT";
        public const string BUSINESS_COTISATION_AFFILIE_INEXISTANTE = "BUSINESS_COTISATION_AFFILIE_INEXISTANTE";
        public const string BUSINESS_ADHESION_EXISTANTE = "BUSINESS_ADHESION_EXISTANTE";
        public const string BUSINESS_WALLET_VIRTUEL_INEXISTANT = "BUSINESS_WALLET_VIRTUEL_INEXISTANT";
        public const string BUSINESS_SOLDE_INSUFFISANT = "BUSINESS_SOLDE_INSUFFISANT";
        public const string BUSINESS_SUPERVISEUR_SANS_COMMUNE_TITULAIRE = "BUSINESS_SUPERVISEUR_SANS_COMMUNE_TITULAIRE";

        // Technical
        public const string TECHNICAL_INTERNAL_ERROR = "TECHNICAL_INTERNAL_ERROR";
        public const string TECHNICAL_DATABASE_ERROR = "TECHNICAL_DATABASE_ERROR";
        public const string TECHNICAL_NOTIFICATION_ERROR = "TECHNICAL_NOTIFICATION_ERROR";
    }
}
