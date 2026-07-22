namespace ProsocAPI.Models.DTOs.Core
{
    public class AffilieConformiteDto
    {
        public int AffilieId { get; set; }
        public string? CodeAdhesion { get; set; }
        public string? NomComplet { get; set; }
        public int? AgentId { get; set; }
        public string StatutGlobal { get; set; } = AffilieConformiteStatuts.EnOrdre;
        public string StatutCotisation { get; set; } = AffilieConformiteStatuts.EnOrdre;
        public string StatutPrestation { get; set; } = AffilieConformiteStatuts.EnOrdre;
        public int NombreArrieresOuverts { get; set; }
        public decimal MontantRestantDu { get; set; }
        public List<ArriereOuvertDto> ArrieresOuverts { get; set; } = new();
        public DateTime DateCalcul { get; set; }
    }

    public class ArriereOuvertDto
    {
        public int IdArrieresAffilie { get; set; }
        public string TypeObligation { get; set; } = string.Empty;
        public string Periode { get; set; } = string.Empty;
        public decimal MontantRestant { get; set; }
        public string StatutPaiement { get; set; } = string.Empty;
        public string? Libelle { get; set; }
        public DateTime DateEcheance { get; set; }
    }

    public class AffilieConformiteFiltreDto
    {
        public string? StatutGlobal { get; set; }
        public string? StatutCotisation { get; set; }
        public string? StatutPrestation { get; set; }
        public int? AgentId { get; set; }
        public string? Search { get; set; }
    }

    public static class AffilieConformiteStatuts
    {
        public const string EnOrdre = "EN_ORDRE";
        public const string HorsOrdre = "HORS_ORDRE";
    }
}
