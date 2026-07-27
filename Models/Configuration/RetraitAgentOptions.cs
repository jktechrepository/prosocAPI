namespace ProsocAPI.Models.Configuration
{
    public class RetraitAgentOptions
    {
        public const string SectionName = "RetraitAgent";

        /// <summary>Premier jour de la 1ère fenêtre autorisée (inclus).</summary>
        public int Fenetre1Debut { get; set; } = 15;

        /// <summary>Dernier jour de la 1ère fenêtre autorisée (inclus).</summary>
        public int Fenetre1Fin { get; set; } = 20;

        /// <summary>Nombre de derniers jours du mois constituant la 2ème fenêtre.</summary>
        public int Fenetre2DerniersJours { get; set; } = 7;

        /// <summary>Montant minimum pour un retrait PARTIEL (devise principale).</summary>
        public decimal MontantMinimumPartiel { get; set; } = 5m;

        /// <summary>Active le job d'expiration automatique des jetons de retrait.</summary>
        public bool ExpirationAutomatiqueActivee { get; set; } = true;

        /// <summary>Intervalle entre deux passages du job d'expiration (minutes, min 1).</summary>
        public int IntervalleExpirationMinutes { get; set; } = 15;
    }
}
