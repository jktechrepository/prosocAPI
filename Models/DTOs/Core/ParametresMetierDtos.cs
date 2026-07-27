namespace ProsocAPI.Models.DTOs.Core
{
    public class RetraitAgentParametresReadDto
    {
        public int Fenetre1Debut { get; set; }
        public int Fenetre1Fin { get; set; }
        public int Fenetre2DerniersJours { get; set; }
        public decimal MontantMinimumPartiel { get; set; }
        public bool ExpirationAutomatiqueActivee { get; set; }
        public int IntervalleExpirationMinutes { get; set; }
        public DateTime? DateModification { get; set; }
        public int? ModifieParUtilisateurId { get; set; }
        public string? ModifieParNom { get; set; }
    }

    public class RetraitAgentParametresUpdateDto
    {
        public int Fenetre1Debut { get; set; }
        public int Fenetre1Fin { get; set; }
        public int Fenetre2DerniersJours { get; set; }
        public decimal MontantMinimumPartiel { get; set; }
    }

    public class AgentMaashParametresReadDto
    {
        public decimal MontantRetenueUsd { get; set; }
        public int DeviseId { get; set; }
        public string[] CodesCategoriesEligibles { get; set; } = Array.Empty<string>();
        public string NomProduitMaash { get; set; } = string.Empty;
        public bool RetenueAutomatiqueActivee { get; set; }
        public int JourExecution { get; set; }
        public int HeureExecution { get; set; }
        public int IntervalleControleMinutes { get; set; }
        public bool RetenterEchecsQuotidiennement { get; set; }
        public DateTime? DateModification { get; set; }
        public int? ModifieParUtilisateurId { get; set; }
        public string? ModifieParNom { get; set; }
    }

    public class AgentMaashParametresUpdateDto
    {
        public decimal MontantRetenueUsd { get; set; }
        public int DeviseId { get; set; }
        public string[] CodesCategoriesEligibles { get; set; } = Array.Empty<string>();
        public string NomProduitMaash { get; set; } = string.Empty;
        public bool RetenueAutomatiqueActivee { get; set; }
        public int JourExecution { get; set; }
        public int HeureExecution { get; set; }
        public int IntervalleControleMinutes { get; set; }
        public bool RetenterEchecsQuotidiennement { get; set; }
    }

    public class ArrieresParametresReadDto
    {
        public bool GenerationAutomatiqueActivee { get; set; }
        public int HeureExecution { get; set; }
        public int MinuteExecution { get; set; }
        public int IntervalleControleMinutes { get; set; }
        public int JourEcheanceMensuelle { get; set; }
        public DateTime? DateModification { get; set; }
        public int? ModifieParUtilisateurId { get; set; }
        public string? ModifieParNom { get; set; }
    }

    public class ArrieresParametresUpdateDto
    {
        public bool GenerationAutomatiqueActivee { get; set; }
        public int HeureExecution { get; set; }
        public int MinuteExecution { get; set; }
        public int IntervalleControleMinutes { get; set; }
        public int JourEcheanceMensuelle { get; set; }
    }

    public class PenaliteParametresReadDto
    {
        public bool ApplicationAutomatiqueActivee { get; set; }
        public int DelaiGraceJours { get; set; }
        public string FraisPenaliteCode { get; set; } = string.Empty;
        public bool RetardCotisationActive { get; set; }
        public DateTime? DateModification { get; set; }
        public int? ModifieParUtilisateurId { get; set; }
        public string? ModifieParNom { get; set; }
    }

    public class PenaliteParametresUpdateDto
    {
        public bool ApplicationAutomatiqueActivee { get; set; }
        public int DelaiGraceJours { get; set; }
        public string FraisPenaliteCode { get; set; } = string.Empty;
        public bool RetardCotisationActive { get; set; }
    }
}
