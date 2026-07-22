using System.ComponentModel.DataAnnotations;

namespace ProsocAPI.Models.DTOs.DashboardAdmin
{
    /// <summary>
    /// Données pour les graphiques du dashboard administrateur
    /// </summary>
    public class DashboardAdminGraphsDto
    {
        /// <summary>
        /// Évolution des collectes mensuelles
        /// </summary>
        public List<CollecteMensuelleDto>? CollectesMensuelles { get; set; }

        /// <summary>
        /// Évolution des adhésions mensuelles
        /// </summary>
        public List<AdhesionMensuelleDto>? AdhesionsMensuelles { get; set; }

        /// <summary>
        /// Performance des meilleurs agents
        /// </summary>
        public List<PerformanceAgentsDto>? TopAgents { get; set; }

        /// <summary>
        /// Répartition des adhésions par type
        /// </summary>
        public List<RepartitionAdhesionsDto>? RepartitionAdhesions { get; set; }
    }

    /// <summary>
    /// Données de collecte mensuelle pour graphique
    /// </summary>
    public class CollecteMensuelleDto
    {
        /// <summary>
        /// Mois formaté (ex: "Jan 2026")
        /// </summary>
        public string Mois { get; set; } = string.Empty;

        /// <summary>
        /// Montant total des collectes du mois
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal Montant { get; set; }

        /// <summary>
        /// Nombre de collectes du mois
        /// </summary>
        public int Nombre { get; set; }

        /// <summary>
        /// Progression par rapport au mois précédent (%)
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N1}%")]
        public decimal Progression { get; set; }
    }

    /// <summary>
    /// Données d'adhésion mensuelle pour graphique
    /// </summary>
    public class AdhesionMensuelleDto
    {
        /// <summary>
        /// Mois formaté (ex: "Jan 2026")
        /// </summary>
        public string Mois { get; set; } = string.Empty;

        /// <summary>
        /// Nombre d'adhésions du mois
        /// </summary>
        public int Nombre { get; set; }

        /// <summary>
        /// Progression par rapport au mois précédent (%)
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N1}%")]
        public decimal Progression { get; set; }
    }

    /// <summary>
    /// Performance d'un agent pour classement
    /// </summary>
    public class PerformanceAgentsDto
    {
        /// <summary>
        /// ID de l'agent
        /// </summary>
        public int AgentId { get; set; }

        /// <summary>
        /// Nom complet de l'agent
        /// </summary>
        public string NomAgent { get; set; } = string.Empty;

        /// <summary>
        /// Nombre total d'affiliés recrutés
        /// </summary>
        public int TotalAffilies { get; set; }

        /// <summary>
        /// Montant total des collectes
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal TotalCollectes { get; set; }

        /// <summary>
        /// Nombre total de collectes
        /// </summary>
        public int NombreCollectes { get; set; }

        /// <summary>
        /// Montant moyen par collecte
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal MontantMoyenCollecte { get; set; }

        /// <summary>
        /// Score de performance (0-100)
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal ScorePerformance { get; set; }
    }

    /// <summary>
    /// Répartition des adhésions par type
    /// </summary>
    public class RepartitionAdhesionsDto
    {
        /// <summary>
        /// Type d'adhésion
        /// </summary>
        public string TypeAdhesion { get; set; } = string.Empty;

        /// <summary>
        /// Nombre d'adhésions de ce type
        /// </summary>
        public int Nombre { get; set; }

        /// <summary>
        /// Pourcentage par rapport au total
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N1}%")]
        public decimal Pourcentage { get; set; }
    }
}
