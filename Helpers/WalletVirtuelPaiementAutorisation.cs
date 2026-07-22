using System.Security.Claims;

namespace ProsocAPI.Helpers
{
    /// <summary>
    /// Autorise le mode <c>VIRTUAL_ACCOUNT</c> (débit WalletVirtuel) aux rôles terrain uniquement.
    /// </summary>
    public static class WalletVirtuelPaiementAutorisation
    {
        public const string MessageNonAutorise =
            "Ce compte ne peut pas percevoir un paiement depuis le mode de paiement WalletVirtuel. Veuillez contacter le support.";

        /// <summary>Noms de rôles JWT (<see cref="Models.Authentication.Role.Nom"/>) autorisés.</summary>
        public static readonly HashSet<string> RolesAutorises = new(StringComparer.OrdinalIgnoreCase)
        {
            "Agent (AT)",
            "Chef d'équipe",
            "Superviseur",
            "Percepteur"
        };

        public static bool CallerPeutPayerEnWalletVirtuel(ClaimsPrincipal? user)
        {
            if (user?.Identity?.IsAuthenticated != true)
                return false;

            return user.FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .Any(r => !string.IsNullOrWhiteSpace(r) && RolesAutorises.Contains(r));
        }

        /// <summary>
        /// Lève <see cref="ArgumentException"/> (mappé en 400) si le caller n'a aucun rôle whitelist.
        /// Multi-rôles : autorisé dès qu'au moins un rôle JWT est dans la whitelist.
        /// </summary>
        public static void EnsureCallerPeutPayerEnWalletVirtuel(ClaimsPrincipal? user)
        {
            if (!CallerPeutPayerEnWalletVirtuel(user))
                throw new ArgumentException(MessageNonAutorise);
        }

        public static void EnsureSiVirtualAccount(string? modePaiement, ClaimsPrincipal? user)
        {
            if (MethodePaiementHelper.IsVirtualAccount(modePaiement))
                EnsureCallerPeutPayerEnWalletVirtuel(user);
        }
    }
}
