using System.ComponentModel.DataAnnotations;
using ProsocAPI.Models.Core;

namespace ProsocAPI.Models.DTOs.DashboardAdmin
{
    /// <summary>
    /// DTO pour les collectes en attente de validation
    /// </summary>
    public class CollecteEnAttenteDto
    {
        /// <summary>
        /// ID de la collecte
        /// </summary>
        public int IdCollecte { get; set; }

        /// <summary>
        /// Nom de l'affilié
        /// </summary>
        public string NomAffilie { get; set; } = string.Empty;

        /// <summary>
        /// Prénom de l'affilié
        /// </summary>
        public string PrenomAffilie { get; set; } = string.Empty;

        /// <summary>
        /// Nom de l'agent
        /// </summary>
        public string NomAgent { get; set; } = string.Empty;

        /// <summary>
        /// Montant de la collecte
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal Montant { get; set; }

        /// <summary>
        /// Référence de paiement
        /// </summary>
        public string ReferencePaiement { get; set; } = string.Empty;

        /// <summary>
        /// Mode de paiement
        /// </summary>
        public string ModePaiement { get; set; } = string.Empty;

        /// <summary>
        /// Date de la collecte
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime DateCollecte { get; set; }

        /// <summary>
        /// Statut du paiement
        /// </summary>
        public string StatutPaiement { get; set; } = string.Empty;

        /// <summary>
        /// Temps d'attente en heures
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N1}")]
        public decimal HeuresAttente { get; set; }

        /// <summary>
        /// Priorité de validation (1=Urgent, 2=Normal, 3=Faible)
        /// </summary>
        public int Priorite { get; set; }
    }
}
