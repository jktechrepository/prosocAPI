using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProsocAPI.Models.DTOs.FlexPay
{
    public class FlexPayCallbackDto
    {
        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("reference")]
        public string? Reference { get; set; }

        [JsonPropertyName("providerReference")]
        public string? ProviderReference { get; set; }

        [JsonPropertyName("orderNumber")]
        public string? OrderNumber { get; set; }

        [JsonPropertyName("amount")]
        public string? Amount { get; set; }

        [JsonPropertyName("amountCustomer")]
        public string? AmountCustomer { get; set; }

        [JsonPropertyName("phone")]
        public string? Phone { get; set; }

        [JsonPropertyName("currency")]
        public string? Currency { get; set; }

        [JsonPropertyName("createdAt")]
        public string? CreatedAt { get; set; }

        [JsonPropertyName("channel")]
        public string? Channel { get; set; }
    }

    public class FlexPayPaymentResponseDto
    {
        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("orderNumber")]
        public string? OrderNumber { get; set; }

        [JsonPropertyName("paymentUrl")]
        public string? PaymentUrl { get; set; }

        [JsonPropertyName("redirectUrl")]
        public string? RedirectUrl { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }

        public string? ResolvePaymentUrl() =>
            !string.IsNullOrWhiteSpace(PaymentUrl) ? PaymentUrl
            : !string.IsNullOrWhiteSpace(RedirectUrl) ? RedirectUrl
            : Url;

        public bool IsSuccess => Code == "0";
    }

    public class FlexPayCheckResponseDto
    {
        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("transaction")]
        public FlexPayTransactionDto? Transaction { get; set; }
    }

    public class FlexPayTransactionDto
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("orderNumber")]
        public string? OrderNumber { get; set; }
    }

    public class FlexPayCallbackProcessResultDto
    {
        public bool Success { get; set; }
        public bool AlreadyProcessed { get; set; }
        /// <summary>
        /// True si le paiement n'est pas encore final chez FlexPay (poll verifier) —
        /// le client doit continuer à interroger, sans traiter comme un refus.
        /// </summary>
        public bool Pending { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? IdCollecte { get; set; }
        public int? IdAdhesion { get; set; }
        public Guid? IdCollecteEnAttente { get; set; }
    }

    public class InitiateFlexPayResponseDto
    {
        public Guid IdCollecteEnAttente { get; set; }
        public string? OrderNumberFlexPay { get; set; }
        public string ReferenceFlexPay { get; set; } = string.Empty;
        public decimal MontantTarif { get; set; }
        public string CodeDeviseTarif { get; set; } = string.Empty;
        public decimal MontantFlexPay { get; set; }
        public string CodeDevisePaiement { get; set; } = string.Empty;
        public decimal? TauxApplique { get; set; }
        public DateTime HoldExpireAt { get; set; }
        public string? PaymentUrl { get; set; }
        public bool FlexPayAccepted { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class InfoPaiementMarchandCreateDto
    {
        [Required, MaxLength(100)]
        public string CodeMarchand { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string ApiToken { get; set; } = string.Empty;

        public bool ActifMobileMoney { get; set; } = true;
        public bool ActifCarteBancaire { get; set; } = true;
        public bool Statut { get; set; } = true;
    }

    public class InfoPaiementMarchandUpdateDto
    {
        [MaxLength(100)]
        public string? CodeMarchand { get; set; }

        [MaxLength(500)]
        public string? ApiToken { get; set; }

        public bool? ActifMobileMoney { get; set; }
        public bool? ActifCarteBancaire { get; set; }
        public bool? Statut { get; set; }
    }

    public class InfoPaiementMarchandReadDto
    {
        public int IdInfoPaiementMarchand { get; set; }
        public string CodeMarchand { get; set; } = string.Empty;
        public string ApiTokenMasked { get; set; } = string.Empty;
        public bool ActifMobileMoney { get; set; }
        public bool ActifCarteBancaire { get; set; }
        public bool Statut { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime? DateModification { get; set; }
    }
}
