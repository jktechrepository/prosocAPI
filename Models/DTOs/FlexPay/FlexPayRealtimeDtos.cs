namespace ProsocAPI.Models.DTOs.FlexPay
{
    /// <summary>
    /// Payload SignalR émis sur l'événement FlexPayPaymentUpdated après traitement du callback.
    /// </summary>
    public class FlexPayPaymentUpdatedDto
    {
        public Guid IdCollecteEnAttente { get; set; }
        public string? OrderNumberFlexPay { get; set; }
        public string? ReferenceFlexPay { get; set; }
        public bool Success { get; set; }
        public bool AlreadyProcessed { get; set; }
        public bool Failed { get; set; }
        public string? CodeFlexPay { get; set; }
        public string Message { get; set; } = string.Empty;
        public string SourceFlux { get; set; } = string.Empty;
        public int? IdAdhesion { get; set; }
        public int? IdCollecte { get; set; }
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    }
}
