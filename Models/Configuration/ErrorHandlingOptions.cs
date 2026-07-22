namespace ProsocAPI.Models.Configuration
{
    /// <summary>
    /// Contrôle l'exposition des détails d'exception dans les réponses HTTP (sécurité prod).
    /// </summary>
    public class ErrorHandlingOptions
    {
        public const string SectionName = "ErrorHandling";

        /// <summary>
        /// Si true, exception.Message est inclus dans Error.Details (dev/staging).
        /// En production, laisser false : seul correlationId + message métier sont renvoyés.
        /// </summary>
        public bool ExposeExceptionDetails { get; set; }
    }
}
