using ProsocAPI.Models.DTOs.FlexPay;
using ProsocAPI.Services;

namespace Prosoc.Tests.Integration.FlexPay;

/// <summary>Stub FlexPay — aucun appel HTTP sortant.</summary>
public sealed class FlexPayStubService : IFlexPayService
{
    public static string LastOrderNumber { get; private set; } = "ORD-STUB-0001";

    /// <summary>Valeur renvoyée dans <c>transaction.status</c> (et utilisée comme repli si null).</summary>
    public static string CheckTransactionStatus { get; set; } = "0";

    /// <summary>Code racine de la réponse check (souvent "0" même en pending selon FlexPay).</summary>
    public static string CheckCode { get; set; } = "0";

    public static void ResetCheckStatus()
    {
        CheckTransactionStatus = "0";
        CheckCode = "0";
    }

    public Task<FlexPayPaymentResponseDto> InitierPaiementMobileMoneyAsync(
        string merchant,
        string apiToken,
        string reference,
        string phone,
        decimal amount,
        string currency,
        string callbackUrl,
        CancellationToken ct = default)
    {
        LastOrderNumber = $"ORD-MM-{reference}";
        return Task.FromResult(SuccessResponse());
    }

    public Task<FlexPayPaymentResponseDto> InitierPaiementCarteV1Async(
        string merchant,
        string apiToken,
        string reference,
        decimal amount,
        string currency,
        string description,
        string callbackUrl,
        string approveUrl,
        string cancelUrl,
        string declineUrl,
        CancellationToken ct = default)
    {
        LastOrderNumber = $"ORD-CB-{reference}";
        return Task.FromResult(SuccessResponse(paymentUrl: "https://stub.flexpay.test/pay"));
    }

    public Task<FlexPayCheckResponseDto> VerifierStatutTransactionAsync(
        string apiToken,
        string orderNumber,
        CancellationToken ct = default) =>
        Task.FromResult(new FlexPayCheckResponseDto
        {
            Code = CheckCode,
            Message = CheckTransactionStatus == "0" ? "Stub OK" : "Stub pending",
            Transaction = new FlexPayTransactionDto
            {
                Status = CheckTransactionStatus,
                OrderNumber = orderNumber
            }
        });

    private static FlexPayPaymentResponseDto SuccessResponse(string? paymentUrl = null) =>
        new()
        {
            Code = "0",
            Message = "Stub OK",
            OrderNumber = LastOrderNumber,
            PaymentUrl = paymentUrl
        };
}
