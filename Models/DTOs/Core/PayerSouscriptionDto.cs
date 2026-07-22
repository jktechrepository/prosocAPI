using System.ComponentModel.DataAnnotations;
using ProsocAPI.Helpers;

namespace ProsocAPI.Models.DTOs.Core
{
    /// <summary>
    /// DTO pour le paiement d'une souscription par un affilié
    /// </summary>
    public class PayerSouscriptionDto
    {
        /// <summary>
        /// ID de la souscription à payer
        /// </summary>
        [Required(ErrorMessage = "L'ID de la souscription est requis")]
        public int SouscriptionPrestationId { get; set; }

        /// <summary>
        /// Montant du paiement (doit correspondre exactement au montant de la souscription)
        /// </summary>
        [Required(ErrorMessage = "Le montant est requis")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Le montant doit être supérieur à 0")]
        public decimal Montant { get; set; }

        /// <summary>
        /// Mode de paiement : MOBILE_MONEY, CARTE_BANCAIRE (FlexPay) ou VIRTUAL_ACCOUNT (synchrone).
        /// </summary>
        [Required(ErrorMessage = "Le mode de paiement est requis")]
        public string ModePaiement { get; set; } = string.Empty;

        /// <summary>
        /// Téléphone Mobile Money (requis pour MOBILE_MONEY via FlexPay).
        /// </summary>
        [StringLength(20)]
        public string? Phone { get; set; }

        /// <summary>
        /// ID de la devise utilisée
        /// </summary>
        [Required(ErrorMessage = "La devise est requise")]
        public int DeviseId { get; set; }

        /// <summary>
        /// Référence du paiement (optionnel)
        /// </summary>
        [StringLength(100, ErrorMessage = "La référence ne peut pas dépasser 100 caractères")]
        public string ReferencePaiement { get; set; } = string.Empty;

        /// <summary>
        /// Mois de la période payée (1-12). Par défaut : mois courant.
        /// </summary>
        [Range(0, 12)]
        public int Mois { get; set; }

        /// <summary>
        /// Année de la période payée. Par défaut : année courante.
        /// </summary>
        [Range(0, 2100)]
        public int Annee { get; set; }

        /// <summary>
        /// Observation ou commentaire sur le paiement
        /// </summary>
        [StringLength(500, ErrorMessage = "L'observation ne peut pas dépasser 500 caractères")]
        public string Observation { get; set; } = string.Empty;

        /// <summary>
        /// Valider que le DTO est cohérent
        /// </summary>
        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(ModePaiement) || Montant <= 0 || SouscriptionPrestationId <= 0 || DeviseId <= 0)
                return false;

            return MethodePaiementHelper.IsFlexPay(ModePaiement)
                   || MethodePaiementHelper.IsGuichetSync(ModePaiement);
        }
    }
}
