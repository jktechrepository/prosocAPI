using ProsocAPI.Helpers;
using ProsocAPI.Models.Core;

namespace Prosoc.Tests.Unit.Helpers;

public class PerceptionOrigineHelperTests
{
    private static Collecte BaseCollecte(string mode, string statutPaiement = CollecteStatutPaiement.Valide) =>
        new()
        {
            Statut = true,
            ModePaiement = mode,
            StatutPaiement = statutPaiement,
            Montant = 100m
        };

    [Fact]
    public void IsOrigineAgent_VirtualAccountAvecDebit_RetourneTrue()
    {
        var collecte = BaseCollecte(MethodePaiementHelper.VirtualAccount);
        Assert.True(PerceptionOrigineHelper.IsOrigineAgent(collecte, hasDebitVirtuel: true));
    }

    [Fact]
    public void IsOrigineAgent_VirtualAccountSansDebit_RetourneFalse()
    {
        var collecte = BaseCollecte(MethodePaiementHelper.VirtualAccount);
        Assert.False(PerceptionOrigineHelper.IsOrigineAgent(collecte, hasDebitVirtuel: false));
    }

    [Fact]
    public void IsOrigineAffilie_EspeceValide_RetourneTrue()
    {
        var collecte = BaseCollecte(MethodePaiementHelper.Espece);
        Assert.True(PerceptionOrigineHelper.IsOrigineAffilie(collecte));
    }

    [Fact]
    public void IsOrigineAffilie_VirtualAccount_RetourneFalse()
    {
        var collecte = BaseCollecte(MethodePaiementHelper.VirtualAccount);
        Assert.False(PerceptionOrigineHelper.IsOrigineAffilie(collecte));
    }

    [Fact]
    public void ResolveStatutPerception_AgentNonPercu_RetourneEnAttente()
    {
        var collecte = BaseCollecte(MethodePaiementHelper.VirtualAccount);
        collecte.StatutPerception = CollecteStatutPerception.NonPerçu;

        Assert.Equal(
            PerceptionOrigineHelper.StatutEnAttente,
            PerceptionOrigineHelper.ResolveStatutPerception(collecte, isOrigineAgent: true));
    }

    [Fact]
    public void ResolveStatutPerception_Affilie_RetourneToujoursPercu()
    {
        var collecte = BaseCollecte(MethodePaiementHelper.Espece);
        collecte.StatutPerception = null;

        Assert.Equal(
            PerceptionOrigineHelper.StatutPercu,
            PerceptionOrigineHelper.ResolveStatutPerception(collecte, isOrigineAgent: false));
    }
}
