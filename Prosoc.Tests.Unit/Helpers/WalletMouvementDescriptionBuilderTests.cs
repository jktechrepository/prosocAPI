using ProsocAPI.Models.Core;
using Prosoc.Utilities;
using Xunit;

namespace Prosoc.Tests.Unit.Helpers;

public class WalletMouvementDescriptionBuilderTests
{
    [Theory]
    [InlineData("Commission collecte #248 - Affilie 78", 248)]
    [InlineData("Commission collecte — Jean Mukendi (n° 248)", 248)]
    [InlineData("Commission collecte — Jean Mukendi (n° 248, converti en USD)", 248)]
    public void TryExtractCollecteId_SupporteFormatsLegacyEtLisible(string description, int expectedId)
    {
        var id = WalletMouvementDescriptionBuilder.TryExtractCollecteId(description);
        Assert.Equal(expectedId, id);
    }

    [Fact]
    public void BuildDisplayDescription_AvecCollecte_RetourneLibelleLisible()
    {
        var mouvement = new WalletMouvement
        {
            Source = WalletMouvementSources.CommissionCollecte,
            Description = "Commission collecte #248 - Affilie 78"
        };
        var collecte = new Collecte
        {
            IdCollecte = 248,
            AffilieId = 78,
            Affilie = new Affilie { NomComplet = "Jean Mukendi" }
        };

        var description = WalletMouvementDescriptionBuilder.BuildDisplayDescription(mouvement, collecte);

        Assert.Equal("Commission collecte — Jean Mukendi (n° 248)", description);
    }

    [Fact]
    public void BuildDisplayDescription_AvecConversion_ConserveSuffixeDevise()
    {
        var mouvement = new WalletMouvement
        {
            Source = WalletMouvementSources.CommissionCollecte,
            Description = "Commission collecte #248 - Affilie 78 (converti en USD)"
        };
        var collecte = new Collecte
        {
            IdCollecte = 248,
            AffilieId = 78,
            Affilie = new Affilie { NomComplet = "Jean Mukendi" }
        };

        var description = WalletMouvementDescriptionBuilder.BuildDisplayDescription(mouvement, collecte);

        Assert.Equal("Commission collecte — Jean Mukendi (n° 248, converti en USD)", description);
    }

    [Fact]
    public void BuildDisplayDescription_SansCollecte_ConserveDescriptionBrute()
    {
        var mouvement = new WalletMouvement
        {
            Source = WalletMouvementSources.CommissionCollecte,
            Description = "Commission collecte #999 - Affilie 1"
        };

        var description = WalletMouvementDescriptionBuilder.BuildDisplayDescription(mouvement, null);

        Assert.Equal("Commission collecte #999 - Affilie 1", description);
    }

    [Fact]
    public void BuildDisplayDescription_AutreSource_ConserveDescription()
    {
        var mouvement = new WalletMouvement
        {
            Source = WalletMouvementSources.RetraitJeton,
            Description = "Paiement retrait agent — jeton ABC123"
        };

        var description = WalletMouvementDescriptionBuilder.BuildDisplayDescription(mouvement, null);

        Assert.Equal("Paiement retrait agent — jeton ABC123", description);
    }
}
