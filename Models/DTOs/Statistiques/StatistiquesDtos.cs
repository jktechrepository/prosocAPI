namespace ProsocAPI.Models.DTOs.Statistiques
{
    public class StatistiquesGeneralesDto
    {
        public int TotalAffilies { get; set; }
        public int NombreObligationsMoisPrecedent { get; set; }
        public decimal TotalArrieres { get; set; }
        public decimal TotalCollectesMois { get; set; }
        public decimal TauxRecouvrement { get; set; }
        public int NombreCollectesMois { get; set; }
        public DateTime DateGeneration { get; set; } = DateTime.Now;
    }

    public class StatistiquesFinancieresDto
    {
        public decimal ChiffreAffaires { get; set; }
        public decimal MontantArrieres { get; set; }
        public decimal MontantPaye { get; set; }
        public decimal MontantDu { get; set; }
        public List<EvolutionMensuelleDto> EvolutionMensuelle { get; set; } = new();
        public List<RepartitionPaiementDto> RepartitionPaiements { get; set; } = new();
        public DateTime DateGeneration { get; set; } = DateTime.Now;
    }

    public class StatistiquesOperationnellesDto
    {
        public List<RepartitionAffilieParCategorieDto> RepartitionAffiliesParCategorie { get; set; } = new();
        public List<RepartitionAffilieParZoneDto> RepartitionAffiliesParZone { get; set; } = new();
        public List<StatistiqueObligationMoisDto> StatistiquesObligationsMois { get; set; } = new();
        public AffilieActiviteDto AffilieActivite { get; set; } = new();
        public DateTime DateGeneration { get; set; } = DateTime.Now;
    }

    public class StatistiquesPerformanceDto
    {
        public decimal TauxRecouvrementGlobal { get; set; }
        public List<TauxRecouvrementParCategorieDto> TauxRecouvrementParCategorie { get; set; } = new();
        public List<TopAgentDto> TopAgents { get; set; } = new();
        public List<StatistiquesPerformanceMensuelleDto> PerformanceMensuelle { get; set; } = new();
        public DateTime DateGeneration { get; set; } = DateTime.Now;
    }

    public class StatistiquesConsolideesDto
    {
        public StatistiquesGeneralesDto Generales { get; set; } = new();
        public StatistiquesFinancieresDto Financieres { get; set; } = new();
        public StatistiquesOperationnellesDto Operationnelles { get; set; } = new();
        public StatistiquesPerformanceDto Performance { get; set; } = new();
        public PeriodeStatistiquesDto Periode { get; set; } = new();
        public DateTime DateGeneration { get; set; } = DateTime.Now;
    }

    public class EvolutionMensuelleDto
    {
        public string Mois { get; set; } = string.Empty;
        public decimal MontantObligations { get; set; }
        public decimal MontantCollectes { get; set; }
        public decimal MontantArrieres { get; set; }
        public int NombreObligations { get; set; }
        public int NombreCollectes { get; set; }
    }

    public class RepartitionPaiementDto
    {
        public string ModePaiement { get; set; } = string.Empty;
        public decimal MontantTotal { get; set; }
        public int NombreCollectes { get; set; }
        public decimal Pourcentage { get; set; }
    }

    public class RepartitionAffilieParCategorieDto
    {
        public int CategorieAdhesionId { get; set; }
        public string NomCategorie { get; set; } = string.Empty;
        public int NombreAffilies { get; set; }
        public decimal Pourcentage { get; set; }
        public decimal MontantTotal { get; set; }
    }

    public class RepartitionAffilieParZoneDto
    {
        public int ZoneSocialeId { get; set; }
        public string NomZone { get; set; } = string.Empty;
        public string NomCommune { get; set; } = string.Empty;
        public int NombreAffilies { get; set; }
        public decimal Pourcentage { get; set; }
    }

    public class StatistiqueObligationMoisDto
    {
        public string Mois { get; set; } = string.Empty;
        public decimal MontantTotal { get; set; }
        public int NombreObligations { get; set; }
        public decimal MontantMoyen { get; set; }
    }

    public class AffilieActiviteDto
    {
        public int NombreAffiliesActifs { get; set; }
        public int NombreAffiliesInactifs { get; set; }
        public int TotalAffilies { get; set; }
        public decimal PourcentageActifs { get; set; }
        public decimal PourcentageInactifs { get; set; }
    }

    public class TauxRecouvrementParCategorieDto
    {
        public int CategorieAdhesionId { get; set; }
        public string NomCategorie { get; set; } = string.Empty;
        public decimal TauxRecouvrement { get; set; }
        public decimal MontantDu { get; set; }
        public decimal MontantPaye { get; set; }
    }

    public class TopAgentDto
    {
        public int IdAgent { get; set; }
        public string NomAgent { get; set; } = string.Empty;
        public string? RoleAgent { get; set; }
        public decimal MontantCollecte { get; set; }
        public int NombreCollectes { get; set; }
        public decimal TauxConversion { get; set; }
    }

    public class StatistiquesPerformanceMensuelleDto
    {
        public string Mois { get; set; } = string.Empty;
        public decimal TauxRecouvrement { get; set; }
        public decimal MontantCollecte { get; set; }
        public int NombreCollectes { get; set; }
        public decimal TicketMoyen { get; set; }
    }

    public class PeriodeStatistiquesDto
    {
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
        public string LibellePeriode { get; set; } = string.Empty;
    }
}
