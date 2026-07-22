using System.ComponentModel.DataAnnotations;

namespace ProsocAPI.Models.DTOs.DashboardAdmin
{
    /// <summary>
    /// KPIs principaux pour le dashboard administrateur
    /// </summary>
    public class DashboardAdminKpisDto
    {
        /// <summary>
        /// Nombre total d'affiliés actifs
        /// </summary>
        public int TotalAffilies { get; set; }

        /// <summary>
        /// Nombre total d'agents actifs
        /// </summary>
        public int TotalAgents { get; set; }

        /// <summary>
        /// Montant total des collectes du mois en cours (tous agents), consolidé en devise principale.
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal TotalCollectesMois { get; set; }

        /// <summary>Code ISO de la devise principale (ex. USD) utilisée pour TotalCollectesMois et TotalCommissionsMois.</summary>
        public string? DevisePrincipaleCode { get; set; }

        /// <summary>
        /// Montant total des commissions du mois en cours (mouvements wallet COMM_COLLECTE), consolidé en devise principale.
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal TotalCommissionsMois { get; set; }

        /// <summary>
        /// Nombre de nouvelles adhésions aujourd'hui
        /// </summary>
        public int NouvellesAdhesionsAujourdhui { get; set; }

        /// <summary>
        /// Nombre de collectes en attente de validation admin (paiement confirmé, statut ≠ Validé admin).
        /// </summary>
        public int CollectesEnAttente { get; set; }

        /// <summary>
        /// Nombre total de collectes ce mois
        /// </summary>
        public int NombreCollectesMois { get; set; }

        /// <summary>
        /// Progression des collectes vs mois précédent (%). 100 % si démarrage sans historique (mois précédent = 0).
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N1}%")]
        public decimal ProgressionCollectesMois { get; set; }

        /// <summary>
        /// Nombre d'agents inactifs
        /// </summary>
        public int AgentsInactifs { get; set; }

        /// <summary>
        /// Date de la dernière collecte enregistrée
        /// </summary>
        public DateTime? DerniereCollecte { get; set; }

        /// <summary>
        /// Nombre d'affiliés inactifs
        /// </summary>
        public int AffiliesInactifs { get; set; }
    }
}
