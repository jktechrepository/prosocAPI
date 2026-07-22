using ProsocAPI.Models.Core;

namespace ProsocAPI.Helpers
{
    public static class MethodePaiementHelper
    {
        public const string MobileMoney = "MOBILE_MONEY";
        public const string CarteBancaire = "CARTE_BANCAIRE";
        public const string Espece = "ESPECE";
        public const string VirtualAccount = "VIRTUAL_ACCOUNT";

        private static readonly HashSet<string> FlexPayCodes = new(StringComparer.OrdinalIgnoreCase)
        {
            MobileMoney,
            CarteBancaire
        };

        private static readonly HashSet<string> GuichetSyncCodes = new(StringComparer.OrdinalIgnoreCase)
        {
            Espece,
            "CHEQUE",
            "VIREMENT_BANCAIRE",
            VirtualAccount,
            "COMPTE VIRTUEL",
            "COMPTE_VIRTUEL"
        };

        public static bool IsFlexPay(string? mode) =>
            ToCanonicalCode(mode) is { } code && FlexPayCodes.Contains(code);

        public static bool IsVirtualAccount(string? mode) =>
            string.Equals(ToCanonicalCode(mode), VirtualAccount, StringComparison.OrdinalIgnoreCase);

        public static bool IsGuichetSync(string? mode) =>
            ToCanonicalCode(mode) is { } code && GuichetSyncCodes.Contains(code);

        /// <summary>Modes éligibles à une entrée caisse guichet : espèces ou paiement électronique FlexPay.</summary>
        public static bool IsEntreeCaisseEligible(string? mode)
        {
            var code = ToCanonicalCode(mode);
            if (code == null)
                return false;

            if (string.Equals(code, Espece, StringComparison.OrdinalIgnoreCase))
                return true;

            return FlexPayCodes.Contains(code);
        }

        public static string ResolveMouvementCaisseSource(string? mode) =>
            IsFlexPay(mode)
                ? MouvementCaisseSources.CollecteElectronique
                : MouvementCaisseSources.CollecteEspece;

        public static string NormalizeForStorage(string? mode)
        {
            var canonical = ToCanonicalCode(mode);
            if (canonical != null)
                return canonical;
            return mode?.Trim().ToUpperInvariant().Replace(" ", "_") ?? string.Empty;
        }

        public static void EnsureFlexPayOnly(string? mode)
        {
            if (!IsFlexPay(mode))
            {
                throw new InvalidOperationException(
                    "Ce flux accepte uniquement MOBILE_MONEY ou CARTE_BANCAIRE via FlexPay. " +
                    "Utilisez ESPECE, CHEQUE, VIREMENT_BANCAIRE ou VIRTUAL_ACCOUNT pour le paiement synchrone.");
            }
        }

        public static void EnsureGuichetSyncOnly(string? mode)
        {
            if (IsFlexPay(mode))
            {
                throw new InvalidOperationException(
                    "MOBILE_MONEY et CARTE_BANCAIRE doivent passer par FlexPay (flux asynchrone).");
            }
        }

        public static string? ToCanonicalCode(string? mode)
        {
            if (string.IsNullOrWhiteSpace(mode))
                return null;

            var upper = mode.Trim().ToUpperInvariant().Replace(" ", "_");
            return upper switch
            {
                "MOBILE_MONEY" or "MOBILEMONEY" or "ORANGE_MONEY" or "AIRTEL_MONEY" => MobileMoney,
                "CARTE_BANCAIRE" or "CARTE" or "CARD" => CarteBancaire,
                "ESPECE" or "ESPECES" => Espece,
                "VIRTUAL_ACCOUNT" or "COMPTE_VIRTUEL" or "COMPTE VIRTUEL" => VirtualAccount,
                "CHEQUE" => "CHEQUE",
                "VIREMENT_BANCAIRE" => "VIREMENT_BANCAIRE",
                _ => FlexPayCodes.Contains(upper) ? upper : null
            };
        }
    }
}
