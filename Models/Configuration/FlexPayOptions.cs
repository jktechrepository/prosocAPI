namespace ProsocAPI.Models.Configuration
{
    public class FlexPayOptions
    {
        public const string SectionName = "FlexPay";

        public bool Enabled { get; set; }

        public int HoldMinutes { get; set; } = 15;

        public string? CallbackBaseUrl { get; set; }

        public string MobileMoneyUrl { get; set; } =
            "https://backend.flexpay.cd/api/rest/v1/paymentService";

        public string CardPaymentUrl { get; set; } =
            "https://cardpayment.flexpay.cd/v1.1/pay";

        public string CheckTransactionUrl { get; set; } =
            "https://apicheck.flexpaie.com/api/rest/v1/check";

        public bool ForceProductionCallbackInDev { get; set; }

        public decimal MontantTolerance { get; set; } = 0.05m;
    }
}
