using System.ComponentModel.DataAnnotations;

namespace ProsocAPI.Models.DTOs.Core
{
    /// <summary>
    /// DTO pour le résumé du dashboard des commissions
    /// </summary>
    public class CommissionDashboardDto
    {
        public decimal TotalCommissions { get; set; }
        public decimal SoldeActuel { get; set; }
        public int NombreCommissions { get; set; }
        public decimal CommissionMoyenne { get; set; }
        public decimal CommissionMax { get; set; }
        public decimal CommissionMin { get; set; }
        public decimal CeMois { get; set; }
        public decimal LaSemaine { get; set; }
        public decimal Aujourdhui { get; set; }
        public string Devise { get; set; } = "USD";
        public DateTime DerniereMiseAJour { get; set; }
    }

    /// <summary>
    /// DTO pour une commission individuelle dans le dashboard
    /// </summary>
    public class CommissionItemDto
    {
        public int Id { get; set; }
        public decimal Montant { get; set; }
        public string Devise { get; set; } = "USD";
        public DateTime DateOperation { get; set; }
        public string Source { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal SoldeApresOperation { get; set; }
        public string AffilieNom { get; set; } = string.Empty;
        public decimal CollecteMontant { get; set; }
        public int CollecteId { get; set; }
    }

    /// <summary>
    /// DTO pour les statistiques de commissions par période
    /// </summary>
    public class CommissionStatsDto
    {
        public string Periode { get; set; } = string.Empty; // "Aujourd'hui", "Cette semaine", "Ce mois", etc.
        public decimal Total { get; set; }
        public int Nombre { get; set; }
        public decimal Moyenne { get; set; }
        public List<DailyCommissionDto> DetailsQuotidiens { get; set; } = new();
    }

    /// <summary>
    /// DTO pour les commissions quotidiennes
    /// </summary>
    public class DailyCommissionDto
    {
        public DateTime Date { get; set; }
        public decimal Total { get; set; }
        public int Nombre { get; set; }
        public decimal Moyenne { get; set; }
    }

    /// <summary>
    /// DTO pour les paramètres de filtrage et pagination
    /// </summary>
    public class CommissionFilterDto
    {
        /// <summary>Identifiant de l'agent (paramètre explicite, non lu depuis le JWT).</summary>
        [Range(1, int.MaxValue)]
        public int IdAgent { get; set; }

        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
        public decimal? MontantMin { get; set; }
        public decimal? MontantMax { get; set; }
        public string? Source { get; set; } // COMMISSION_COLLECTE, etc.
        public string? Devise { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? TriPar { get; set; } = "DateOperation"; // DateOperation, Montant
        public string OrdreTri { get; set; } = "desc"; // asc, desc
    }

    /// <summary>
    /// DTO pour le résumé mensuel des commissions
    /// </summary>
    public class MonthlyCommissionSummaryDto
    {
        public int Annee { get; set; }
        public int Mois { get; set; }
        public string MoisNom { get; set; } = string.Empty;
        public decimal TotalCommissions { get; set; }
        public int NombreCommissions { get; set; }
        public decimal MoyenneJournaliere { get; set; }
        public decimal MeilleurJour { get; set; }
        public decimal PireJour { get; set; }
        public List<DailyCommissionDto> DetailsQuotidiens { get; set; } = new();
    }

    /// <summary>
    /// DTO pour l'export des commissions
    /// </summary>
    public class CommissionExportDto
    {
        public List<CommissionItemDto> Commissions { get; set; } = new();
        public CommissionDashboardDto Resume { get; set; } = new();
        public string DateGeneration { get; set; } = string.Empty;
        public string Periode { get; set; } = string.Empty;
        public string AgentNom { get; set; } = string.Empty;
        public string AgentMatricule { get; set; } = string.Empty;
    }
}
