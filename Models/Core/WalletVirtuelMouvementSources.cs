namespace ProsocAPI.Models.Core
{
    public static class WalletVirtuelMouvementSources
    {
        public const string AjoutSolde = "AJOUT_SOLDE";
        public const string AjustementSolde = "AJUSTEMENT_SOLDE";
        public const string CollecteCompteVirtuel = "COLLECTE_COMPTE_VIRTUEL";
        public const string Creation = "CREATION";
        /// <summary>Crédit float AT à la confirmation de perception terrain (max 30 car.).</summary>
        public const string RemisePerceptionVirtuelle = "REMISE_PERCEPTION_VIRTUELLE";
        /// <summary>Re-débit float AT à l'annulation de perception (max 30 car.).</summary>
        public const string AnnulRemisePerceptionVirtuelle = "ANNUL_REMISE_PERCEPTION_VIRT";
        /// <summary>Crédit jusqu'au plafond via demande de recharge (max 30 car.).</summary>
        public const string RechargePlafond = "RECHARGE_PLAFOND";

        public static string GetLibelle(string source) => source switch
        {
            AjoutSolde => "Recharge manuelle",
            Creation => "Solde initial",
            AjustementSolde => "Ajustement administratif",
            CollecteCompteVirtuel => "Paiement collecte compte virtuel",
            RemisePerceptionVirtuelle => "Remise perception virtuelle",
            AnnulRemisePerceptionVirtuelle => "Annulation remise perception",
            RechargePlafond => "Recharge jusqu'au plafond",
            _ => source
        };
    }
}
