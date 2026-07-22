using System.ComponentModel.DataAnnotations;

namespace ProsocAPI.Models.Core
{
    /// <summary>
    /// Représente une commission calculée pour un agent
    /// </summary>
    public class Commission
    {
        /// <summary>
        /// ID unique de la commission
        /// </summary>
        public int IdCommission { get; set; }

        /// <summary>
        /// ID de la collecte qui a généré la commission
        /// </summary>
        public int CollecteId { get; set; }

        /// <summary>
        /// ID de l'agent qui reçoit la commission
        /// </summary>
        public int AgentId { get; set; }

        /// <summary>
        /// Montant de la commission
        /// </summary>
        [Required]
        public decimal Montant { get; set; }

        /// <summary>
        /// Taux de commission appliqué (ex: 0.25 pour 25%)
        /// </summary>
        [Required]
        public decimal Taux { get; set; }

        /// <summary>
        /// Date de création de la commission
        /// </summary>
        public DateTime DateCreation { get; set; }

        /// <summary>
        /// Statut de la commission
        /// </summary>
        public bool Statut { get; set; }

        /// <summary>
        /// Description ou commentaire sur la commission
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Source de la commission (ex: "COMMISSION_COLLECTE")
        /// </summary>
        [StringLength(100)]
        public string Source { get; set; } = string.Empty;
    }
}
