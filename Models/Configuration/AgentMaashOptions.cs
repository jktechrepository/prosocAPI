namespace ProsocAPI.Models.Configuration
{
    public class AgentMaashOptions
    {
        public const string SectionName = "AgentMaash";

        public decimal MontantRetenueUsd { get; set; } = 5m;

        public int DeviseId { get; set; } = 2;

        /// <summary>Codes CategorieAgent éligibles (AT, AA, …).</summary>
        public string[] CodesCategoriesEligibles { get; set; } =
            { "AT", "AA", "AP", "AS", "CA", "FI", "IT", "AD" };

        public string NomProduitMaash { get; set; } = "MAASH";

        /// <summary>Active le traitement automatique en arrière-plan.</summary>
        public bool RetenueAutomatiqueActivee { get; set; } = true;

        /// <summary>Jour du mois (1–28) à partir duquel la retenue peut être prélevée.</summary>
        public int JourExecution { get; set; } = 1;

        /// <summary>Heure locale (0–23) minimale pour lancer le traitement.</summary>
        public int HeureExecution { get; set; } = 2;

        /// <summary>Fréquence de vérification du planificateur (minutes).</summary>
        public int IntervalleControleMinutes { get; set; } = 60;

        /// <summary>Si true, retente chaque jour (à partir de JourExecution) les agents non prélevés (ex. solde insuffisant).</summary>
        public bool RetenterEchecsQuotidiennement { get; set; } = true;
    }
}
