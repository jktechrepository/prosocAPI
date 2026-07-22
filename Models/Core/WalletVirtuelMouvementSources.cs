namespace ProsocAPI.Models.Core
{
    public static class WalletVirtuelMouvementSources
    {
        public const string AjoutSolde = "AJOUT_SOLDE";
        public const string AjustementSolde = "AJUSTEMENT_SOLDE";
        public const string CollecteCompteVirtuel = "COLLECTE_COMPTE_VIRTUEL";
        public const string Creation = "CREATION";

        public static string GetLibelle(string source) => source switch
        {
            AjoutSolde => "Recharge manuelle",
            Creation => "Solde initial",
            AjustementSolde => "Ajustement administratif",
            CollecteCompteVirtuel => "Paiement collecte compte virtuel",
            _ => source
        };
    }
}
