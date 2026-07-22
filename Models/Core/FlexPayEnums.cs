namespace ProsocAPI.Models.Core
{
    public enum CollecteEnAttenteSourceFlux
    {
        CollecteAgent = 1,
        PaiementAffilie = 2,
        AdhesionWithAffilie = 3,
        CollectePaiementElectroniquePublic = 4,
        SouscriptionAchatPaiementElectronique = 5
    }

    public enum CollecteEnAttenteStatut
    {
        EnAttente = 1,
        Finalise = 2,
        Echec = 3,
        Expire = 4
    }
}
