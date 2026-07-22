using System.ComponentModel.DataAnnotations;

namespace ProsocAPI.Models.DTOs.Core
{
    // KPIs spécifiques pour l'agent de terrain
    public class AgentKpisDto
    {
        public int TotalAffilies { get; set; }
        public int CollectesMois { get; set; }
        public decimal TotalCommissionsMois { get; set; }
        public decimal TotalCollectesMois { get; set; }
        /// <summary>Code ISO de la devise principale (ex. USD).</summary>
        public string? DevisePrincipaleCode { get; set; }
        public int NouvellesAdhesionsMois { get; set; }
        public int CollectesEnAttente { get; set; }
        public decimal TauxConversion { get; set; }
        public decimal MoyenneCollecte { get; set; }
        public decimal ObjectifMois { get; set; }
        public decimal ProgressionObjectif { get; set; }
    }

    // Performance personnelle de l'agent
    public class AgentPerformanceDto
    {
        public int AgentId { get; set; }
        public string AgentNom { get; set; } = string.Empty;
        public int TotalAffilies { get; set; }
        public decimal TotalCollectes { get; set; }
        public decimal TotalCommissions { get; set; }
        public int Classement { get; set; }
        public decimal ProgressionMois { get; set; }
        public decimal ProgressionAnnee { get; set; }
        public decimal TauxReussite { get; set; }
        
        // Propriétés additionnelles pour la compatibilité avec SuperviseurService
        public string NomAgent { get; set; } = string.Empty;
        public decimal MontantTotal { get; set; }
        public int NombreTransactions { get; set; }
        public decimal MontantMoyen { get; set; }
        public decimal TauxSucces { get; set; }
        public decimal PerformanceMoyenne { get; set; }
        public decimal ObjectifPersonnel { get; set; }
        public decimal AtteinteObjectif { get; set; }
        public int RangEquipe { get; set; }
        public decimal Progression { get; set; }
        public DateTime DerniereActivite { get; set; }
        public int NombreJoursActifs { get; set; }
        public decimal MontantCommissions { get; set; }
        public decimal NetAPercevoir { get; set; }
    }

    // Graphiques pour l'agent
    public class AgentGraphsDto
    {
        public List<MonthlyCollectionDto> CollectesMensuelles { get; set; } = new();
        public List<MonthlyAdhesionDto> AdhesionsMensuelles { get; set; } = new();
        public List<AgentCommissionGraphDto> CommissionsMensuelles { get; set; } = new();
        public List<PrestationStatsDto> RepartitionPrestations { get; set; } = new();
        public List<DailyActivityDto> ActiviteQuotidienne { get; set; } = new();
    }

    // Affiliés récents de l'agent
    public class AgentAffilieRecentDto
    {
        public int IdAffilie { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string Telephone { get; set; } = string.Empty;
        public DateTime DateAdhesion { get; set; }
        public string TypeAdhesion { get; set; } = string.Empty;
        public decimal DerniereCollecte { get; set; }
        public DateTime? DerniereCollecteDate { get; set; }
        public int NombreCollectes { get; set; }
        public decimal TotalCollectes { get; set; }
        public string StatutDossier { get; set; } = string.Empty;
    }

    // Collectes en attente pour l'agent
    public class AgentCollecteEnAttenteDto
    {
        public int IdCollecte { get; set; }
        public string NomAffilie { get; set; } = string.Empty;
        public string PrenomAffilie { get; set; } = string.Empty;
        public string TelephoneAffilie { get; set; } = string.Empty;
        public decimal Montant { get; set; }
        public string ReferencePaiement { get; set; } = string.Empty;
        public string ModePaiement { get; set; } = string.Empty;
        public DateTime DateCollecte { get; set; }
        public int HeuresAttente { get; set; }
        public int Priorite { get; set; }
        public string StatutPaiement { get; set; } = string.Empty;
    }

    // Commissions détaillées pour l'agent
    public class AgentCommissionDto
    {
        public DateTime Mois { get; set; }
        public decimal MontantCommission { get; set; }
        public decimal MontantCollectes { get; set; }
        public int NombreCollectes { get; set; }
        public decimal TauxCommission { get; set; }
        public decimal ObjectifMois { get; set; }
        public decimal AtteinteObjectif { get; set; }
    }

    // DTO pour les graphiques mensuels
    public class MonthlyCollectionDto
    {
        public string Mois { get; set; } = string.Empty;
        public decimal Montant { get; set; }
        public int NombreCollectes { get; set; }
        public decimal Moyenne { get; set; }
    }

    public class MonthlyAdhesionDto
    {
        public string Mois { get; set; } = string.Empty;
        public int NombreAdhesions { get; set; }
        public decimal ValeurTotale { get; set; }
    }

    public class AgentCommissionGraphDto
    {
        public string Mois { get; set; } = string.Empty;
        public decimal Montant { get; set; }
        public decimal Objectif { get; set; }
        public decimal Progression { get; set; }
    }

    public class PrestationStatsDto
    {
        public string NomPrestation { get; set; } = string.Empty;
        public int NombreSouscriptions { get; set; }
        public decimal MontantTotal { get; set; }
        public decimal Pourcentage { get; set; }
    }

    public class DailyActivityDto
    {
        public DateTime Date { get; set; }
        public int NombreVisites { get; set; }
        public int NombreAdhesions { get; set; }
        public int NombreCollectes { get; set; }
        public decimal MontantCollectes { get; set; }
    }

    /// <summary>Remarque 5/6 — Vue consolidée dashboard Agent de Terrain.</summary>
    public class AgentTerrainDashboardDto
    {
        public int AgentId { get; set; }
        public string NomAgent { get; set; } = string.Empty;
        public AgentKpisDto Kpis { get; set; } = new();
        public AgentPrimesResumeDto Primes { get; set; } = new();
        public AgentCommissionsResumeDto Commissions { get; set; } = new();
        public List<AgentSuiviAdherentDto> SuiviAdherents { get; set; } = new();
        public List<AgentAffilieRecentDto> AffiliesRecents { get; set; } = new();
        public List<AgentCollecteEnAttenteDto> CollectesEnAttente { get; set; } = new();
        public AgentObjectifDto? Objectifs { get; set; }
        public DateTime DateGeneration { get; set; }
        /// <summary>Code ISO de la devise principale (ex. USD) pour les montants consolidés.</summary>
        public string? DevisePrincipaleCode { get; set; }
    }

    public class AgentPrimesResumeDto
    {
        public decimal TotalPrimesMois { get; set; }
        public decimal TotalPrimesAssuranceMois { get; set; }
        public decimal TotalPrimesMutuelleMois { get; set; }
        public int NombreSouscriptionsMois { get; set; }
        public List<AgentPrimeDetailDto> Details { get; set; } = new();
    }

    public class AgentPrimeDetailDto
    {
        public int IdCollecte { get; set; }
        public int AffilieId { get; set; }
        public string NomAffilie { get; set; } = string.Empty;
        public string NomProduit { get; set; } = string.Empty;
        public string TypeProduit { get; set; } = string.Empty;
        public decimal MontantPrime { get; set; }
        public decimal? CommissionEstimee { get; set; }
        public DateTime DateCollecte { get; set; }
        public string StatutPaiement { get; set; } = string.Empty;
    }

    public class AgentCommissionsResumeDto
    {
        public decimal SoldeWallet { get; set; }
        public decimal TotalCommissionsMois { get; set; }
        public decimal TotalCommissionsAnnee { get; set; }
        public int NombreMouvementsMois { get; set; }
        public List<AgentCommissionMouvementDto> MouvementsRecents { get; set; } = new();
    }

    public class AgentCommissionMouvementDto
    {
        public int IdWalletMouvement { get; set; }
        public decimal Montant { get; set; }
        public string Source { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime DateOperation { get; set; }
        public string? NomAffilie { get; set; }
        public decimal? MontantCollecteLiee { get; set; }
    }

    public class AgentSuiviAdherentDto
    {
        public int IdAffilie { get; set; }
        public int IdAdhesion { get; set; }
        public string CodeAdhesion { get; set; } = string.Empty;
        public string NomComplet { get; set; } = string.Empty;
        public string? Telephone { get; set; }
        public DateTime DateAdhesion { get; set; }
        public string StatutDossier { get; set; } = string.Empty;
        public string TypeAdhesion { get; set; } = string.Empty;
        public bool CotisationAJour { get; set; }
        public decimal TotalCollectes { get; set; }
        public int NombrePrimes { get; set; }
        public DateTime? DerniereActivite { get; set; }
        public string? Alerte { get; set; }
        public string StatutGlobal { get; set; } = AffilieConformiteStatuts.EnOrdre;
        public string StatutCotisation { get; set; } = AffilieConformiteStatuts.EnOrdre;
        public string StatutPrestation { get; set; } = AffilieConformiteStatuts.EnOrdre;
        public int NombreArrieresOuverts { get; set; }
        public decimal MontantRestantDu { get; set; }
    }

    // DTO pour les objectifs
    public class AgentObjectifDto
    {
        public int Mois { get; set; }
        public int Annee { get; set; }
        public decimal ObjectifCollectes { get; set; }
        public decimal ObjectifAdhesions { get; set; }
        public decimal ObjectifCommissions { get; set; }
        public decimal ProgressionCollectes { get; set; }
        public decimal ProgressionAdhesions { get; set; }
        public decimal ProgressionCommissions { get; set; }
    }
}
